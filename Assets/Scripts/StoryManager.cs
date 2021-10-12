using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [SerializeField] GameObject mainUI;
    [SerializeField] Text storyText;
    [SerializeField] Text characterName;
    [SerializeField] GameObject characterSprites;
    [SerializeField] Image secondarySprite;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] GameObject inputPrefab;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] bool storyTime = true;
    [SerializeField] bool cutscene = true;
    [SerializeField] PlayerState initState;
    Queue<string> inputStream = new Queue<string>();

    // Start is called before the first frame update
    void Start()
    {
        if (!cutscene)
        {
            mainUI.SetActive(false);
        }
        secondarySprite.enabled = false;
    }

    public void StartDialogue(Queue<string> dialogue)
    {
        mainUI.SetActive(true); // open the dialogue box
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
                GameObject panel = Instantiate(panelPrefab, spawnAt, Quaternion.identity, mainUI.transform);
                GameObject input = Instantiate(inputPrefab, panel.transform.position, Quaternion.identity, panel.transform);
                GameObject confirm = Instantiate(buttonPrefab, panel.transform.position, Quaternion.identity, panel.transform);

                Button confirmButton = confirm.GetComponent<Button>();

                confirmButton.onClick.AddListener(() => SetName(input.GetComponent<InputField>(), panel));
            }
            else if (action.Contains("EnterCharacter"))
            {
                string newCharacter = action.Substring(14);
                Debug.Log(newCharacter);
                AddSprite(newCharacter);
            }
            else if (action == "ExitCharacter")
            {
                secondarySprite.enabled = false;
            }
            PrintDialogue();
        }
        else if (inputStream.Peek().Contains("{0}"))
        {
            string current = inputStream.Dequeue();
            int newSprite = int.Parse(current[0].ToString());
            Debug.Log(newSprite);
            storyText.text = string.Format(current.Substring(2), initState.GetName());
            SwitchSprites(characterName.text, newSprite);
        }
        else if (inputStream.Peek().Contains(":"))
        {
            string current = inputStream.Dequeue();
            int newSprite = int.Parse(current[0].ToString());
            Debug.Log(newSprite);
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
        mainUI.SetActive(false);
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
