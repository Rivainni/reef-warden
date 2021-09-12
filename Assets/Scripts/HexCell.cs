using UnityEngine.UI;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    [SerializeField] HexCell[] neighbors;
    public RectTransform uiRect;
    public HexGridChunk chunk;
    public int SearchHeuristic { get; set; }
    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }
    public HexCell PathFrom { get; set; }
    public bool IsImpassable
    {
        get
        {
            return isImpassable;
        }
        set
        {
            if (isImpassable == value)
            {
                return;
            }
            isImpassable = true;
        }
    }

    bool isImpassable;

    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
            UpdateDistanceLabel();
        }
    }
    int distance;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    void UpdateDistanceLabel()
    {
        Text label = uiRect.GetComponent<Text>();
        label.text = distance == int.MaxValue ? "" : distance.ToString();
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
}