using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerBehaviour : MonoBehaviour
{
    public HexGrid grid;
    HexCell currentPathFrom, currentPathTo;

    public void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        if (grid.HasPath)
        {
            ClearPath();
        }
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        grid.HasPath = Search(fromCell, toCell, speed);
        ShowPath(speed);
    }

    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {
        for (int i = 0; i < grid.GetCells().Length; i++)
        {
            grid.GetCells()[i].Distance = int.MaxValue;
        }

        List<HexCell> frontier = ListPool<HexCell>.Get();
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

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                int moveCost = 1;

                if (neighbor == null || neighbor.Distance != int.MaxValue)
                {
                    continue;
                }
                if (GlobalCellCheck.IsImpassable(neighbor) || neighbor.Unit)
                {
                    continue;
                }
                if (neighbor.HasOverlap)
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
        ListPool<HexCell>.Release(frontier);
        return false;
    }

    void ShowPath(int speed)
    {
        if (grid.HasPath)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                int turn = (current.Distance - 1) / speed;
                if (turn > 0)
                {
                    current.SetLabel("X");
                    current.EnableHighlight(Color.red);
                }
                else
                {
                    current.SetLabel("R");
                    current.EnableHighlight(Color.white);
                }
                current.HasOverlap = true;
                current = current.PathFrom;
            }
        }
        currentPathFrom.HasOverlap = true;
        currentPathFrom.EnableHighlight(Color.blue);
        if (!grid.GetBuoyCells().Contains(currentPathTo))
        {
            if (currentPathTo.GetLabel() == "R")
            {
                currentPathTo.EnableHighlight(Color.green);
            }
            else
            {
                currentPathTo.EnableHighlight(Color.red);
            }
        }
    }

    public int WithinTurnPath(int speed)
    {
        if (grid.HasPath)
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
        if (grid.HasPath)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                current.HasOverlap = false;
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.HasOverlap = false;
            current.DisableHighlight();
            grid.HasPath = false;
        }
        else if (currentPathFrom && !currentPathFrom.Structure)
        {
            currentPathFrom.DisableHighlight();
            if (!currentPathTo.Structure)
            {
                currentPathTo.DisableHighlight();
            }
        }
        currentPathFrom = currentPathTo = null;
    }
    public List<HexCell> GetPath()
    {
        if (!grid.HasPath)
        {
            return null;
        }

        List<HexCell> path = ListPool<HexCell>.Get();
        for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
        {
            path.Add(c);
        }
        path.Add(currentPathFrom);
        path.Reverse();
        return path;
    }
}