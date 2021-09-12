using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;
    public HexGrid hexGrid;
    // private Color activeColor;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell, searchFromCell, searchToCell;
    void Awake()
    {
        SelectColor(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }

            if (Input.GetKey(KeyCode.LeftShift) && searchToCell != currentCell)
            {
                if (searchFromCell)
                {
                    searchFromCell.DisableHighlight();
                }
                searchFromCell = currentCell;
                searchFromCell.EnableHighlight(Color.blue);
                if (searchToCell)
                {
                    hexGrid.FindPath(searchFromCell, searchToCell);
                }
            }
            else if (searchFromCell && searchFromCell != currentCell)
            {
                searchToCell = currentCell;
                hexGrid.FindPath(searchFromCell, searchToCell);
            }
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
            isDrag = false;
        }
    }

    void EditCell(HexCell cell)
    {
        // cell.Color = activeColor;
    }

    public void SelectColor(int index)
    {
        // activeColor = colors[index];
    }
}