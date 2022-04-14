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
    public Upgrade Upgrade { get; set; }
    public int FeatureIndex
    {
        get
        {
            return featureIndex;
        }
        set
        {
            featureIndex = value;
        }
    }
    int featureIndex;

    Renderer cellRenderer;
    Color defaultColor;
    Feature feature;

    void Awake()
    {
        cellRenderer = GetComponent<Renderer>();
        defaultColor = cellRenderer.material.color;
    }

    void Start()
    {
        feature = uiRect.GetChild(1).GetComponent<Feature>();
        if (featureIndex != -1)
        {
            feature.SetImage(featureIndex);
            feature.EnableImage(true);
            if (featureIndex == 3)
            {
                feature.SetImageScale(0.75f, 0.75f);
            }
        }
        else
        {
            feature.EnableImage(false);
        }
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
        Text label = uiRect.GetChild(2).GetComponent<Text>();
        label.text = text;
    }

    public string GetLabel()
    {
        Text label = uiRect.GetChild(2).GetComponent<Text>();
        return label.text;
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

    void OnMouseOver()
    {
        if (FeatureIndex != -1)
        {
            feature.SetImage(featureIndex + 4);
        }
    }

    void OnMouseExit()
    {
        if (FeatureIndex != -1)
        {
            feature.SetImage(featureIndex);
        }
    }

    public void EnableHeavyHighlight()
    {
        cellRenderer.material.color = Color.red;
    }

    public void EnableHeavyHighlight(Color color)
    {
        cellRenderer.material.color = color;
    }

    public void ResetColor()
    {
        cellRenderer.material.color = defaultColor;
    }
}