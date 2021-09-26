using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public HexGrid grid;

    HexCell currentCell;
    HexUnit selectedUnit;

    [SerializeField] PlayerState initState;

    PlayerState currentState;

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
                    DoMove();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
    }
    bool UpdateCurrentCell()
    {
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (cell != currentCell)
        {
            Debug.Log("You clicked on a cell with coordinates " + cell.coordinates.ToString());
            currentCell = cell;
            return true;
        }
        return false;
    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (currentCell.Unit)
        {
            selectedUnit = currentCell.Unit;
            Debug.Log("Selected " + selectedUnit.UnitType);
        }
    }

    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell && selectedUnit.IsValidDestination(currentCell) && selectedUnit.MovementPoints > 0)
            {
                grid.FindPath(selectedUnit.Location, currentCell, selectedUnit.MovementPoints);
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    void DoMove()
    {
        if (grid.HasPath && grid.WithinTurnPath(selectedUnit.MovementPoints) < int.MaxValue && selectedUnit.MovementPoints > 0)
        {
            selectedUnit.movement = true;
            selectedUnit.Location = currentCell;
            selectedUnit.MovementPoints = grid.WithinTurnPath(selectedUnit.MovementPoints);
            grid.ClearPath();
        }
    }

    public void EndTurn(Button clicked)
    {
        currentState.endTurn();
        clicked.GetComponentInChildren<Text>().text = "TURN " + currentState.GetTurn();
        grid.ResetPoints();
        selectedUnit = null;
        grid.ClearPath();
    }
}