using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainUI : MonoBehaviour
{
    public HexGrid grid;
    public Spawner spawner;

    HexCell currentCell;
    HexUnit selectedUnit;

    [SerializeField] PlayerState initState;

    PlayerState currentState;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] GameObject doublePanelPrefab;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject textPrefab;
    [SerializeField] GameObject researchPrefab;
    [SerializeField] GameObject valuesContainer;

    void Start()
    {
        currentState = initState;
        currentState.Clean();
        currentState.UpdateHealth();
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
                    if (grid.HasPath && grid.WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
                    {
                        HexAction();
                    }
                }
                else
                {
                    DoPathfinding();
                }
            }
            else if (Input.GetMouseButtonDown(1) && grid.CheckUpgradeCell(currentCell))
            {
                DoUpgrade();
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

        if (selectedUnit == currentCell.Unit || !currentCell.Unit)
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
        selectedUnit.movement = true;
        selectedUnit.Travel(grid.GetPath());
        selectedUnit.ActionPoints = grid.WithinTurnPath(selectedUnit.ActionPoints);
        grid.ClearPath();
        grid.ShowUI(false);
    }

    void HexAction()
    {
        Vector3 spawnAt = Input.mousePosition;
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        HexCell tempA = null;
        HexCell tempB = null;

        List<string> contextMenuContent = new List<string>();

        if (selectedUnit.UnitType.Contains("Patrol Boat"))
        {
            if (grid.HasPath && grid.WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
            {
                contextMenuContent.Add("Patrol");

                // moving this check once we've marked the location of the reefs
                if (currentState.FetchCD("CH1") == 0 && currentState.FetchCD("CH2") == 0 && currentState.FetchCD("CH3") == 0)
                {
                    contextMenuContent.Add("Check Reef Health");
                }

                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCell toCheckCell = cell.GetNeighbor(d);
                    if (toCheckCell != null && toCheckCell.Unit != null)
                    {
                        if (toCheckCell.Unit.UnitType == "Fishing Boat" && !contextMenuContent.Contains("Catch Fisherman"))
                        {
                            contextMenuContent.Add("Catch Fisherman");
                            tempA = toCheckCell;
                        }
                        else if (toCheckCell.Unit.UnitType == "Tourist Boat" && !contextMenuContent.Contains("Inspect Tourist"))
                        {
                            contextMenuContent.Add("Inspect Tourist");
                            tempB = toCheckCell;
                        }
                    }
                }
            }
        }

        if (contextMenuContent.Count > 0)
        {
            GameObject contextMenu = Instantiate(panelPrefab, spawnAt, Quaternion.identity, transform);

            foreach (string item in contextMenuContent)
            {
                GameObject generic = Instantiate(buttonPrefab, contextMenu.transform.position, Quaternion.identity, contextMenu.transform);
                Button currentButton = generic.GetComponent<Button>();
                currentButton.GetComponentInChildren<Text>().text = item;
                if (item == "Patrol")
                {
                    currentButton.onClick.AddListener(() => Patrol(cell, contextMenu));
                }
                else if (item == "Check Reef Health")
                {
                    currentButton.onClick.AddListener(() => CheckHealth(cell, contextMenu));
                }
                else if (item == "Catch Fisherman")
                {
                    currentButton.onClick.AddListener(() => CatchFisherman(tempA, contextMenu, cell.Unit));
                }
                else if (item == "Inspect Tourist")
                {
                    currentButton.onClick.AddListener(() => InspectTourist(tempB, contextMenu, cell.Unit));
                }
            }
        }
    }

    void AfterAction(GameObject remove)
    {
        DoMove();
        UpdateUIElements();
        Destroy(remove);
    }

    void Patrol(HexCell destination, GameObject remove)
    {
        float factor = destination.Distance * 0.5f;
        currentState.AddSecurity(factor);
        Debug.Log(currentState.GetSecurity());
        AfterAction(remove);
    }

    void CheckHealth(HexCell destination, GameObject remove)
    {
        currentState.UpdateHealth();
        currentState.AddResearch(250);
        currentState.ResetCD("CH1");
        currentState.ResetCD("CH2");
        currentState.ResetCD("CH3");
        AfterAction(remove);
    }

    void InspectTourist(HexCell destination, GameObject remove, HexUnit target)
    {
        bool success = true;
        // mini game
        if (success)
        {
            currentState.AddSecurity(5);
        }
        AfterAction(remove);
    }

    void CatchFisherman(HexCell destination, GameObject remove, HexUnit target)
    {
        spawner.DestroyUnit(target);
        currentState.AddSecurity(2);
        AfterAction(remove);
    }

    void DoUpgrade()
    {
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        // clear the context menu
        List<string> contextMenuContent = new List<string>();

        foreach (string item in spawner.GetStructureTypes())
        {
            if (item != "Ranger Station" && item != "Buoy")
            {
                contextMenuContent.Add(item);
            }
        }
        if (contextMenuContent.Count > 0)
        {
            GameObject upgradePanel = Instantiate(doublePanelPrefab, spawnAt, Quaternion.identity, transform);

            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    foreach (string item in contextMenuContent)
                    {
                        GameObject generic = Instantiate(buttonPrefab, upgradePanel.transform.GetChild(i).position, Quaternion.identity, upgradePanel.transform.GetChild(i));
                        Button currentButton = generic.GetComponent<Button>();
                        currentButton.GetComponentInChildren<Text>().text = item;
                        currentButton.onClick.AddListener(() => UpgradeText(item, currentButton, upgradePanel));
                    }
                }
                else
                {
                    GameObject generic = Instantiate(textPrefab, upgradePanel.transform.GetChild(i).position, Quaternion.identity, upgradePanel.transform.GetChild(i));
                    Text currentText = generic.GetComponent<Text>();
                    currentText.text = "Select an available upgrade to get started!";
                }
            }
        }
    }

    void BuildUpgrade(string upgrade, int constructionTime, int researchCost, int buildCost, GameObject remove)
    {
        currentState.QueueUpgrade(upgrade, constructionTime);
        Destroy(remove);
        currentState.AdjustMoney(-buildCost);
        currentState.AddManpower(-1);
        UpdateUIElements();
        StartCoroutine(DelayedBuild(upgrade, constructionTime, researchCost, buildCost));
    }

    IEnumerator DelayedBuild(string upgrade, int constructionTime, int researchCost, int buildCost)
    {
        yield return new WaitUntil(() => currentState.CheckUpgrade(upgrade) == 0);
        spawner.SpawnUpgrade(currentCell, upgrade, constructionTime, researchCost, buildCost);
        currentState.AddManpower(1);
        UpdateUIElements();
    }

    void UpgradeText(string upgrade, Button button, GameObject toRemove)
    {
        GameObject toReplace = button.transform.parent.parent.GetChild(1).GetChild(0).gameObject;
        int constructionTime = 0;
        int researchCost = 0;
        int buildCost = 0;

        if (upgrade == "Radar")
        {
            toReplace.GetComponent<Text>().text = "When triggered, it gives the player map-wide visibility for one turn. May be used again after 5 turns.";
            toReplace.GetComponent<Text>().text += "\n\nCosts 2000. Requires 250 RP. Has upkeep of 200 per turn.";
            toReplace.GetComponent<Text>().text += "\nRequires 1 turn and 1 manpower to construct.";

            constructionTime = 1;
            researchCost = 250;
            buildCost = 2000;
        }
        else if (upgrade == "AIS")
        {
            toReplace.GetComponent<Text>().text = "Lets the player know the information of the vessels in the area by Identifying their purpose.";
            toReplace.GetComponent<Text>().text += "\n\nCosts 2500. Requires 250 RP. Has upkeep of 250 per turn. ";

            constructionTime = 1;
            researchCost = 250;
            buildCost = 2500;
        }

        GameObject generic = Instantiate(buttonPrefab, toReplace.transform.parent.position, Quaternion.identity, toReplace.transform.parent);
        Button currentButton = generic.GetComponent<Button>();

        if (currentState.CheckUpgrade(upgrade) > 0)
        {
            currentButton.GetComponentInChildren<Text>().text = "IN QUEUE (" + currentState.CheckUpgrade(upgrade) + " TURNS)";
        }
        else
        {
            currentButton.GetComponentInChildren<Text>().text = "BUILD";
            currentButton.onClick.AddListener(() => BuildUpgrade(upgrade, constructionTime, researchCost, buildCost, toRemove));
        }
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
        currentState.NextTurn();
        clicked.GetComponentInChildren<Text>().text = "TURN " + currentState.GetTurn();
        grid.ResetPoints();
        selectedUnit = null;
        grid.ClearPath();
        UpdateUIElements();
    }

    public void Research(Button clicked)
    {
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        GameObject researchPanel = Instantiate(researchPrefab, spawnAt, Quaternion.identity, transform);
    }
}