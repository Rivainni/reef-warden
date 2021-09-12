using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public int chunkCountX = 6, chunkCountZ = 4;
    public HexGridChunk chunkPrefab;
    int cellCountX, cellCountZ;
    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;
    public HexCell water;
    public HexCell land;
    public Text cellLabelPrefab;
    public State initState;

    HexGridChunk[] chunks;
    HexCell[] cells;

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

        if (initState.IsLand(new Vector2(computed.X, computed.Z)))
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
        cell.transform.localScale = new Vector3(17.0f, 1.0f, 17.0f);
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
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;

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

    public void FindPath(HexCell fromCell, HexCell toCell)
    {
        StopAllCoroutines();
        StartCoroutine(Search(fromCell, toCell));
    }

    IEnumerator Search(HexCell fromCell, HexCell toCell)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Distance = int.MaxValue;
            cells[i].DisableHighlight();
        }
        fromCell.EnableHighlight(Color.blue);
        toCell.EnableHighlight(Color.red);

        WaitForSeconds delay = new WaitForSeconds(1 / 60f);
        List<HexCell> frontier = new List<HexCell>();
        fromCell.Distance = 0;
        frontier.Add(fromCell);
        while (frontier.Count > 0)
        {
            yield return delay;
            HexCell current = frontier[0];
            frontier.RemoveAt(0);

            if (current == toCell)
            {
                current = current.PathFrom;
                while (current != fromCell)
                {
                    current.EnableHighlight(Color.white);
                    current = current.PathFrom;
                }
                break;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.Distance != int.MaxValue)
                {
                    continue;
                }
                if (neighbor.IsImpassable)
                {
                    continue;
                }
                neighbor.Distance = current.Distance + 1;
                neighbor.PathFrom = current;
                neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                frontier.Add(neighbor);
                frontier.Sort((x, y) => x.SearchPriority.CompareTo(y.SearchPriority));
            }
        }
    }
}