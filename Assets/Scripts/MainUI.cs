using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public HexGrid grid;

    HexCell currentCell;
    HexUnit selectedUnit;

    int currentTurn;

    [SerializeField] PlayerState initState;

    PlayerState currentState;

    void Start()
    {
        currentState = initState;
        currentState.Clean();
        currentTurn = currentState.GetTurn();
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
            currentCell = cell;
            return true;
        }
        return false;
    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (currentCell)
        {
            selectedUnit = currentCell.Unit;
        }
    }

    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell && selectedUnit.IsValidDestination(currentCell))
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
        if (grid.HasPath && grid.withinTurnPath(selectedUnit.MovementPoints) != int.MaxValue && selectedUnit.MovementPoints > 0)
        {
            selectedUnit.Location = currentCell;
            selectedUnit.MovementPoints = grid.withinTurnPath(selectedUnit.MovementPoints);
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
    public void SetEditMode(bool toggle)
    {
        enabled = !toggle;
        grid.ShowUI(!toggle);
        grid.ClearPath();
    }
}