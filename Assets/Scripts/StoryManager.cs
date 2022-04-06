using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [SerializeField] GameObject storyUI;
    [SerializeField] MainUI mainUI;
    [SerializeField] Text storyText;
    [SerializeField] Text characterName;
    [SerializeField] GameObject characterSprites;
    [SerializeField] Image secondarySprite;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] GameObject inputPrefab;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] bool cutscene = true;
    [SerializeField] PlayerState initState;
    Queue<string> inputStream = new Queue<string>();
    bool pause = false;
    bool primarySpeaker = false;

    // Start is called before the first frame update
    void Start()
    {
        secondarySprite.enabled = false;
    }

    public void StartDialogue(Queue<string> dialogue)
    {
        if (!cutscene)
        {
            mainUI.GetCameraController().FreezeCamera(true); // freeze input
        }
        storyUI.SetActive(true); // open the dialogue box
        inputStream = dialogue; // store the dialogue from dialogue trigger
        PrintDialogue(); // Prints out the first line of dialogue
    }

    public void AdvanceDialogue() // call when a player presses a button in Dialogue Trigger
    {
        if (!pause)
        {
            PrintDialogue();
        }
    }

    void PrintDialogue()
    {
        if (inputStream.Count == 0 || inputStream.Peek().Contains("EndQueue")) // special phrase to stop dialogue
        {
            inputStream.Dequeue(); // Clear Queue
            EndDialogue();
        }
        else if (inputStream.Peek().Contains("[NAME="))
        {
            string name = inputStream.Peek();
            name = inputStream.Dequeue().Substring(name.IndexOf('=') + 1, name.IndexOf(']') - (name.IndexOf('=') + 1));
            characterName.text = name;
            primarySpeaker = true;
            PrintDialogue();
        }
        else if (inputStream.Peek().Contains("[ACTION="))
        {
            string action = inputStream.Peek();
            action = inputStream.Dequeue().Substring(action.IndexOf('=') + 1, action.IndexOf(']') - (action.IndexOf('=') + 1));

            if (action == "EnterName")
            {
                Vector3 spawnAt = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
                GameObject panel = Instantiate(panelPrefab, spawnAt, Quaternion.identity, storyUI.transform);
                GameObject input = Instantiate(inputPrefab, panel.transform.position, Quaternion.identity, panel.transform);
                GameObject confirm = Instantiate(buttonPrefab, panel.transform.position, Quaternion.identity, panel.transform);

                Button confirmButton = confirm.GetComponent<Button>();
                confirmButton.GetComponentInChildren<Text>().text = "CONFIRM";

                confirmButton.onClick.AddListener(() => SetName(input.GetComponent<InputField>(), panel));
                pause = true;
            }
            else if (action.Contains("EnterCharacter"))
            {
                string newCharacter = action.Substring(14);
                if (!cutscene)
                {
                    characterName.text = newCharacter;
                }
                AddSprite(newCharacter);
                if (mainUI)
                {
                    if (!mainUI.GetPlayerState().CheckTutorial())
                    {
                        primarySpeaker = false;
                    }
                }
            }
            else if (action == "ExitCharacter")
            {
                secondarySprite.enabled = false;
            }
            else if (action == "BG")
            {
                Image bg1 = storyUI.transform.GetChild(1).GetComponent<Image>();
                bg1.enabled = true;

                Image bg0 = storyUI.transform.GetChild(0).GetComponent<Image>();
                bg0.enabled = false;
            }
            else if (mainUI.GetPlayerState().CheckTutorial())
            {
                Tutorial(action);
            }

            PrintDialogue();
        }
        else if (inputStream.Peek().Contains("[CONDITIONAL="))
        {
            string condition = inputStream.Peek();
            condition = inputStream.Dequeue().Substring(condition.IndexOf('=') + 1, condition.IndexOf(']') - (condition.IndexOf('=') + 1));
            mainUI.GetCameraController().FreezeCamera(false);

            if (condition == "UpgradeAIS" && !mainUI.GetPlayerState().CheckBuilt("AIS"))
            {
                inputStream.Dequeue();
            }
            else if (condition == "Spotted")
            {
                List<int> playerIndices = new List<int>();
                for (int i = 0; i < mainUI.GetHexGrid().GetUnits().Count; i++)
                {
                    if (mainUI.GetHexGrid().GetUnits()[i].UnitType.Contains("Patrol Boat"))
                    {
                        playerIndices.Add(i);
                    }
                }
                storyUI.SetActive(false);
                StartCoroutine(WaitForPlayer());
                IEnumerator WaitForPlayer()
                {
                    yield return new WaitUntil(() => Scans() || IsOver());
                    if (Scans())
                    {
                        storyUI.SetActive(true);
                        mainUI.GetCameraController().FreezeCamera(true);
                    }
                    else
                    {
                        EndDialogue();
                        mainUI.GetCameraController().FreezeCamera(false);
                    }
                }

                bool Scans()
                {
                    foreach (int index in playerIndices)
                    {
                        if (mainUI.GetHexGrid().GetUnits()[index].ScanFor("Fishing Boat"))
                        {
                            return true;
                        }
                    }

                    if (mainUI.GetPlayerState().GetRadarState())
                    {
                        return true;
                    }
                    return false;
                }

                bool IsOver()
                {
                    if (mainUI.GetTimeController().IsDay())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (condition == "Caught")
            {

            }
            PrintDialogue();
        }
        else if (inputStream.Peek().Contains("{0}"))
        {
            string current = inputStream.Dequeue();

            int stop = 0;
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] == ':')
                {
                    stop = i;
                    break;
                }
            }
            string expression = current.Substring(0, stop);
            storyText.text = string.Format(current.Substring(stop + 1), initState.GetName());
            SwitchSprites(characterName.text, expression);
        }
        else if (inputStream.Peek().Contains(":"))
        {
            string current = inputStream.Dequeue();

            int stop = 0;
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] == ':')
                {
                    stop = i;
                    break;
                }
            }
            string expression = current.Substring(0, stop);
            storyText.text = current.Substring(stop + 1);
            SwitchSprites(characterName.text, expression, primarySpeaker);
        }
        else
        {
            storyText.text = inputStream.Dequeue();
        }
    }

    public void EndDialogue()
    {
        if (!cutscene)
        {
            mainUI.GetCameraController().FreezeCamera(false);
        }

        storyText.text = "";
        characterName.text = "";
        inputStream.Clear();
        storyUI.SetActive(false);

        if (cutscene)
        {
            SceneManager.LoadSceneAsync("Main Game");
            cutscene = false;
            initState.StartTutorial();
        }
        else if (mainUI.GetPlayerState().CheckTutorial())
        {
            mainUI.GetPlayerState().EndTutorial();
            mainUI.GetPlayerState().AddLevel();
        }
    }

    void SetName(InputField input, GameObject toRemove)
    {
        initState.SetName(input.text);
        Destroy(toRemove);
        pause = false;
        PrintDialogue();
    }

    void SwitchSprites(string character, string expression, bool primary = false)
    {
        Image current;
        if (primary)
        {
            current = characterSprites.transform.GetChild(0).GetComponent<Image>();
        }
        else
        {
            current = characterSprites.transform.GetChild(1).GetComponent<Image>();
        }

        string toLoad = character + expression;
        Sprite newSprite;
        if (expression == "Happy1" || expression == "Happy2")
        {
            newSprite = Resources.Load<Sprite>("Characters/" + expression.Substring(0, 5) + "/" + toLoad);
        }
        else
        {
            newSprite = Resources.Load<Sprite>("Characters/" + expression + "/" + toLoad);
        }
        current.sprite = newSprite;
    }

    // same as above
    void AddSprite(string character)
    {
        secondarySprite.enabled = true;
        string toLoad = character;
        Sprite newSprite = Resources.Load<Sprite>("Characters/Idle/" + toLoad + "Idle");
        secondarySprite.sprite = newSprite;
    }


    // separated tutorial because it's built different
    void Tutorial(string action)
    {
        mainUI.GetCameraController().FreezeCamera(false);
        // TUTORIAL STUFF
        if (action == "WASD")
        {
            mainUI.FreezeInput(true);
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Move the camera with WASD.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S));
                yield return new WaitForSeconds(3.0f);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "RF")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Zoom in using the Mousewheel, R, or F.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.F) || Input.mouseScrollDelta.y != 0);
                yield return new WaitForSeconds(3.0f);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
                mainUI.FreezeInput(false);
            }
        }
        else if (action == "Patrol")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Patrol to the indicated location.");
            mainUI.FreezeTurnUI(true);
            mainUI.GetSpawner().AddCellWaypoint(mainUI.GetHexGrid().GetCells()[260]);
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu() && mainUI.FindInContextMenu("Patrol"));
                mainUI.RailroadContextMenu("Patrol");
                yield return new WaitUntil(() => mainUI.GetHexGrid().GetUnits()[0].Location == mainUI.GetHexGrid().GetCells()[260]);
                mainUI.GetSpawner().DestroyWaypoint(mainUI.GetHexGrid().FindWaypoint(mainUI.GetHexGrid().GetCells()[260]));
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
                mainUI.FreezeTurnUI(false);
            }
        }
        else if (action == "CheckReefHealth")
        {
            storyUI.SetActive(false);
            mainUI.FreezeTurnUI(true);
            mainUI.DisplayTutorialObjective("Check the reef health.");
            mainUI.GetSpawner().AddCellWaypoint(mainUI.GetHexGrid().GetCells()[259]);
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu() && mainUI.FindInContextMenu("Check Reef Health"));
                mainUI.RailroadContextMenu("Check Reef Health");
                yield return new WaitUntil(() => !mainUI.GetPlayerState().CheckHealthNeeded() && mainUI.GetHexGrid().GetUnits()[0].Location == mainUI.GetHexGrid().GetCells()[259]);
                mainUI.GetSpawner().DestroyWaypoint(mainUI.GetHexGrid().FindWaypoint(mainUI.GetHexGrid().GetCells()[259]));
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
                mainUI.FreezeTurnUI(false);
            }
        }
        else if (action == "InspectTourist1")
        {
            mainUI.GetSpawner().TutorialSpawn("Tourist Boat");
            mainUI.GetSpawner().AddUnitWaypoint(mainUI.GetHexGrid().GetCells()[300]);
            mainUI.GetPlayerState().AddTourists(1);
            mainUI.GetPlayerState().ToggleDaySpawn();
            mainUI.UpdateUIElements();
            mainUI.DisplayTutorialObjective("Look for the tourist.");
        }
        else if (action == "MoveAttempt")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Go towards the tourist.");
            HexCell currentLocation = mainUI.GetHexGrid().GetUnits()[0].Location;
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetHexGrid().GetUnits()[0].Location != currentLocation);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "EndTurn")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("End the turn.");
            int currentTurn = mainUI.GetPlayerState().GetTurn();
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetTurn() > currentTurn);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "InspectTourist2")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Do a random inspection on the tourist.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu() && mainUI.FindInContextMenu("Inspect Tourist"));
                mainUI.RailroadContextMenu("Inspect Tourist");
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetTouristScore() > 0);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "InspectTourist3")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Help the tourist with mooring.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu() && mainUI.FindInContextMenu("Assist Mooring"));
                mainUI.RailroadContextMenu("Assist Mooring");
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetTouristScore() > 1);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "Info")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Open the info menu.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu() && mainUI.FindInContextMenu("Assist Researcher"));
                yield return new WaitUntil(() => !mainUI.HasActiveContextMenu());
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "ServiceMoved")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Move the service boat.");
            HexCell start = mainUI.GetHexGrid().GetUnits()[1].Location;
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetHexGrid().GetUnits()[1].Location != start);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "Refuel")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Rendezvous with the Service Boat.");
            mainUI.GetSpawner().AddUnitWaypoint(mainUI.GetHexGrid().GetUnits()[1].Location);
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetHexGrid().GetUnits()[0].HP == 100);
                mainUI.GetSpawner().DestroyWaypoint(mainUI.GetHexGrid().FindWaypoint(mainUI.GetHexGrid().GetUnits()[1]));
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "MoveBack")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Return to the ranger station (next to the boat).");
            mainUI.GetSpawner().AddCellWaypoint(mainUI.GetHexGrid().GetCells()[261]);
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu());
                mainUI.RailroadContextMenu("Patrol");
                yield return new WaitUntil(() => mainUI.GetPlayerLocation().coordinates.ToString() == "(6, 10)");
                mainUI.GetSpawner().DestroyWaypoint(mainUI.GetHexGrid().FindWaypoint(mainUI.GetHexGrid().GetCells()[261]));
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "Research")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Research the RADAR.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().CheckResearched("RADAR"));
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "Build")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Build a RADAR.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu());
                mainUI.RailroadContextMenu("Build Upgrade");
                yield return new WaitUntil(() => mainUI.GetPlayerState().CheckBuilt("RADAR"));
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "UseRADAR")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Use the RADAR.");
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetRadarState());
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "CatchFisherman")
        {
            storyUI.SetActive(false);
            StartCoroutine(WaitForPlayer());
            mainUI.GetSpawner().TutorialSpawn("Fishing Boat");
            mainUI.GetPlayerState().AddFisherman(1);
            mainUI.GetPlayerState().ToggleNightSpawn();
            mainUI.DisplayTutorialObjective("Catch the Fishing Boat.");
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetCatchScore() > 0);
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
        }
        else if (action == "CheckReefHealth2")
        {
            storyUI.SetActive(false);
            mainUI.DisplayTutorialObjective("Check the reef health.");

            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.HasActiveContextMenu() && mainUI.FindInContextMenu("Check Reef Health"));
                mainUI.RailroadContextMenu("Check Reef Health");
                yield return new WaitUntil(() => !mainUI.HasActiveContextMenu());
                storyUI.SetActive(true);
                mainUI.GetCameraController().FreezeCamera(true);
            }
            mainUI.GetPlayerState().RemoveObjective("Check the reef health.");
        }
    }
}