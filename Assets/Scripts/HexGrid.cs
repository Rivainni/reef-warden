using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class HexGrid : MonoBehaviour
{
    public int chunkCountX, chunkCountZ;
    public HexGridChunk chunkPrefab;
    int cellCountX, cellCountZ;
    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;
    public HexCell water;
    public HexCell land;
    public Spawner spawner;
    public MainUI mainUI;
    public Text cellLabelPrefab;
    public MapCreation mapCreation;
    [SerializeField] TextAsset unsafeCells;
    [SerializeField] TextAsset escapeCells;

    public bool HasPath
    {
        get
        {
            return currentPathExists;
        }
        set
        {
            currentPathExists = value;
        }
    }
    bool currentPathExists;
    HexGridChunk[] chunks;
    HexCell[] cells;
    List<HexUnit> units = new List<HexUnit>();
    List<HexStructure> structures = new List<HexStructure>();
    List<HexCell> upgradeCells = new List<HexCell>();
    List<HexCell> buoyCells = new List<HexCell>();
    HexCell rangerStation;
    PlayerBehaviour playerBehaviour;

    void Awake()
    {
        playerBehaviour = this.gameObject.GetComponent<PlayerBehaviour>();
        GlobalCellCheck.SetUnsafeCells(unsafeCells);
        GlobalCellCheck.SetEscapeCells(escapeCells);

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
        CreateChunks();
        CreateCells();
        PopulateUpgradeCells();

        if (SceneManager.GetActiveScene().name == "Map")
        {
            spawner.RandomSpawn("Tourist Boat");
            spawner.RandomSpawn("Fishing Boat");
        }
    }

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }
    void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    // Create cell using metrics and assign it to its respective chunk.
    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell;
        HexCoordinates computed = HexCoordinates.FromOffsetCoordinates(x, z);
        Vector2 check = new Vector2(computed.X, computed.Z);

        if (mapCreation.IsLand(check))
        {
            cell = cells[i] = Instantiate<HexCell>(land);
            cell.Type = "Land";
        }
        else
        {
            cell = cells[i] = Instantiate<HexCell>(water);
            cell.Type = "Water";
        }

        cell.transform.localPosition = position;
        cell.transform.localScale = new Vector3(17.2f, 1.0f, 17.2f);
        // cell.transform.localScale = new Vector3(17.2f, 17.2f, 17.2f);
        cell.coordinates = computed;
        cell.HasOverlap = false;
        cell.Index = i;

        // Set neighbours
        // x = 0 has no neighbours west. We start with west neighbours.
        // Note that setting neighbours does it for the opposite direction as well.
        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }

        // Even numbered rows except 0 has an SE neighbour.
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        cell.uiRect = label.rectTransform;

        // add units *after* the cell has spawned
        switch (mapCreation.HasInitialUnit(check))
        {
            case 0:
                spawner.SpawnUnit(cell, "Tier 1 Patrol Boat");
                Debug.Log("P1 spawned.");
                break;
            case 1:
                spawner.SpawnUnit(cell, "Service Boat");
                Debug.Log("S spawned.");
                break;
        }

        switch (mapCreation.HasStructure(check))
        {
            case 0:
                spawner.SpawnStructure(cell, "Ranger Station");
                Debug.Log("RS spawned.");
                rangerStation = cell;
                break;
            case 1:
                spawner.SpawnStructure(cell, "Buoy");
                Debug.Log("B spawned.");
                buoyCells.Add(cell);
                break;
        }

        AddCellToChunk(x, z, cell);
    }

    void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[index];
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;
        int x = coordinates.X + z / 2;
        return cells[x + z * cellCountX];
    }

    public void ShowUI(bool visible)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }

    void ClearUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Die();
        }
        units.Clear();
    }

    public void AddUnit(HexUnit unit, HexCell location, float orientation, string unitType, int actionPoints)
    {
        units.Add(unit);
        unit.transform.SetParent(transform, false);

        if (unitType != "Fishing Boat")
        {
            unit.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        }

        unit.Location = location;
        unit.Orientation = orientation;
        unit.UnitType = unitType;
        unit.ActionPoints = actionPoints;

        if (unitType == "Fishing Boat" || unitType == "Tourist Boat")
        {
            AIBehaviour currentBehaviour = unit.gameObject.GetComponent<AIBehaviour>();
            currentBehaviour.grid = this;
            currentBehaviour.mainUI = mainUI;
            currentBehaviour.spawner = spawner;
        }
    }

    public void RemoveUnit(HexUnit unit)
    {
        units.Remove(unit);
        unit.Die();
    }

    public void AddStructure(HexStructure structure, HexCell location, float orientation, string structureType)
    {
        structures.Add(structure);
        structure.transform.SetParent(transform, false);
        structure.Location = location;
        structure.Orientation = orientation;
        structure.StructureType = structureType;
    }

    public void AddUpgrade(Upgrade upgrade, HexCell location, float orientation, string upgradeType, int constructionTime, int researchCost, int constructionCost)
    {
        structures.Add(upgrade);
        upgrade.transform.SetParent(transform, false);
        upgrade.Location = location;
        upgrade.Orientation = orientation;
        upgrade.UpgradeType = upgradeType;

        upgrade.SetBuildTime(constructionTime);
        upgrade.SetResearchCost(researchCost);
        upgrade.SetBuildCost(constructionCost);
    }

    public void ResetPoints()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].ResetMovement();
        }
    }

    void PopulateUpgradeCells()
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell eligible = rangerStation.GetNeighbor(d);
            upgradeCells.Add(eligible);
        }
    }

    public bool CheckUpgradeCell(HexCell upgrade)
    {
        if (upgradeCells.Contains(upgrade))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // public void Save(BinaryWriter writer)
    // {
    //     writer.Write(cellCountX);
    //     writer.Write(cellCountZ);

    //     writer.Write(units.Count);
    //     for (int i = 0; i < cells.Length; i++)
    //     {
    //         cells[i].Save(writer);
    //     }
    // }

    // public void Load(BinaryReader reader)
    // {
    //     ClearPath();
    //     ClearUnits();
    //     for (int i = 0; i < cells.Length; i++)
    //     {
    //         cells[i].Load(reader);
    //     }

    //     int unitCount = reader.ReadInt32();
    //     for (int i = 0; i < unitCount; i++)
    //     {
    //         HexUnit.Load(reader, this);
    //     }
    // }

    public HexCell GetCell(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return GetCell(hit.point);
        }
        return null;
    }

    public HexCell[] GetCells()
    {
        return cells;
    }

    public List<HexCell> GetBuoyCells()
    {
        return buoyCells;
    }

    public List<HexUnit> GetUnits()
    {
        return units;
    }

    public PlayerBehaviour GetPlayerBehaviour()
    {
        return playerBehaviour;
    }
}