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

            // TUTORIAL STUFF
            else if (action == "Patrol")
            {
                storyUI.SetActive(false);
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
                IEnumerator WaitForPlayer()
                {
                    yield return new WaitUntil(() => mainUI.GetPlayerState().GetCatchScore() > 0);
                    storyUI.SetActive(true);
                }
            }
            PrintDialogue();
        }
        else if (inputStream.Peek().Contains("{0}"))
        {
            string current = inputStream.Dequeue();
            int newSprite = int.Parse(current[0].ToString());
            storyText.text = string.Format(current.Substring(2), initState.GetName());
            SwitchSprites(characterName.text, newSprite);
        }
        else if (inputStream.Peek().Contains(":"))
        {
            string current = inputStream.Dequeue();
            int newSprite = int.Parse(current[0].ToString());
            storyText.text = current.Substring(3);
            SwitchSprites(characterName.text, newSprite);
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
            SceneManager.LoadScene("Tutorial");
        }
    }

    void SetName(InputField input, GameObject toRemove)
    {
        initState.SetName(input.text);
        Destroy(toRemove);
    }

    void SwitchSprites(string character, int counter)
    {
        Image[] current = characterSprites.GetComponentsInChildren<Image>();
        string toLoad = character + "_" + counter;
        Sprite newSprite = Resources.Load<Sprite>("Characters/" + toLoad);

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
        Sprite newSprite = Resources.Load<Sprite>("Characters/" + toLoad);
        secondarySprite.sprite = newSprite;
    }
}