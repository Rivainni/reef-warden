using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public HexCell[] neighbors;
    public RectTransform uiRect;
    public HexGridChunk chunk;
    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }
    public int SearchHeuristic { get; set; }
    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }
    public HexCell PathFrom { get; set; }
    public bool HasOverlap
    {
        get
        {
            return hasOverlap;
        }
        set
        {
            hasOverlap = value;
        }
    }

    bool hasOverlap;

    public string Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
        }
    }

    string type;

    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
        }
    }
    int distance;

    public int Index
    {
        get
        {
            return index;
        }
        set
        {
            index = value;
        }
    }
    int index;

    public HexUnit Unit { get; set; }
    public HexStructure Structure { get; set; }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public void SetLabel(string text)
    {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }
    public void DisableHighlight()
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    public void EnableHighlight(Color color)
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    // public void Save(BinaryWriter writer)
    // {
    //     writer.Write(IsImpassable);
    // }

    // public void Load(BinaryReader reader)
    // {
    //     IsImpassable = reader.ReadBoolean();
    // }
}