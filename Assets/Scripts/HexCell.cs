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

    public bool IsVisible
    {
        get
        {
            return visibility > 0;
        }
    }
    int visibility;

    public HexUnit Unit { get; set; }
    public HexStructure Structure { get; set; }
    public Upgrade Upgrade { get; set; }
    public int Adjacency
    {
        get
        {
            return adjacency;
        }
        set
        {
            adjacency = value;
            if (adjacency > 0)
            {
                Image highlight = uiRect.GetChild(1).GetComponent<Image>();
                highlight.enabled = true;
            }
        }
    }
    int adjacency;

    Renderer cellRenderer;
    Color defaultColor;

    void Awake()
    {
        cellRenderer = GetComponent<Renderer>();
        defaultColor = cellRenderer.material.color;
    }

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

    void ClearFeature()
    {
        Image highlight = uiRect.GetChild(2).GetComponent<Image>();
        highlight.enabled = false;
    }

    void EnableFeature()
    {
        Image highlight = uiRect.GetChild(2).GetComponent<Image>();
        highlight.enabled = true;
    }

    void OnMouseOver()
    {
        if (Adjacency > 0)
        {
            EnableFeature();
        }
    }

    void OnMouseExit()
    {
        if (Adjacency > 0)
        {
            ClearFeature();
        }
    }

    public void EnableHeavyHighlight()
    {
        cellRenderer.material.color = Color.red;
    }

    public void ResetColor()
    {
        cellRenderer.material.color = defaultColor;
    }

    public void IncreaseVisibility()
    {
        if (visibility < 1)
        {
            visibility++;
        }
    }

    public void DecreaseVisibility()
    {
        if (visibility > 0)
        {
            visibility--;
        }
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