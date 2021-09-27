using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public UnitSpawner unitSpawner;
    public Text cellLabelPrefab;
    public State initState;

    public bool HasPath
    {
        get
        {
            return currentPathExists;
        }
    }

    HexCell currentPathFrom, currentPathTo;
    bool currentPathExists;
    HexGridChunk[] chunks;
    HexCell[] cells;
    List<HexUnit> units = new List<HexUnit>();
    List<HexStructure> structures = new List<HexStructure>();

    void Awake()
    {
        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
        CreateChunks();
        CreateCells();
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

        // commented out since this was for flat top (I decided to go for point top instead because it's more intuitive)
        // position.x = x * (HexMetrics.outerRadius * 1.5f);
        // position.y = 0f;
        // position.z = (z + x * 0.5f - x / 2) * (HexMetrics.innerRadius * 2f);

        HexCell cell;
        HexCoordinates computed = HexCoordinates.FromOffsetCoordinates(x, z);
        Vector2 check = new Vector2(computed.X, computed.Z);

        if (initState.IsLand(check))
        {
            cell = cells[i] = Instantiate<HexCell>(land);
            cell.IsImpassable = true;
        }
        else
        {
            cell = cells[i] = Instantiate<HexCell>(water);
            cell.IsImpassable = false;
        }

        cell.transform.localPosition = position;
        cell.transform.localScale = new Vector3(17.2f, 1.0f, 17.2f);
        cell.coordinates = computed;
        // cell.Color = defaultColor;

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
        switch (initState.HasInitialUnit(check))
        {
            case 0:
                unitSpawner.SpawnUnit(cell, "Tier 1 Patrol Boat");
                Debug.Log("P1 spawned.");
                break;
            case 1:
                unitSpawner.SpawnUnit(cell, "Service Boat");
                Debug.Log("S spawned.");
                break;
        }

        switch (initState.HasStructure(check))
        {
            case 0:
                unitSpawner.SpawnStructure(cell, "Ranger Station");
                Debug.Log("RS spawned.");
                break;
            case 1:
                unitSpawner.SpawnStructure(cell, "Buoy");
                Debug.Log("B spawned.");
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

    public void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists = Search(fromCell, toCell, speed);
        ShowPath(speed);
    }

    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Distance = int.MaxValue;
        }

        List<HexCell> frontier = new List<HexCell>();
        fromCell.Distance = 0;
        frontier.Add(fromCell);
        while (frontier.Count > 0)
        {
            HexCell current = frontier[0];
            frontier.RemoveAt(0);

            if (current == toCell)
            {
                return true;
            }

            int currentTurn = (current.Distance - 1) / speed;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                // We didn't plan to have variable move costs (e.g. certain parts of the map have you move faster), but we do have variable "speeds"
                int moveCost = 1;
                if (neighbor == null || neighbor.Distance != int.MaxValue)
                {
                    continue;
                }
                if (neighbor.IsImpassable || neighbor.Unit)
                {
                    continue;
                }

                int distance = current.Distance + moveCost;
                neighbor.Distance = distance;
                neighbor.PathFrom = current;
                neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                frontier.Add(neighbor);
                frontier.Sort((x, y) => x.SearchPriority.CompareTo(y.SearchPriority));
            }
        }
        return false;
    }

    void ShowPath(int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                int turn = (current.Distance - 1) / speed;
                current.SetLabel((turn + 1).ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
        }
        currentPathFrom.EnableHighlight(Color.blue);
        currentPathTo.EnableHighlight(Color.red);
    }

    public int WithinTurnPath(int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            if (current.Distance <= speed)
            {
                return speed - current.Distance;
            }
            else
            {
                return int.MaxValue;
            }
        }
        return int.MaxValue;
    }

    public void ClearPath()
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.DisableHighlight();
            currentPathExists = false;
        }
        else if (currentPathFrom)
        {
            currentPathFrom.DisableHighlight();
            currentPathTo.DisableHighlight();
        }
        currentPathFrom = currentPathTo = null;
    }
    public List<HexCell> GetPath()
    {
        if (!currentPathExists)
        {
            return null;
        }
        List<HexCell> path = new List<HexCell>(); // ListPool is only available in 2021 oof
        for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
        {
            path.Add(c);
        }
        path.Add(currentPathFrom);
        path.Reverse();
        return path;
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
        unit.Location = location;
        unit.Orientation = orientation;
        unit.UnitType = unitType;
        unit.ActionPoints = actionPoints;
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

    public void ResetPoints()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].ResetMovement();
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
}