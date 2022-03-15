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
    public HexCell landA;
    public HexCell landB;
    public WaypointMarker waypointMarker;
    public Spawner spawner;
    public MainUI mainUI;
    public Text cellLabelPrefab;
    public MapCreation mapCreation;
    public TimeController timeController;
    [SerializeField] TextAsset unsafeCells;
    [SerializeField] TextAsset escapeCells;
    [SerializeField] TextAsset adjacentChecks;
    [SerializeField] TextAsset objectives;
    [SerializeField] TextAsset upgrades;
    [SerializeField] AudioManager audioManager;

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
    List<WaypointMarker> waypoints = new List<WaypointMarker>();
    HexCell rangerStation;
    PlayerBehaviour playerBehaviour;
    int patrolBoatSpawn, serviceBoatSpawn;

    void Awake()
    {
        playerBehaviour = this.gameObject.GetComponent<PlayerBehaviour>();
        GlobalCellCheck.SetUnsafeCells(unsafeCells);
        GlobalCellCheck.SetEscapeCells(escapeCells);
        GlobalCellCheck.SetAdjacentChecks(adjacentChecks);

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
        CreateChunks();
        CreateCells();
        PopulateUpgradeCells();

        spawner.SpawnUnit(cells[patrolBoatSpawn], "Tier 1 Patrol Boat");
        spawner.SpawnUnit(cells[serviceBoatSpawn], "Service Boat");

        TextRW.SetObjectives(objectives);
        TextRW.SetUpgrades(upgrades);
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

        // for adding the images
        for (int i = 0; i < cells.Length; i++)
        {
            switch (GlobalCellCheck.IsAdjacentToBuoy(cells[i]))
            {
                case 1:
                    cells[i].Adjacency = 1;
                    break;
                case 2:
                    cells[i].Adjacency = 2;
                    break;
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
            // int treeChance = Random.Range(0, 101);
            // if (treeChance < 85)
            // {
            //     cell = cells[i] = Instantiate<HexCell>(landA);
            // }
            // else
            // {
            //     cell = cells[i] = Instantiate<HexCell>(landB);
            // }

            if (i == 465)
            {
                cell = cells[i] = Instantiate<HexCell>(landB);
            }
            else
            {
                cell = cells[i] = Instantiate<HexCell>(landA);
            }
            cell.Type = "Land";
        }
        else
        {
            cell = cells[i] = Instantiate<HexCell>(water);
            cell.Type = "Water";

            // Randomise waves
            Animator waves = cell.gameObject.GetComponent<Animator>();
            waves.SetFloat("Offset", Random.Range(0, 0.25f));
        }

        cell.transform.localPosition = position;
        cell.transform.localScale = new Vector3(17.2f, 5.0f, 17.2f);
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

        // add units *after* the cells have spawned
        switch (mapCreation.HasInitialUnit(check))
        {
            case 0:
                patrolBoatSpawn = i;
                break;
            case 1:
                serviceBoatSpawn = i;
                break;
        }

        switch (mapCreation.HasStructure(check))
        {
            case 0:
                spawner.SpawnStructure(cell, "Ranger Station");
                rangerStation = cell;
                break;
            case 1:
                spawner.SpawnStructure(cell, "Buoy");
                buoyCells.Add(cell);
                cell.EnableHighlight(Color.green);
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

        // Should only matter for players
        unit.Grid = this;
        unit.VisionRange = 10; // This is just default vision range; it's only for players

        unit.UnitType = unitType;
        unit.Location = location;
        unit.Orientation = orientation;
        unit.ActionPoints = actionPoints;
        unit.transform.Translate(Vector3.up * 0.8f);

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
        location.Structure = structure;
        structure.transform.SetParent(transform, false);
        structure.Location = location;
        structure.Orientation = orientation;
        structure.StructureType = structureType;
        structure.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        structure.transform.Translate(Vector3.up * 0.8f);
    }

    public void AddUpgrade(Upgrade upgrade, HexCell location, float orientation, string upgradeType, int constructionTime, int researchCost, int constructionCost, int upkeep)
    {
        structures.Add(upgrade);
        location.Upgrade = upgrade;
        upgrade.transform.SetParent(transform, false);
        upgrade.Location = location;
        upgrade.Orientation = orientation;
        upgrade.UpgradeType = upgradeType;

        upgrade.SetBuildTime(constructionTime);
        upgrade.SetResearchCost(researchCost);
        upgrade.SetBuildCost(constructionCost);
        upgrade.SetUpkeep(upkeep);
    }

    public void RemoveUpgrade(Upgrade upgrade)
    {
        structures.Remove(upgrade);
        upgrade.Die();
    }

    public void AddWaypoint(WaypointMarker waypointMarker, Transform target)
    {
        waypoints.Add(waypointMarker);
        waypointMarker.target = target;
        waypointMarker.transform.SetParent(mainUI.transform);
    }

    public void RemoveWaypoint(WaypointMarker waypointMarker)
    {
        waypoints.Remove(waypointMarker);
        waypointMarker.Die();
    }

    public void RemoveWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            RemoveWaypoint(waypoints[i]);
        }
        waypoints.Clear();
    }

    public WaypointMarker FindWaypoint(HexUnit unit)
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i].target == unit.transform)
            {
                return waypoints[i];
            }
        }
        return null;
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

    List<HexCell> GetVisibleCells(HexCell fromCell, int range)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Distance = int.MaxValue;
        }

        List<HexCell> frontier = new List<HexCell>();
        List<HexCell> visibleCells = new List<HexCell>();
        fromCell.Distance = 0;
        frontier.Add(fromCell);
        while (frontier.Count > 0)
        {
            HexCell current = frontier[0];
            frontier.RemoveAt(0);
            visibleCells.Add(current);

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);

                if (neighbor == null || neighbor.Distance != int.MaxValue)
                {
                    continue;
                }
                if (GlobalCellCheck.IsImpassable(neighbor))
                {
                    continue;
                }

                int distance = current.Distance + 1;
                if (distance > range)
                {
                    continue;
                }

                neighbor.Distance = distance;
                neighbor.SearchHeuristic = 0;
                frontier.Add(neighbor);
                frontier.Sort((x, y) => x.SearchPriority.CompareTo(y.SearchPriority));
            }
        }
        return visibleCells;
    }

    public void IncreaseVisibility(HexCell fromCell, int range)
    {
        List<HexCell> curr = GetVisibleCells(fromCell, range);
        for (int i = 0; i < curr.Count; i++)
        {
            cells[i].IncreaseVisibility();
            if (cells[i].Unit)
            {
                if (!cells[i].Unit.IsVisible)
                {
                    cells[i].Unit.ToggleVisibility();
                }
            }
        }
    }

    public void DecreaseVisibility(HexCell fromCell, int range)
    {
        List<HexCell> curr = GetVisibleCells(fromCell, range);
        for (int i = 0; i < curr.Count; i++)
        {
            cells[i].DecreaseVisibility();

            if (cells[i].Unit)
            {
                if (cells[i].Unit.IsVisible)
                {
                    cells[i].Unit.ToggleVisibility();
                }
            }
        }
    }

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

    public AudioManager GetAudioManager()
    {
        return audioManager;
    }

    public WaypointMarker GetWaypointMarker()
    {
        return waypointMarker;
    }
}