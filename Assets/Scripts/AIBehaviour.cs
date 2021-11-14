using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is for allied and enemy units
public class AIBehaviour : MonoBehaviour
{
    public HexGrid grid;
    public MainUI mainUI;
    public Spawner spawner;
    HexUnit currentUnit;
    HexCell currentDestination, finalDestination;
    HexCell currentPathFrom, currentPathTo;
    bool currentPathExists;
    bool chaseState;
    List<HexCell> fullPath;
    int[] distances;
    int[] heuristics;

    void Start()
    {
        chaseState = false;
        distances = new int[grid.GetCells().Length];
        heuristics = new int[grid.GetCells().Length];
        currentUnit = this.gameObject.GetComponent<HexUnit>();
        ChooseTarget();
    }

    public void Execute()
    {
        if (currentUnit.Location != finalDestination)
        {
            StartCoroutine(TurnMove());
            SetMovementTarget(finalDestination);
        }
        else
        {
            ClearPath();
            if (chaseState)
            {
                ChooseTarget();
                StartCoroutine(TurnMove());
                SetMovementTarget(finalDestination);
            }
        }

        if (currentUnit.UnitType == "Fishing Boat")
        {
            CheckForPatrolBoat();
        }
    }

    IEnumerator TurnMove()
    {
        DoMove();
        grid.ShowUI(false);
        yield return new WaitUntil(() => currentUnit.Location.Position == currentDestination.Position);
        currentUnit.ActionPoints = WithinTurnPath(currentUnit.ActionPoints);
        yield return new WaitUntil(() => Mathf.Abs(currentUnit.transform.position.x - currentUnit.Location.transform.position.x) < 1.0f
        && Mathf.Abs(currentUnit.transform.position.z - currentUnit.Location.transform.position.z) < 1.0f);
        grid.ShowUI(true);

        if (currentUnit.Location == finalDestination)
        {
            ClearPath();
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

    void DoMove()
    {
        currentUnit.movement = true;
        currentUnit.Travel(GetPath());
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

            while (GlobalCellCheck.IsImpassable(finalDestination) || GlobalCellCheck.IsNotReachable(randomIndex) || finalDestination == currentUnit.Location)
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
        if (currentUnit.UnitType == "Tourist Boat")
        {
            ShowPath(speed);
        }
    }

    // Possible optimization: pass indexes instead of the objects so we don't have to run a linear search every time
    // polynomial time is fine ig but ughh
    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }

        List<HexCell> frontier = new List<HexCell>();
        distances[GetCellIndex(fromCell)] = 0;
        frontier.Add(fromCell);
        while (frontier.Count > 0)
        {
            HexCell current = frontier[0];
            int currentIndex = GetCellIndex(current);
            frontier.RemoveAt(0);

            if (current == toCell)
            {
                return true;
            }

            int currentTurn = (distances[currentIndex] - 1) / speed;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                int neighborIndex = GetCellIndex(neighbor);

                // We didn't plan to have variable move costs (e.g. certain parts of the map have you move faster), but we do have variable "speeds"
                int moveCost = 1;
                if (neighbor == null || distances[neighborIndex] != int.MaxValue)
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

                int distance = distances[currentIndex] + moveCost;
                distances[neighborIndex] = distance;
                neighbor.PathFrom = current;
                heuristics[neighborIndex] = neighbor.coordinates.DistanceTo(toCell.coordinates);
                frontier.Add(neighbor);
                frontier.Sort((x, y) => GetPriority(GetCellIndex(x)).CompareTo(GetPriority(GetCellIndex(y))));
            }
        }
        return false;
    }

    int GetPriority(int index)
    {
        return distances[index] + heuristics[index];
    }

    void ShowPath(int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            int currentIndex = GetCellIndex(current);
            while (current != currentPathFrom)
            {
                currentIndex = GetCellIndex(current);
                int turn = (distances[currentIndex] - 1) / speed;
                current.SetLabel((turn + 1).ToString());
                current.EnableHighlight(Color.green);
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
            int currentIndex = GetCellIndex(current);
            if (distances[currentIndex] <= speed)
            {
                return speed - distances[currentIndex];
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
                current.HasOverlap = false;
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
            c.HasOverlap = true;
            path.Add(c);
        }
        path.Add(currentPathFrom);
        path.Reverse();
        return path;
    }

    int GetCellIndex(HexCell cell)
    {
        int ret = System.Array.IndexOf(grid.GetCells(), cell);
        return ret;
    }

    // we want the fisherman to run if it spots a patrol boat in range (2 tiles for now)
    // the tourist boat should begin moving towards a buoy once it spawns.

    void CheckForPatrolBoat()
    {
        for (HexDirection i = HexDirection.NE; i <= HexDirection.NW; i++)
        {
            HexCell currentA = currentUnit.Location.GetNeighbor(i);
            if (currentA != null)
            {
                for (HexDirection j = HexDirection.NE; j <= HexDirection.NW; j++)
                {
                    HexCell currentB = currentA.GetNeighbor(j);
                    if (currentB != null)
                    {
                        if (currentB.Unit != null)
                        {
                            if (currentB.Unit.UnitType.Contains("Patrol Boat"))
                            {
                                chaseState = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (chaseState)
            {
                break;
            }
        }
    }
}