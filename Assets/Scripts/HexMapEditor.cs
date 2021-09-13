using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class HexMapEditor : MonoBehaviour
{
    public HexGrid hexGrid;
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    public void SetEditMode(bool toggle)
    {
        enabled = toggle;
        Debug.Log("Edit mode is now " + toggle);
    }

    void Awake()
    {
        SetEditMode(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // if (Input.GetMouseButton(0))
            // {
            //     HandleInput();
            //     return;
            // }
            if (Input.GetMouseButton(0))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    CreateUnit();
                }

                return;
            }
        }
    }

    void HandleInput() // leaving this for now
    {
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell)
        {
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }

            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    HexCell GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
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

    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit)
        {
            hexGrid.AddUnit(Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f));
        }
    }

    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }

    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    // public void Save()
    // {
    //     string path = Path.Combine(Application.persistentDataPath, "test.map");
    //     using
    //     (
    //         BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create))
    //     )
    //     {
    //         writer.Write(0);
    //         hexGrid.Save(writer);
    //     }
    // }

    // public void Load()
    // {
    //     string path = Path.Combine(Application.persistentDataPath, "test.map");
    //     using
    //     (
    //         BinaryReader reader = new BinaryReader(File.OpenRead(path))
    //     )
    //     {
    //         reader.ReadInt32();
    //         hexGrid.Load(reader);
    //     }
    // }
}