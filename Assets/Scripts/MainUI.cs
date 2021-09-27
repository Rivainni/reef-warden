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
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject valuesContainer;
    List<string> contextMenuContent = new List<string>();

    void Start()
    {
        currentState = initState;
        currentState.Clean();
        UpdateUIElements();
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
        contextMenuContent.Clear();
        GameObject contextMenu = Instantiate(panelPrefab, spawnAt, Quaternion.identity, transform);

        if (selectedUnit.UnitType.Contains("Patrol Boat"))
        {
            if (grid.HasPath && grid.WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
            {
                contextMenuContent.Add("Patrol");
            }
        }

        foreach (string item in contextMenuContent)
        {
            GameObject generic = Instantiate(buttonPrefab, contextMenu.transform.position, Quaternion.identity, contextMenu.transform);
            Button currentButton = generic.GetComponent<Button>();
            currentButton.GetComponentInChildren<Text>().text = item;
            currentButton.onClick.AddListener(() => Patrol(cell, contextMenu));
        }

        contextMenu.transform.Translate(new Vector3(buttonPrefab.GetComponent<RectTransform>().rect.width * 2, buttonPrefab.GetComponent<RectTransform>().rect.height * 2, 0));
    }

    void Patrol(HexCell destination, GameObject remove)
    {
        float factor = destination.Distance * 0.5f;
        currentState.AddSecurity(factor);
        Debug.Log(currentState.GetSecurity());
        DoMove();
        UpdateUIElements();
        Destroy(remove);
    }

    void UpdateUIElements()
    {
        UpdateText[] toUpdate = GetComponentsInChildren<UpdateText>();
        foreach (UpdateText item in toUpdate)
        {
            item.UpdateUIElement();
        }
    }

    public PlayerState GetPlayerState()
    {
        return currentState;
    }

    public void EndTurn(Button clicked)
    {
        currentState.nextTurn();
        clicked.GetComponentInChildren<Text>().text = "TURN " + currentState.GetTurn();
        grid.ResetPoints();
        selectedUnit = null;
        grid.ClearPath();
        UpdateUIElements();
    }
}