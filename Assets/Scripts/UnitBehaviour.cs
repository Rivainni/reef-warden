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
    HexCell currentDestination;
    HexCell finalDestination;
    bool chaseState;
    bool safe;
    List<HexCell> path;


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
        grid.FindPath(currentUnit.Location, currentDestination, currentUnit.ActionPoints);
    }

    IEnumerator PerTurnMovement()
    {
        int currentTurn = mainUI.GetPlayerState().GetTurn();
        yield return new WaitUntil(() => mainUI.GetPlayerState().GetTurn() > currentTurn);
        DoMove();
        yield return new WaitForSeconds(2);
        safe = true;
    }

    void DoMove()
    {
        currentUnit.Travel(path);
        currentUnit.ActionPoints = grid.WithinTurnPath(currentUnit.ActionPoints);
        grid.ClearPath();
        grid.ShowUI(false);
    }

    void SetMovementTarget(HexCell target)
    {
        if (currentUnit.IsValidDestination(target))
        {
            currentDestination = target;
            DoPathfinding();
            path = grid.GetPath();
            foreach (HexCell cell in path)
            {
                currentDestination = cell;
                DoPathfinding();

                if (grid.WithinTurnPath(currentUnit.ActionPoints) < int.MaxValue)
                {
                    currentDestination = target;
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
            || randomIndex == 156 || randomIndex == 528)
            {
                randomIndex = Random.Range(0, grid.GetCells().Length - 1);
                finalDestination = grid.GetCells()[randomIndex];
            }
        }

        SetMovementTarget(finalDestination);
    }





    // we want the fisherman to run if it spots a patrol boat in range (2 tiles for now)
    // the tourist boat should begin moving towards a buoy once it spawns.
}