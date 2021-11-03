using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainUI : MonoBehaviour
{
    public HexGrid grid;
    public Spawner spawner;

    HexCell currentCell;
    HexCell playerLocation;
    HexUnit selectedUnit;

    [SerializeField] PlayerState initState;

    PlayerState currentState;
    [SerializeField] MinigameData minigameData;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] GameObject doublePanelPrefab;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject textPrefab;
    [SerializeField] GameObject researchPrefab;
    [SerializeField] GameObject valuesContainer;
    [SerializeField] Button radarButton;

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

        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main Menu");
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
        int bawal = System.Array.IndexOf(grid.GetCells(), currentCell);
        Debug.Log("DO NOT GO TO CELL NUMBER " + bawal);

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
        HexUnit targetA = null;
        HexUnit targetB = null;

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
                            targetA = toCheckCell.Unit;
                        }
                        else if (toCheckCell.Unit.UnitType == "Tourist Boat" && !contextMenuContent.Contains("Inspect Tourist"))
                        {
                            contextMenuContent.Add("Inspect Tourist");
                            tempB = toCheckCell;
                            targetB = toCheckCell.Unit;
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
                    currentButton.onClick.AddListener(() => CatchFisherman(tempA, contextMenu, targetA));
                }
                else if (item == "Inspect Tourist")
                {
                    currentButton.onClick.AddListener(() => InspectTourist(tempB, contextMenu, targetB));
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
        playerLocation = destination;
        Debug.Log("You are at " + playerLocation.coordinates.ToString());
        currentState.AddSecurity(factor);
        AfterAction(remove);
    }

    void CheckHealth(HexCell destination, GameObject remove)
    {
        currentState.UpdateHealth();
        currentState.AddResearch(250);
        currentState.ResetCD("CH1");
        currentState.ResetCD("CH2");
        currentState.ResetCD("CH3");
        playerLocation = destination;
        Debug.Log("You are at " + playerLocation.coordinates.ToString());
        AfterAction(remove);
    }

    void InspectTourist(HexCell destination, GameObject remove, HexUnit target)
    {
        InspectTouristGame(target);
        playerLocation = destination;
        Debug.Log("You are at " + playerLocation.coordinates.ToString());
        AfterAction(remove);
    }

    void InspectTouristGame(HexUnit target)
    {
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        GameObject gamePanel = Instantiate(doublePanelPrefab, spawnAt, Quaternion.identity, transform);

        for (int i = 0; i < 2; i++)
        {
            GameObject toShow = Instantiate(textPrefab, gamePanel.transform.GetChild(i).position, Quaternion.identity, gamePanel.transform.GetChild(i));
            Text matchText = toShow.GetComponent<Text>();
            matchText.text = minigameData.GenerateSet(0, i);
        }

        // determine if value should be true or not
        bool correctValue = false;

        GameObject approvePanel = Instantiate(panelPrefab, gamePanel.transform.position, Quaternion.identity, gamePanel.transform);
        GameObject buttonA = Instantiate(buttonPrefab, approvePanel.transform.position, Quaternion.identity, approvePanel.transform);
        GameObject buttonB = Instantiate(buttonPrefab, approvePanel.transform.position, Quaternion.identity, approvePanel.transform);
        Button actualButtonA = buttonA.GetComponent<Button>();
        Button actualButtonB = buttonB.GetComponent<Button>();

        actualButtonA.GetComponentInChildren<Text>().text = "Approve";
        actualButtonB.GetComponentInChildren<Text>().text = "Disapprove";

        actualButtonA.onClick.AddListener(() => InspectTouristGameApprove(correctValue, gamePanel));
        actualButtonB.onClick.AddListener(() => InspectTouristGameDisapprove(correctValue, target, gamePanel));
    }

    void InspectTouristGameApprove(bool correctValue, GameObject toRemove)
    {
        Destroy(toRemove);
        currentState.AddTouristScore();
    }

    void InspectTouristGameDisapprove(bool correctValue, HexUnit target, GameObject toRemove)
    {
        spawner.DestroyUnit(target);
        if (!correctValue)
        {
            currentState.AddSecurity(5);
            UpdateUIElements();
        }
        Destroy(toRemove);
        currentState.AddTouristScore();
        currentState.AddTourists(-1);
    }

    void CatchFisherman(HexCell destination, GameObject remove, HexUnit target)
    {
        spawner.DestroyUnit(target);
        currentState.AddSecurity(2);
        AfterAction(remove);
        currentState.AddCatchScore();
    }

    void DoUpgrade()
    {
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        // clear the context menu
        List<string> contextMenuContent = new List<string>();

        foreach (string item in spawner.GetUpgradeTypes())
        {
            contextMenuContent.Add(item);
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

    void BuildUpgrade(string upgrade, int constructionTime, int researchCost, int buildCost, int upkeep, GameObject remove)
    {
        currentState.QueueUpgrade(upgrade, constructionTime);
        Destroy(remove);
        currentState.AdjustMoney(-buildCost);
        currentState.AddManpower(-1);
        UpdateUIElements();
        StartCoroutine(DelayedBuild(upgrade, constructionTime, researchCost, buildCost, upkeep));
    }

    IEnumerator DelayedBuild(string upgrade, int constructionTime, int researchCost, int buildCost, int upkeep)
    {
        yield return new WaitUntil(() => currentState.CheckUpgrade(upgrade) == 0);
        spawner.SpawnUpgrade(currentCell, upgrade, constructionTime, researchCost, buildCost);
        currentState.AdjustIncome(-upkeep);
        currentState.AddManpower(1);
        if (upgrade == "RADAR")
        {
            radarButton.interactable = true;
        }
        currentState.AddUpgrade(upgrade);
        UpdateUIElements();
    }

    void UpgradeText(string upgrade, Button button, GameObject toRemove)
    {
        GameObject toReplace = button.transform.parent.parent.GetChild(1).GetChild(0).gameObject;
        int constructionTime = 0;
        int researchCost = 0;
        int buildCost = 0;
        int upkeep = 0;

        if (upgrade == "RADAR")
        {
            toReplace.GetComponent<Text>().text = "When triggered, it gives the player map-wide visibility for one turn. May be used again after 5 turns.";
            toReplace.GetComponent<Text>().text += "\n\nCosts 2000. Requires 250 RP. Has upkeep of 200 per turn.";
            toReplace.GetComponent<Text>().text += "\nRequires 1 turn and 1 manpower to construct.";

            constructionTime = 1;
            researchCost = 250;
            buildCost = 2000;
            upkeep = 200;
        }
        else if (upgrade == "AIS")
        {
            toReplace.GetComponent<Text>().text = "Lets the player know the information of the vessels in the area by Identifying their purpose.";
            toReplace.GetComponent<Text>().text += "\n\nCosts 2500. Requires 250 RP. Has upkeep of 250 per turn. ";

            constructionTime = 1;
            researchCost = 250;
            buildCost = 2500;
            upkeep = 250;
        }

        GameObject generic = Instantiate(buttonPrefab, toReplace.transform.parent.position, Quaternion.identity, toReplace.transform.parent);
        Button currentButton = generic.GetComponent<Button>();

        if (currentState.CheckUpgrade(upgrade) > 0)
        {
            currentButton.GetComponentInChildren<Text>().text = "IN QUEUE (" + currentState.CheckUpgrade(upgrade) + " TURNS)";
        }
        else if (!currentState.CheckResearched(upgrade) || buildCost > currentState.GetMoney())
        {
            currentButton.GetComponentInChildren<Text>().text = "CLOSE";
            currentButton.onClick.AddListener(() => Close(toRemove));
        }
        else
        {
            currentButton.GetComponentInChildren<Text>().text = "BUILD";
            currentButton.onClick.AddListener(() => BuildUpgrade(upgrade, constructionTime, researchCost, buildCost, upkeep, toRemove));
        }
    }

    public void UpdateUIElements()
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

        foreach (HexUnit unit in grid.GetUnits())
        {
            if (unit.UnitType == "Tourist Boat" || unit.UnitType == "Fishing Boat")
            {
                UnitBehaviour currentBehaviour = unit.gameObject.GetComponent<UnitBehaviour>();
                currentBehaviour.Execute();
            }
        }
        UpdateUIElements();
    }

    public void Research(Button clicked)
    {
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        GameObject researchPanel = Instantiate(researchPrefab, spawnAt, Quaternion.identity, transform);

        Button[] buttons = researchPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (currentState.CheckResearched(button.GetComponentInChildren<Text>().text))
            {
                button.interactable = false;
            }
            else
            {
                button.interactable = true;
                button.onClick.AddListener(() => ResearchUpgrade(button));
            }
        }
    }

    void ResearchUpgrade(Button clicked)
    {
        string name = clicked.GetComponentInChildren<Text>().text;
        GameObject window = clicked.transform.parent.parent.gameObject;

        int researchTime = 0;
        int researchCost = 0;

        if (name == "RADAR")
        {
            researchTime = 1;
            researchCost = 250;
        }

        if (currentState.GetResearch() >= researchCost)
        {
            currentState.QueueResearch(name, researchTime);
            Destroy(window);
            currentState.AddResearch(-researchCost);
            UpdateUIElements();
            StartCoroutine(DelayedResearch(name));
        }
    }

    IEnumerator DelayedResearch(string name)
    {
        yield return new WaitUntil(() => currentState.CheckResearchQueue(name) == 0);
        currentState.UnlockUpgrade(name);
        UpdateUIElements();
    }

    void Close(GameObject toRemove)
    {
        Destroy(toRemove);
    }

    public void UseRadar()
    {
        currentState.ActivateRadar();
        radarButton.interactable = false;
        int startTurn = currentState.GetTurn();
        StartCoroutine(OffRadar(startTurn));
    }

    IEnumerator OffRadar(int startTurn)
    {
        yield return new WaitUntil(() => currentState.GetTurn() > startTurn);
        currentState.DeactivateRadar();
        StartCoroutine(UnlockRadar());
    }
    IEnumerator UnlockRadar()
    {
        yield return new WaitUntil(() => currentState.FetchCD("RADAR") == 0);
        radarButton.interactable = true;
    }

    public Spawner GetSpawner()
    {
        return spawner;
    }

    public HexCell GetPlayerLocation()
    {
        return playerLocation;
    }
}