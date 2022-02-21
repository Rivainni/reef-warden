using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainUI : MonoBehaviour
{
    [SerializeField] HexGrid grid;
    [SerializeField] Spawner spawner;

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
    [SerializeField] GameObject queueDisplay;
    [SerializeField] GameObject endPrefab;
    [SerializeField] TimeController timeController;
    [SerializeField] Button radarButton;
    [SerializeField] ObjectivesDisplay objectivesDisplay;
    [SerializeField] CameraController cameraController;
    bool freeze;

    void Start()
    {
        freeze = false;
        currentState = initState;
        currentState.Clean();

        if (!currentState.CheckTutorial())
        {
            currentState.SetObjectives(TextRW.GetObjectives(1));
        }

        objectivesDisplay.currentState = currentState;
        objectivesDisplay.DisplayObjectives();
        // PointToObject(grid.GetUnits()[0].gameObject);
        minigameData.SetInspection();
        StartCoroutine(UIUpdateDelay());
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && currentState.GetHealth() > 0 && !freeze)
        {
            if (Input.GetMouseButtonDown(0))
            {
                grid.GetAudioManager().Play("Selected", 0);
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    if (grid.HasPath && grid.GetPlayerBehaviour().WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
                    {
                        grid.GetAudioManager().Play("Move", 0);
                        HexAction();
                    }
                }
                else if (!selectedUnit.movement && selectedUnit.ActionPoints > 0)
                {
                    DoPathfinding();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition)) != null || currentCell)
                {
                    HexAction();
                }
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
        grid.GetPlayerBehaviour().ClearPath();
        UpdateCurrentCell();

        if (currentCell)
        {
            Debug.Log("This is cell index " + currentCell.Index);
            if (!currentCell.Unit || selectedUnit == currentCell.Unit)
            {
                selectedUnit = null;
                currentState.SetMessage("No unit selected.");
            }
            else if (currentCell.Unit && currentCell.Unit.UnitType.Contains("Patrol Boat") || currentCell.Unit.UnitType == "Service Boat")
            {
                selectedUnit = currentCell.Unit;
                Debug.Log("Selected " + selectedUnit.UnitType);
                currentState.SetMessage("Selected Unit: " + selectedUnit.UnitType);
                grid.ShowUI(true);
            }
        }
        UpdateUIElements();
    }

    void DoPathfinding()
    {
        UpdateCurrentCell();
        if (currentCell && selectedUnit.IsValidDestination(currentCell) && selectedUnit.ActionPoints > 0)
        {
            grid.GetPlayerBehaviour().FindPath(selectedUnit.Location, currentCell, selectedUnit.ActionPoints);
        }
        else
        {
            grid.GetPlayerBehaviour().ClearPath();
        }
    }

    void DoMove()
    {
        selectedUnit.movement = true;
        selectedUnit.Travel(grid.GetPlayerBehaviour().GetPath());
        selectedUnit.ActionPoints = grid.GetPlayerBehaviour().WithinTurnPath(selectedUnit.ActionPoints);
        grid.GetPlayerBehaviour().ClearPath();
    }

    void HexAction()
    {
        Vector3 spawnAt = Input.mousePosition;
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition)) == null ? currentCell : grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        HexCell tempA = null;
        HexCell tempB = null;
        HexUnit targetA = null;
        HexUnit targetB = null;
        HexUnit targetC = null;
        int reefStructure = 0;

        List<string> contextMenuContent = new List<string>();

        if (selectedUnit)
        {
            if (selectedUnit.UnitType.Contains("Patrol Boat"))
            {
                if (grid.HasPath && grid.GetPlayerBehaviour().WithinTurnPath(selectedUnit.ActionPoints) < int.MaxValue && selectedUnit.ActionPoints > 0)
                {
                    contextMenuContent.Add("Patrol");

                    for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    {
                        HexCell toCheckCell = cell.GetNeighbor(d);
                        if (toCheckCell != null && toCheckCell.Unit != null)
                        {
                            if (toCheckCell.Unit.UnitType == "Fishing Boat" && !contextMenuContent.Contains("Catch Fisherman") && !toCheckCell.Unit.HasInteracted())
                            {
                                contextMenuContent.Add("Catch Fisherman");
                                tempA = toCheckCell;
                                targetA = toCheckCell.Unit;
                            }
                            else if (toCheckCell.Unit.UnitType == "Tourist Boat" && !contextMenuContent.Contains("Assist Mooring") && toCheckCell.Unit.GetAIBehaviour().HasStopped())
                            {
                                contextMenuContent.Add("Assist Mooring");
                                tempB = toCheckCell;
                                targetB = toCheckCell.Unit;
                            }
                            else if (toCheckCell.Unit.UnitType == "Tourist Boat" && !contextMenuContent.Contains("Inspect Tourist") && !toCheckCell.Unit.HasInteracted())
                            {
                                contextMenuContent.Add("Inspect Tourist");
                                tempB = toCheckCell;
                                targetB = toCheckCell.Unit;
                            }
                            else if (toCheckCell.Unit.UnitType == "Research Boat" && !contextMenuContent.Contains("Assist Researcher"))
                            {
                                contextMenuContent.Add("Assist Researcher");
                                tempB = toCheckCell;
                                targetB = toCheckCell.Unit;
                            }
                        }
                        else if (toCheckCell != null)
                        {
                            if (GlobalCellCheck.GetIsland(toCheckCell) > 0)
                            {
                                Debug.Log("reef structure" + GlobalCellCheck.GetIsland(toCheckCell));
                                if (currentState.FetchCD("CH" + GlobalCellCheck.GetIsland(toCheckCell)) == 0 && !contextMenuContent.Contains("Check Reef Health"))
                                {
                                    contextMenuContent.Add("Check Reef Health");
                                }
                                if (currentState.FetchCD("B") == 0 && toCheckCell.Type == "Land" && toCheckCell.Index == 465 && !contextMenuContent.Contains("Count Birds"))
                                {
                                    contextMenuContent.Add("Count Birds");
                                }
                                if (currentState.FetchCD("C" + GlobalCellCheck.GetIsland(toCheckCell)) == 0 && !contextMenuContent.Contains("Monitor Clams"))
                                {
                                    contextMenuContent.Add("Monitor Clams");
                                }
                                if (currentState.FetchCD("T") == 0 && !contextMenuContent.Contains("Tag Turtles") && currentState.CheckResearched("Species Tagging"))
                                {
                                    contextMenuContent.Add("Tag Turtles");
                                }
                                reefStructure = GlobalCellCheck.GetIsland(toCheckCell);
                            }
                        }
                    }
                }
            }
            else if (selectedUnit.UnitType.Contains("Service Boat"))
            {
                contextMenuContent.Add("Move");

                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCell toCheckCell = cell.GetNeighbor(d);
                    if (toCheckCell != null && toCheckCell.Unit != null)
                    {
                        if (toCheckCell.Unit.IsPatrolBoat())
                        {
                            contextMenuContent.Add("Repair");
                            targetC = toCheckCell.Unit;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            contextMenuContent.Add("Inspect");
            if (grid.CheckUpgradeCell(cell) && !cell.Upgrade)
            {
                contextMenuContent.Add("Build Upgrade");
            }
            else if (cell.Upgrade)
            {
                contextMenuContent.Add("Replace Upgrade");
            }
            if (cell.Upgrade)
            {
                contextMenuContent.Add("Demolish Upgrade");
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
                    currentButton.onClick.AddListener(() => CheckHealth(cell, contextMenu, reefStructure));
                }
                else if (item == "Count Birds")
                {
                    currentButton.onClick.AddListener(() => CountBirds(cell, contextMenu));
                }
                else if (item == "Monitor Clams")
                {
                    currentButton.onClick.AddListener(() => MonitorClam(selectedUnit, contextMenu, reefStructure));
                }
                else if (item == "Tag Turtles")
                {
                    currentButton.onClick.AddListener(() => TagTurtle(selectedUnit, contextMenu));
                }
                else if (item == "Assist Researcher")
                {
                    currentButton.onClick.AddListener(() => AssistResearcher(tempB, contextMenu, targetB));
                }
                else if (item == "Catch Fisherman")
                {
                    currentButton.onClick.AddListener(() => CatchFisherman(tempA, contextMenu, targetA));
                }
                else if (item == "Inspect Tourist")
                {
                    currentButton.onClick.AddListener(() => InspectTourist(tempB, contextMenu, targetB));
                }
                else if (item == "Assist Mooring")
                {
                    currentButton.onClick.AddListener(() => AssistMooring(tempB, contextMenu, targetB));
                }
                else if (item == "Inspect")
                {
                    currentButton.onClick.AddListener(() => Inspect(cell, contextMenu));
                }
                else if (item == "Repair")
                {
                    currentButton.onClick.AddListener(() => Repair(contextMenu, targetC));
                }
                else if (item == "Move")
                {
                    currentButton.onClick.AddListener(() => AfterAction(contextMenu));
                }
                else if (item == "Build Upgrade" || item == "Replace Upgrade")
                {
                    currentButton.onClick.AddListener(() => DoUpgrade(contextMenu));
                }
                else if (item == "Demolish Upgrade")
                {
                    currentButton.onClick.AddListener(() => DemolishUpgrade(contextMenu, cell));
                }
            }
        }
    }

    void AfterAction(GameObject remove)
    {
        grid.GetAudioManager().Play("Next", 0);
        DoMove();
        if (selectedUnit.IsPatrolBoat())
        {
            selectedUnit.DecreaseHP();
        }
        UpdateUIElements();
        Destroy(remove);
    }

    void Inspect(HexCell target, GameObject remove)
    {
        string message = "";
        if (target.Unit)
        {
            message += "That is a " + target.Unit.UnitType + ".\n";
            if (target.Unit.UnitType == "Fishing Boat")
            {
                message += "It's not supposed to be here. Get em!";
            }
            else if (target.Unit.UnitType == "Tourist Boat")
            {
                message += "It has a permit - make sure to check if its manifests match!";
            }
        }
        else if (target.Structure)
        {
            message += "That is a " + target.Structure.StructureType + ".\n";
        }
        else if (target.Type == "Land")
        {
            message += "A pristine plot of sand.\n";
        }
        else if (target.Type == "Water")
        {
            message += "Nothing but water. It's clean enough that you can see fish swimming under it.\n";
        }

        currentState.SetMessage(message);
        UpdateUIElements();
        Destroy(remove);
    }

    void Repair(GameObject remove, HexUnit target)
    {
        AfterAction(remove);
        target.RestoreHP();
    }

    void Patrol(HexCell destination, GameObject remove)
    {
        currentState.incrementLevelCounters("patrol");
        float factor = destination.Distance * 0.5f;
        playerLocation = destination;
        // Debug.Log("You are at " + playerLocation.coordinates.ToString());
        currentState.AddSecurity(factor);
        AfterAction(remove);
    }

    void CheckHealth(HexCell destination, GameObject remove, int reefStructure)
    {
        currentState.incrementLevelCounters("health");
        currentState.UpdateHealth();
        currentState.AddResearch(250);
        currentState.ResetCD("CH" + reefStructure);
        AfterAction(remove);
    }

    void CountBirds(HexCell destination, GameObject remove)
    {
        currentState.incrementLevelCounters("bird");
        currentState.AddResearch(500);
        currentState.ResetCD("B");
        AfterAction(remove);
    }

    void MonitorClam(HexUnit unit, GameObject remove, int reefStructure)
    {
        AfterAction(remove);
        int startTurn = currentState.GetTurn();
        unit.ToggleBusy();
        unit.ActionPoints = 0;
        StartCoroutine(WaitForTwoTurns());
        IEnumerator WaitForTwoTurns()
        {
            yield return new WaitUntil(() => currentState.GetTurn() == startTurn + 2);
            unit.ToggleBusy();
            currentState.AddResearch(1000);
            currentState.ResetCD("C" + reefStructure);
            currentState.incrementLevelCounters("clam");
        }
    }

    void TagTurtle(HexUnit unit, GameObject remove)
    {
        AfterAction(remove);
        currentState.AdjustMoney(-10000);
        int startTurn = currentState.GetTurn();
        unit.ToggleBusy();
        unit.ActionPoints = 0;
        StartCoroutine(WaitForTwoTurns());
        IEnumerator WaitForTwoTurns()
        {
            yield return new WaitUntil(() => currentState.GetTurn() == startTurn + 2);
            unit.ToggleBusy();
            currentState.AddResearch(1000);
            currentState.ResetCD("T");
        }
    }

    void AssistResearcher(HexCell destination, GameObject remove, HexUnit target)
    {
        spawner.DestroyUnit(target);
        currentState.AddResearch(10000);
        AfterAction(remove);
    }

    void InspectTourist(HexCell destination, GameObject remove, HexUnit target)
    {
        StartCoroutine(InspectTouristGame(target));
        AfterAction(remove);
    }

    IEnumerator InspectTouristGame(HexUnit target)
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => selectedUnit.movement == false);
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        GameObject gamePanel = Instantiate(doublePanelPrefab, spawnAt, Quaternion.identity, transform);

        // determine if value should be true or not
        int correct = Random.Range(0, 2);
        bool correctValue = correct < 1;

        int random = Random.Range(0, 30);

        for (int i = 0; i < 2; i++)
        {
            GameObject toShow = Instantiate(textPrefab, gamePanel.transform.GetChild(i).position, Quaternion.identity, gamePanel.transform.GetChild(i));
            Text matchText = toShow.GetComponent<Text>();
            if (correctValue)
            {
                matchText.text = minigameData.GenerateSet(random, correct);
            }
            else
            {
                matchText.text = minigameData.GenerateSet(random, correct + i);
            }
        }

        GameObject approvePanel = Instantiate(panelPrefab, gamePanel.transform.position, Quaternion.identity, gamePanel.transform);
        GameObject buttonA = Instantiate(buttonPrefab, approvePanel.transform.position, Quaternion.identity, approvePanel.transform);
        GameObject buttonB = Instantiate(buttonPrefab, approvePanel.transform.position, Quaternion.identity, approvePanel.transform);
        Button actualButtonA = buttonA.GetComponent<Button>();
        Button actualButtonB = buttonB.GetComponent<Button>();

        actualButtonA.GetComponentInChildren<Text>().text = "Approve";
        actualButtonB.GetComponentInChildren<Text>().text = "Disapprove";

        actualButtonA.onClick.AddListener(() => InspectTouristGameApprove(correctValue, target, gamePanel));
        actualButtonB.onClick.AddListener(() => InspectTouristGameDisapprove(correctValue, target, gamePanel));
    }

    void InspectTouristGameApprove(bool correctValue, HexUnit target, GameObject toRemove)
    {
        Destroy(toRemove);
        if (correctValue)
        {
            currentState.AddSecurity(5);
            currentState.SetMessage("Inspection correct.");
            UpdateUIElements();
        }
        currentState.AddTouristScore();
        target.SetInteracted();
    }

    void InspectTouristGameDisapprove(bool correctValue, HexUnit target, GameObject toRemove)
    {
        if (!correctValue)
        {
            currentState.AddSecurity(5);
            currentState.SetMessage("Inspection correct.");
            UpdateUIElements();
        }
        Destroy(toRemove);
        currentState.AddTouristScore();
        target.SetInteracted();
    }

    void AssistMooring(HexCell destination, GameObject remove, HexUnit target)
    {
        // probably a minigame
        AIBehaviour current = target.GetComponent<AIBehaviour>();
        current.Moor();
        currentState.AddTouristScore();
        target.Location.ResetColor();
        AfterAction(remove);
    }

    void CatchFisherman(HexCell destination, GameObject remove, HexUnit target)
    {
        spawner.DestroyUnit(target);
        currentState.AddSecurity(2);
        AfterAction(remove);
        currentState.AddCatchScore();
        target.SetInteracted();
    }

    void DemolishUpgrade(GameObject remove, HexCell target)
    {
        Destroy(remove);
        if (target.Upgrade.UpgradeType == "AIS")
        {
            currentState.RemoveAIS();
        }
        else if (target.Upgrade.UpgradeType == "Satellite Internet")
        {
            currentState.RemoveSAT();
        }
        else if (target.Upgrade.UpgradeType == "Souvenir Stand")
        {
            currentState.RemoveSS();
        }
        spawner.DestroyUpgrade(target.Upgrade);
    }
    void DoUpgrade(GameObject remove)
    {
        Destroy(remove);
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        // clear the context menu
        List<string> contextMenuContent = new List<string>();

        foreach (TextRW.UpgradeItem item in TextRW.GetUpgrades())
        {
            if (item.BuildCost > 0)
            {
                contextMenuContent.Add(item.Name);
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
                        currentButton.onClick.AddListener(() => UpgradeText(item, currentButton, upgradePanel, cell));
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

    void BuildUpgrade(string upgrade, int constructionTime, int researchCost, int buildCost, int upkeep, GameObject remove, HexCell target)
    {
        currentState.QueueUpgrade(upgrade, constructionTime);
        Destroy(remove);
        currentState.AdjustMoney(-buildCost);
        currentState.AddManpower(-1);
        UpdateUIElements();
        UpdateUIQueue(upgrade, 1, constructionTime);
        StartCoroutine(DelayedBuild(upgrade, constructionTime, researchCost, buildCost, upkeep, target));
    }

    IEnumerator DelayedBuild(string upgrade, int constructionTime, int researchCost, int buildCost, int upkeep, HexCell target)
    {
        yield return new WaitUntil(() => currentState.CheckUpgrade(upgrade) == 0);
        UpdateUIQueue(upgrade, 1, 0);
        if (currentCell.Upgrade)
        {
            spawner.DestroyUpgrade(target.Upgrade);
        }
        if (upgrade != "Double-engine Patrol Boat")
        {
            spawner.SpawnUpgrade(target, upgrade, constructionTime, researchCost, buildCost, upkeep);
            currentState.AddUpgrade(upgrade);
        }
        else
        {
            spawner.RandomSpawn("Tier 2 Patrol Boat");
            currentState.AddManpower(-4);
        }
        currentState.AdjustIncome(-upkeep);
        currentState.AddManpower(1);
        if (upgrade == "RADAR")
        {
            radarButton.interactable = true;
        }
        else if (upgrade == "AIS")
        {
            currentState.AddAIS();
        }
        else if (upgrade == "Satellite Internet")
        {
            currentState.AddSAT();
        }
        else if (upgrade == "Souvenir Stand")
        {
            currentState.AddSS();
        }
        UpdateUIElements();
    }

    void UpgradeText(string upgrade, Button button, GameObject toRemove, HexCell target)
    {
        GameObject toReplace = button.transform.parent.parent.GetChild(1).GetChild(0).gameObject;
        Button[] rem = toReplace.transform.parent.gameObject.GetComponentsInChildren<Button>();
        int constructionTime = 0;
        int researchCost = 0;
        int buildCost = 0;
        int upkeep = 0;

        TextRW.UpgradeItem curr = TextRW.GetUpgrade(upgrade);

        toReplace.GetComponent<Text>().text = curr.Description;
        toReplace.GetComponent<Text>().text += "Build: " + curr.BuildCost + "\n";
        toReplace.GetComponent<Text>().text += "Research: " + curr.ResearchCost + "\n";
        toReplace.GetComponent<Text>().text += "Upkeep: " + curr.Upkeep + "\n";
        constructionTime = curr.Turns;
        researchCost = curr.ResearchCost;
        buildCost = curr.BuildCost;
        upkeep = curr.Upkeep;

        foreach (Button trash in rem)
        {
            Destroy(trash.gameObject);
        }

        GameObject generic = Instantiate(buttonPrefab, toReplace.transform.parent.position, Quaternion.identity, toReplace.transform.parent);
        Button currentButton = generic.GetComponent<Button>();

        if (currentState.CheckUpgrade(upgrade) > 0)
        {
            currentButton.GetComponentInChildren<Text>().text = "IN QUEUE (" + currentState.CheckUpgrade(upgrade) + " TURNS)";
            currentButton.onClick.AddListener(() => Close(toRemove));
        }
        else if (!currentState.CheckResearched(upgrade) || buildCost > currentState.GetMoney())
        {
            currentButton.GetComponentInChildren<Text>().text = "CLOSE";
            currentButton.onClick.AddListener(() => Close(toRemove));
        }
        else if (currentState.CheckBuilt(upgrade) && upgrade == "Basketball Court")
        {
            currentButton.GetComponentInChildren<Text>().text = "USE";
            currentButton.onClick.AddListener(() => Use(upgrade, toRemove));
        }
        else
        {
            currentButton.GetComponentInChildren<Text>().text = "BUILD";
            currentButton.onClick.AddListener(() => BuildUpgrade(upgrade, constructionTime, researchCost, buildCost, upkeep, toRemove, target));
        }
    }

    public void UpdateUIElements()
    {
        UITextUpdate[] toUpdate = GetComponentsInChildren<UITextUpdate>();
        foreach (UITextUpdate item in toUpdate)
        {
            // Debug.Log("Error at " + item.gameObject.name);
            item.UpdateText();
        }
    }

    public PlayerState GetPlayerState()
    {
        return currentState;
    }

    public void EndTurn(Button clicked)
    {
        grid.GetAudioManager().Play("Next", 0);
        currentState.EndTurn();
        objectivesDisplay.DisplayObjectives();
        clicked.GetComponentInChildren<Text>().text = "TURN " + currentState.GetTurn();
        grid.ResetPoints();
        selectedUnit = null;
        grid.GetPlayerBehaviour().ClearPath();


        timeController.ForwardTime();
        StartCoroutine(Movement(clicked));

        // makes everything visible again
        if (timeController.IsDay())
        {
            for (int i = 0; i < grid.GetUnits().Count; i++)
            {
                HexUnit currentUnit = grid.GetUnits()[i];

                if (!currentUnit.IsVisible)
                {
                    grid.GetUnits()[i].ToggleVisibility();
                }
            }

            foreach (HexCell cell in grid.GetCells())
            {
                if (!cell.Structure)
                {
                    cell.IncreaseVisibility();
                }
            }
        }

        foreach (HexUnit unit in grid.GetUnits())
        {
            if (unit.UnitType == "Tourist Boat" || unit.UnitType == "Fishing Boat")
            {
                AIBehaviour currentBehaviour = unit.gameObject.GetComponent<AIBehaviour>();
                currentBehaviour.Execute();
            }
        }
        SpawnUnits();
        spawner.DestroyUnits();
        UpdateUIElements();

        if (currentState.GetHealth() <= 0)
        {
            GenerateEndScreen("Defeat.");
        }
        else if (currentState.CheckResearched("Total Protection"))
        {
            GenerateEndScreen("Victory!");
        }
    }

    void GenerateEndScreen(string text)
    {
        GameObject end = Instantiate(endPrefab, transform.position, Quaternion.identity, transform);
        Button button = end.transform.GetChild(1).gameObject.GetComponent<Button>();
        Text curr = button.GetComponentInChildren<Text>();

        curr.text = text;
    }

    void SpawnUnits()
    {
        if (!currentState.CheckTutorial())
        {
            if (timeController.IsDay() && !currentState.SpawnedDay())
            {
                currentState.ResetFisherman();
                foreach (HexUnit unit in grid.GetUnits())
                {
                    if (unit.UnitType == "Fishing Boat")
                    {
                        spawner.DestroyUnit(unit);
                    }
                }
                int max = currentState.GetLevel();
                if (currentState.GetTourists() == 0)
                {
                    currentState.SetTourists(max);
                }

                for (int i = 0; i < max; i++)
                {
                    spawner.RandomSpawn("Tourist Boat");
                }

                if (currentState.GetLevel() == 5 && currentState.FetchCD("R") == 0)
                {
                    spawner.RandomSpawn("Research Boat");
                    currentState.ResetCD("R");
                }

                currentState.ToggleDaySpawn();
            }
            else if (!timeController.IsDay() && !currentState.SpawnedNight())
            {
                int max = currentState.GetLevel();
                // int random = Random.Range(0, max * ((int)currentState.GetSecurity() / 100));

                for (int i = 0; i < max; i++)
                {
                    spawner.RandomSpawn("Fishing Boat");
                    currentState.AddFisherman(1);
                }

                currentState.ToggleNightSpawn();
            }
        }
    }

    IEnumerator Movement(Button clicked)
    {
        clicked.interactable = false;
        yield return new WaitUntil(() => CheckMovement() == false);
        yield return new WaitUntil(() => timeController.CheckPause());
        clicked.interactable = true;
    }

    bool CheckMovement()
    {
        foreach (HexUnit unit in grid.GetUnits())
        {
            if (unit.movement)
            {
                return true;
            }
        }

        return false;
    }

    public void Research(Button clicked)
    {
        grid.GetAudioManager().Play("Next", 0);
        Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        GameObject researchPanel = Instantiate(researchPrefab, spawnAt, Quaternion.identity, transform);

        Button[] buttons = researchPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (currentState.CheckResearched(button.GetComponentInChildren<Text>().text) || TextRW.GetUpgrade(button.GetComponentInChildren<Text>().text).ResearchCost > currentState.GetResearch())
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
        grid.GetAudioManager().Play("Next", 0);
        string name = clicked.GetComponentInChildren<Text>().text;
        GameObject window = clicked.transform.parent.parent.gameObject;

        int researchTime = 0;
        int researchCost = 0;

        TextRW.UpgradeItem curr = TextRW.GetUpgrade(name);

        researchTime = curr.Turns;
        researchCost = curr.ResearchCost;
        Debug.Log(name + ", " + researchTime + " turns and " + researchCost + " points.");

        if (currentState.GetResearch() >= researchCost)
        {
            currentState.QueueResearch(name, researchTime);
            Destroy(window);
            currentState.AddResearch(-researchCost);
            UpdateUIElements();
            UpdateUIQueue(name, 0, researchTime);
            StartCoroutine(DelayedResearch(name));
        }
    }

    IEnumerator DelayedResearch(string name)
    {
        yield return new WaitUntil(() => currentState.CheckResearchQueue(name) == 0);
        UpdateUIQueue(name, 0, 0);
        currentState.UnlockUpgrade(name);
        UpdateUIElements();
    }

    void Close(GameObject toRemove)
    {
        grid.GetAudioManager().Play("Prev", 0);
        Destroy(toRemove);
    }

    void Use(string upgrade, GameObject toRemove)
    {
        if (upgrade == "Basketball Court")
        {
            currentState.ResetCD("BB");
            if (currentState.CheckSAT())
            {
                currentState.AddMorale(currentState.GetLevel() * 5 * 1.2f);
            }
            else
            {
                currentState.AddMorale(currentState.GetLevel() * 5);
            }
        }
        Destroy(toRemove);
    }

    void UpdateUIQueue(string name, int choice, int time)
    {
        Text[] check = queueDisplay.GetComponentsInChildren<Text>();
        bool inQueue = false;
        Text toUpdate = null;
        for (int i = 0; i < check.Length; i++)
        {
            if (check[i].text.Contains(name))
            {
                inQueue = true;
                toUpdate = check[i];
                break;
            }
        }

        if (inQueue)
        {
            if (time == 0)
            {
                Destroy(toUpdate.transform.gameObject);
            }
            else
            {
                if (choice == 1)
                {
                    toUpdate.text = "B: ";
                }
                else
                {
                    toUpdate.text = "R: ";
                }

                toUpdate.text += name + " - ";
                toUpdate.text += time;
            }
        }
        else
        {
            GameObject update = Instantiate(textPrefab, queueDisplay.transform.position, Quaternion.identity, queueDisplay.transform);
            toUpdate = update.GetComponent<Text>();

            if (choice == 1)
            {
                toUpdate.text = "B: ";
            }
            else
            {
                toUpdate.text = "R: ";
            }

            toUpdate.text += name + " - ";
            toUpdate.text += time;
        }
        UpdateUIElements();
    }

    public void UseRadar()
    {
        grid.GetAudioManager().Play("Next", 0);
        currentState.ActivateRadar();
        radarButton.interactable = false;
        int startTurn = currentState.GetTurn();
        StartCoroutine(OffRadar(startTurn));
    }

    IEnumerator OffRadar(int startTurn)
    {
        Stack<HexUnit> affectedUnits = new Stack<HexUnit>();

        for (int i = 0; i < grid.GetUnits().Count; i++)
        {
            HexUnit currentUnit = grid.GetUnits()[i];

            if (!currentUnit.IsVisible)
            {
                affectedUnits.Push(currentUnit);
                grid.GetUnits()[i].ToggleVisibility();
            }
        }

        yield return new WaitUntil(() => currentState.GetTurn() > startTurn);

        while (affectedUnits.Count > 0)
        {
            affectedUnits.Pop().ToggleVisibility();
        }

        currentState.DeactivateRadar();
        StartCoroutine(UnlockRadar());
    }
    IEnumerator UnlockRadar()
    {
        yield return new WaitUntil(() => currentState.FetchCD("RADAR") == 0);
        radarButton.interactable = true;
    }

    public void FreezeInput(bool toggle)
    {
        freeze = toggle;
    }

    public Spawner GetSpawner()
    {
        return spawner;
    }

    public HexCell GetPlayerLocation()
    {
        return playerLocation;
    }

    public void DisplayTutorialObjective(string objective)
    {
        GetPlayerState().SetObjectives(objective);
        objectivesDisplay.DisplayObjectives();
    }

    IEnumerator UIUpdateDelay()
    {
        yield return new WaitUntil(() => currentState != null);
        UpdateUIElements();
    }

    public CameraController GetCameraController()
    {
        return cameraController;
    }

    public HexGrid GetHexGrid()
    {
        return grid;
    }
}