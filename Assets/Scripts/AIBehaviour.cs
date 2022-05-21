using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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
    bool stateChanged;
    bool satisfied;
    int turnStopped;
    int[] distances;
    int[] heuristics;

    void Start()
    {
        currentPathExists = false;
        chaseState = false;
        stateChanged = false;
        distances = new int[grid.GetCells().Length];
        heuristics = new int[grid.GetCells().Length];
        currentUnit = this.gameObject.GetComponent<HexUnit>();
        ChooseTarget();
        if (currentUnit.UnitType == "Tourist Boat")
        {
            finalDestination.EnableHighlight(Color.green);
        }
        turnStopped = 0;
        satisfied = false;
    }

    public void Execute()
    {
        if (currentUnit.Location != finalDestination)
        {

            if (mainUI.GetPlayerState().CheckAIS() && currentUnit.UnitType != "Fishing Boat")
            {
                if (!currentPathExists)
                {
                    SetMovementTarget(finalDestination);
                }
                StartCoroutine(TurnMove());
                SetMovementTarget(finalDestination);
            }
            else
            {
                SetMovementTarget(finalDestination);
                if (currentPathExists)
                {
                    StartCoroutine(TurnMove());
                }
            }

            // this may look stupid, but this is for when the unit has no more final destinations
            if (turnStopped == 0 && currentUnit.Location == finalDestination)
            {
                turnStopped = mainUI.GetPlayerState().GetTurn();
                if (currentPathExists)
                {
                    ClearPath();
                }
                if (currentUnit.UnitType == "Tourist Boat")
                {
                    currentUnit.Location.EnableHeavyHighlight();
                    turnStopped++;
                }
            }
        }
        else if (!stateChanged)
        {
            if (currentUnit.UnitType == "Fishing Boat")
            {
                CheckForPatrolBoat();
                mainUI.GetPlayerState().DecreaseHealth(4 * mainUI.GetPlayerState().GetLevel());

                if (chaseState || mainUI.GetTimeController().IsDay())
                {
                    currentUnit.Location.ResetColor();
                    ChooseEscape();
                    StartCoroutine(TurnMove());
                    stateChanged = true;
                }
            }
            else if (currentUnit.UnitType == "Tourist Boat" && mainUI.GetPlayerState().GetTurn() >= turnStopped + 2)
            {
                if (!satisfied)
                {
                    int random = Random.Range(0, 10);
                    if (random == 0)
                    {
                        mainUI.GetPlayerState().DecreaseHealth(10);
                    }
                    spawner.DestroyWaypoint(currentUnit.Waypoint);
                    mainUI.UpdateUIElements();
                }
                currentUnit.Location.ResetColor();
                ChooseEscape();
                stateChanged = true;
            }
        }
        else if (stateChanged && currentUnit.Location == finalDestination)
        {
            spawner.DestroyUnit(currentUnit);
            if (currentUnit.UnitType == "Tourist Boat")
            {
                if (mainUI.GetPlayerState().CheckSS())
                {
                    int levelBonus = mainUI.GetPlayerState().GetLevel() > 3 ? 3 : mainUI.GetPlayerState().GetLevel();
                    mainUI.GetPlayerState().AdjustMoney((int)(1500 + (1500 * 0.1f * levelBonus)));
                }
                else
                {
                    mainUI.GetPlayerState().AdjustMoney(1500);
                }
            }
        }
    }

    IEnumerator TurnMove()
    {
        DoMove();
        yield return new WaitUntil(() => currentUnit.Location == currentDestination);
        currentUnit.ActionPoints = WithinTurnPath(currentUnit.ActionPoints);
        yield return new WaitUntil(() => currentUnit.movement == false);
        if (!mainUI.GetPlayerState().CheckAIS() || currentUnit.UnitType == "Fishing Boat")
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
        currentDestination = target;
        DoPathfinding();

        if (WithinTurnPath(currentUnit.ActionPoints) == int.MaxValue)
        {
            for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
            {
                if (distances[c.Index] <= currentUnit.ActionPoints && c.Unit == null)
                {
                    currentDestination = c;
                    break;
                }
            }
        }

        if (currentPathExists)
        {
            ClearPath();
        }
        DoPathfinding();
    }

    void ChooseTarget()
    {
        if (currentUnit.UnitType == "Tourist Boat" || currentUnit.UnitType == "Fishing Boat")
        {
            ChooseBuoy();
            if (finalDestination == null)
            {
                spawner.DestroyUnit(currentUnit);
            }
        }
    }

    void ChooseBuoy()
    {
        int maxDistance = 0;
        int currentIndex = -1;
        for (int i = 0; i < grid.GetBuoyCells().Count; i++)
        {
            finalDestination = grid.GetBuoyCells()[i];
            FindPath(currentUnit.Location, finalDestination, currentUnit.ActionPoints);
            if (i == 0)
            {
                maxDistance = distances[finalDestination.Index];
            }

            int currDistance = distances[finalDestination.Index];
            if (currDistance < maxDistance && finalDestination.Unit == null)
            {
                currentIndex = finalDestination.Index;
            }
            if (currentPathExists)
            {
                ClearPath();
            }
        }

        if (currentIndex != -1)
        {
            finalDestination = grid.GetCells()[currentIndex];
        }
        else
        {
            finalDestination = null;
        }
    }

    void ChooseEscape()
    {
        int maxDistance = 0;
        int currentIndex = 0;
        for (int i = 0; i < GlobalCellCheck.GetEscapeCellCount(); i++)
        {
            finalDestination = grid.GetCells()[GlobalCellCheck.GetEscapeCell(i)];
            FindPath(currentUnit.Location, finalDestination, currentUnit.ActionPoints);
            if (i == 0)
            {
                maxDistance = distances[GlobalCellCheck.GetEscapeCell(i)];
            }

            int currDistance = distances[GlobalCellCheck.GetEscapeCell(i)];
            if (currDistance < maxDistance)
            {
                currentIndex = GlobalCellCheck.GetEscapeCell(i);
            }
            if (currentPathExists)
            {
                ClearPath();
            }
        }

        finalDestination = grid.GetCells()[currentIndex];
        SetMovementTarget(finalDestination);
    }


    // duplicate methods for individual movement
    void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        if (currentPathExists)
        {
            ClearPath();
        }
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists = Search(fromCell, toCell, speed);
        if (currentUnit.UnitType == "Tourist Boat" || mainUI.GetPlayerState().CheckAIS())
        {
            ShowPath(speed);
        }
        else
        {
            ShowPathHidden(speed);
        }
    }

    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }

        List<HexCell> frontier = ListPool<HexCell>.Get();
        distances[fromCell.Index] = 0;
        frontier.Add(fromCell);
        while (frontier.Count > 0)
        {
            HexCell current = frontier[0];
            int currentIndex = current.Index;
            frontier.RemoveAt(0);

            if (current == toCell)
            {
                return true;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null)
                {
                    continue;
                }

                int moveCost = 1;
                if (neighbor == null || distances[neighbor.Index] != int.MaxValue)
                {
                    continue;
                }
                if (GlobalCellCheck.IsImpassable(neighbor) || neighbor.Unit || GlobalCellCheck.IsNotReachable(neighbor.Index))
                {
                    continue;
                }
                if (neighbor.HasOverlap)
                {
                    continue;
                }

                int distance = distances[currentIndex] + moveCost;
                distances[neighbor.Index] = distance;
                neighbor.PathFrom = current;
                heuristics[neighbor.Index] = neighbor.coordinates.DistanceTo(toCell.coordinates);
                frontier.Add(neighbor);
                frontier.Sort((x, y) => GetPriority(x.Index).CompareTo(GetPriority(y.Index)));
            }
        }
        ListPool<HexCell>.Release(frontier);
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
            while (current != currentPathFrom)
            {
                current.SetLabel("M");
                current.EnableHighlight(Color.green);
                current.HasOverlap = true;
                current = current.PathFrom;
            }
        }
        currentPathFrom.HasOverlap = true;
        currentPathFrom.EnableHighlight(Color.blue);
        if (!grid.GetBuoyCells().Contains(currentPathTo))
        {
            currentPathTo.EnableHighlight(Color.green);
        }
    }

    void ShowPathHidden(int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            int currentIndex;
            while (current != currentPathFrom)
            {
                currentIndex = current.Index;
                int turn = (distances[currentIndex] - 1) / speed;
                current.HasOverlap = true;
                current = current.PathFrom;
            }
        }
        currentPathFrom.HasOverlap = true;
    }

    int WithinTurnPath(int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            int currentIndex = current.Index;
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
            current.HasOverlap = false;
            current.DisableHighlight();
            currentPathExists = false;
        }
        if (currentPathFrom && !currentPathFrom.Structure)
        {
            currentPathFrom.DisableHighlight();
            if (!currentPathTo.Structure)
            {
                currentPathTo.DisableHighlight();
            }
        }
        currentPathFrom = currentPathTo = null;
    }

    List<HexCell> GetPath()
    {
        if (!currentPathExists)
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

    // we want the fisherman to run if it spots a patrol boat in range (2 tiles for now)
    // the tourist boat should begin moving towards a buoy once it spawns.

    void CheckForPatrolBoat()
    {
        if (currentUnit.ScanFor("Patrol Boat"))
        {
            chaseState = true;
            stateChanged = true;
        }
    }

    public void Clean()
    {
        if (currentPathExists)
        {
            ClearPath();
        }
    }

    public bool HasStopped()
    {
        return turnStopped > 0 && finalDestination == currentUnit.Location;
    }

    public void Moor()
    {
        satisfied = true;
        currentUnit.SetMoored();
        spawner.DestroyWaypoint(currentUnit.Waypoint);
    }
}