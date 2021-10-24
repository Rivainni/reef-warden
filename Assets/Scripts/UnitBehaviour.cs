using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is for allied and enemy units
public class UnitBehaviour : MonoBehaviour
{
    public HexGrid grid;
    public MainUI mainUI;
    public Spawner spawner;
    HexUnit currentUnit;
    HexCell currentDestination, finalDestination;
    HexCell currentPathFrom, currentPathTo;
    bool currentPathExists;
    bool chaseState;

    bool safe;
    List<HexCell> fullPath;
    HexCell[] dummy;


    void Start()
    {
        safe = true;
        chaseState = false;
        currentUnit = this.gameObject.GetComponent<HexUnit>();
        ChooseTarget();
    }

    void LateUpdate()
    {
        if (safe)
        {
            StartCoroutine(PerTurnMovement());
            safe = false;
        }
    }

    // we want the ability to set a target destination
    // then, we check if the target destination is reachable. if yes, go there. if not, store the path
    // and continue once the turn increments

    void DoPathfinding()
    {
        if (currentUnit.ActionPoints > 0)
        {
            FindPath(currentUnit.Location, currentDestination, currentUnit.ActionPoints);
        }
    }

    IEnumerator PerTurnMovement()
    {
        int currentTurn = mainUI.GetPlayerState().GetTurn();
        yield return new WaitUntil(() => mainUI.GetPlayerState().GetTurn() > currentTurn);
        DoMove();
        yield return new WaitUntil(() => currentUnit.Location == currentDestination);
        SetMovementTarget(finalDestination);
        if (currentUnit.Location == finalDestination)
        {
            safe = false;
        }
        else
        {
            safe = true;
        }
    }

    void DoMove()
    {
        currentUnit.movement = true;
        currentUnit.Travel(GetPath());
        currentUnit.ActionPoints = WithinTurnPath(currentUnit.ActionPoints);
        grid.ClearPath();
        grid.ShowUI(false);
    }

    void SetMovementTarget(HexCell target)
    {
        if (currentUnit.IsValidDestination(target))
        {
            currentDestination = target;
            DoPathfinding();
            fullPath = GetPath();
            for (int i = fullPath.Count - 1; i > 0; i--)
            {
                currentDestination = fullPath[i];
                DoPathfinding();

                if (WithinTurnPath(currentUnit.ActionPoints) < int.MaxValue)
                {
                    grid.ShowUI(true);
                    break;
                }
            }
        }
    }

    void ChooseTarget()
    {
        if (currentUnit.UnitType == "Tourist Boat")
        {
            int randomIndex = Random.Range(0, grid.GetBuoyCells().Count - 1);
            finalDestination = grid.GetBuoyCells()[randomIndex];
        }
        else if (currentUnit.UnitType == "Fishing Boat")
        {
            int randomIndex = Random.Range(0, grid.GetCells().Length - 1);
            finalDestination = grid.GetCells()[randomIndex];

            while (finalDestination.IsImpassable || (464 >= randomIndex && 311 <= randomIndex)
            || randomIndex == 105 || randomIndex == 106 || randomIndex == 130 || randomIndex == 155
            || randomIndex == 156 || randomIndex == 528 || finalDestination == currentUnit.Location)
            {
                randomIndex = Random.Range(0, grid.GetCells().Length - 1);
                finalDestination = grid.GetCells()[randomIndex];
            }
        }

        SetMovementTarget(finalDestination);
    }


    // duplicate methods for individual movement
    void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists = Search(fromCell, toCell, speed);
        ShowPath(speed);
    }

    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {
        dummy = (HexCell[])grid.GetCells().Clone();
        for (int i = 0; i < dummy.Length; i++)
        {
            dummy[i].Distance = int.MaxValue;
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

    int WithinTurnPath(int speed)
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

    void ClearPath()
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
    List<HexCell> GetPath()
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





    // we want the fisherman to run if it spots a patrol boat in range (2 tiles for now)
    // the tourist boat should begin moving towards a buoy once it spawns.
}