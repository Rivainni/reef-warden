using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainUI : MonoBehaviour
{
    public HexGrid grid;

    HexCell currentCell;
    HexUnit selectedUnit;

    [SerializeField] PlayerState initState;

    PlayerState currentState;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] GameObject valuesContainer;
    List<string> contextMenuContent = new List<string>();

    void Start()
    {
        currentState = initState;
        currentState.Clean();
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    HexAction();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
        else
        {

        }
    }
    bool UpdateCurrentCell()
    {
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (cell)
        {
            if (cell != currentCell)
            {
                Debug.Log("You clicked on a cell with coordinates " + cell.coordinates.ToString());
                currentCell = cell;
                return true;
            }
        }
        return false;
    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();

        if (selectedUnit == currentCell.Unit)
        {
            selectedUnit = null;
        }
        else if (currentCell.Unit)
        {
            selectedUnit = currentCell.Unit;
            Debug.Log("Selected " + selectedUnit.UnitType);
        }

        grid.ShowUI(true);
    }

    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell && selectedUnit.IsValidDestination(currentCell) && selectedUnit.ActionPoints > 0)
            {
                grid.FindPath(selectedUnit.Location, currentCell, selectedUnit.ActionPoints);
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    void DoMove()
    {
        if (grid.HasPath && grid.WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
        {
            selectedUnit.movement = true;
            selectedUnit.Travel(grid.GetPath());
            selectedUnit.ActionPoints = grid.WithinTurnPath(selectedUnit.ActionPoints);
            grid.ClearPath();
            grid.ShowUI(false);
        }
    }

    void HexAction()
    {
        Vector3 spawnAt = grid.GetClickPosition(Camera.main.ScreenPointToRay(Input.mousePosition));
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        // clear the context menu
        contextMenuContent = null;
        GameObject contextMenu = Instantiate(panelPrefab, spawnAt + new Vector3(0, 500f, 0), Quaternion.identity, transform);

        if (selectedUnit.UnitType.Contains("Patrol Boat"))
        {
            if (grid.HasPath && grid.WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
            {
                contextMenuContent.Add("Patrol");
                Button first = contextMenu.GetComponent<Button>();
                first.GetComponent<Text>().text = "Patrol";
            }
        }
    }

    void Patrol()
    {
        DoMove();
    }

    public void EndTurn(Button clicked)
    {
        currentState.nextTurn();
        clicked.GetComponentInChildren<Text>().text = "TURN " + currentState.GetTurn();
        grid.ResetPoints();
        selectedUnit = null;
        grid.ClearPath();
    }

    void UpdateUIElements()
    {

    }
}