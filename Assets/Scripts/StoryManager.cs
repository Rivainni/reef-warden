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
    [SerializeField] Text objectives;
    Queue<string> inputStream = new Queue<string>();

    // Start is called before the first frame update
    void Start()
    {
        secondarySprite.enabled = false;
    }

    public void StartDialogue(Queue<string> dialogue)
    {
        storyUI.SetActive(true); // open the dialogue box
        // isOpen = true;
        inputStream = dialogue; // store the dialogue from dialogue trigger
        PrintDialogue(); // Prints out the first line of dialogue
    }

    public void AdvanceDialogue() // call when a player presses a button in Dialogue Trigger
    {
        PrintDialogue();
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
            }
            else if (action.Contains("EnterCharacter"))
            {
                string newCharacter = action.Substring(14);
                AddSprite(newCharacter);
            }
            else if (action == "ExitCharacter")
            {
                secondarySprite.enabled = false;
            }
            else
            {
                mainUI.GetPlayerState().StartTutorial();
                Tutorial(action);
            }

            // if (SceneManager.GetActiveScene().name == "Main Game")
            // {
            //     mainUI.GetPlayerState().StartTutorial();
            //     if (mainUI.GetPlayerState().CheckTutorial())
            //     {
            //         Debug.Log("WTF");
            //         Tutorial(action);
            //     }
            // }

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
            SwitchSprites(characterName.text, expression);
        }
        else
        {
            storyText.text = inputStream.Dequeue();
        }
    }

    public void EndDialogue()
    {
        storyText.text = "";
        characterName.text = "";
        inputStream.Clear();
        storyUI.SetActive(false);
        // isOpen = false;
        if (cutscene)
        {
            SceneManager.LoadScene("Main Game");
        }
        else
        {
            mainUI.GetPlayerState().EndTutorial();
        }
    }

    void SetName(InputField input, GameObject toRemove)
    {
        initState.SetName(input.text);
        Destroy(toRemove);
        PrintDialogue();
    }

    void SwitchSprites(string character, string expression)
    {
        Image[] current = characterSprites.GetComponentsInChildren<Image>();
        string toLoad = character + expression;
        Sprite newSprite = Resources.Load<Sprite>("Characters/" + expression + "/" + toLoad);

        foreach (Image item in current)
        {
            if (item.sprite.name.Contains(character))
            {
                item.sprite = newSprite;
                break;
            }
        }
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
        // TUTORIAL STUFF
        if (action == "WASD")
        {
            storyUI.SetActive(false);
            objectives.text = "Move the camera with WASD.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S));
                yield return new WaitForSeconds(6.0f);
                storyUI.SetActive(true);
            }
        }
        else if (action == "QE")
        {
            storyUI.SetActive(false);
            objectives.text = "Rotate the camera with Q or E.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E));
                yield return new WaitForSeconds(6.0f);
                storyUI.SetActive(true);
            }
        }
        else if (action == "RF")
        {
            storyUI.SetActive(false);
            objectives.text = "Zoom in using the Mousewheel, R, or F.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.F) || Input.mouseScrollDelta.y != 0);
                yield return new WaitForSeconds(6.0f);
                storyUI.SetActive(true);
            }
        }
        else if (action == "Patrol")
        {
            storyUI.SetActive(false);
            objectives.text = "Patrol to any location.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetSecurity() > 50);
                storyUI.SetActive(true);
            }
        }
        else if (action == "CheckReefHealth")
        {
            storyUI.SetActive(false);
            objectives.text = "Check the reef health.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => !mainUI.GetPlayerState().CheckHealthNeeded());
                storyUI.SetActive(true);
            }
        }
        else if (action == "InspectTourist1")
        {
            storyUI.SetActive(false);
            mainUI.GetSpawner().RandomSpawn("Tourist Boat");
            mainUI.GetPlayerState().AddTourists(1);
            mainUI.UpdateUIElements();
            objectives.text = "Look for the tourist.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitForSeconds(15.0f);
                storyUI.SetActive(true);
            }
        }
        else if (action == "InspectTourist2")
        {
            storyUI.SetActive(false);
            objectives.text = "Do a random inspection on the tourist.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetTouristScore() > 0);
                storyUI.SetActive(true);
            }
        }
        else if (action == "MoveBack")
        {
            storyUI.SetActive(false);
            objectives.text = "Return to the ranger station (next to the boat).";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerLocation().coordinates.ToString() == "(6, 10)");
                storyUI.SetActive(true);
            }
        }
        else if (action == "Research")
        {
            storyUI.SetActive(false);
            objectives.text = "Research the RADAR.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().CheckResearched("RADAR"));
                storyUI.SetActive(true);
            }
        }
        else if (action == "Build")
        {
            storyUI.SetActive(false);
            objectives.text = "Build a RADAR.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().CheckBuilt("RADAR"));
                storyUI.SetActive(true);
            }
        }
        else if (action == "UseRADAR")
        {
            storyUI.SetActive(false);
            objectives.text = "Use the RADAR.";
            StartCoroutine(WaitForPlayer());
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetRadarState());
                storyUI.SetActive(true);
            }
        }
        else if (action == "CatchFisherman")
        {
            storyUI.SetActive(false);
            StartCoroutine(WaitForPlayer());
            mainUI.GetSpawner().RandomSpawn("Fishing Boat");
            mainUI.GetPlayerState().AddFisherman(1);
            objectives.text = "Catch the Fishing Boat.";
            IEnumerator WaitForPlayer()
            {
                yield return new WaitUntil(() => mainUI.GetPlayerState().GetCatchScore() > 0);
                storyUI.SetActive(true);
            }
        }
    }
}