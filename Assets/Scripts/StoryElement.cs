using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryElement : MonoBehaviour
{
    [SerializeField] TextAsset TextFileAsset; // your imported text file for your NPC
    [SerializeField] string name;
    [SerializeField] MainUI mainUI; // need to access the player state to determine whether to start dialogue or nah.
    Queue<string> dialogue = new Queue<string>(); // stores the dialogue (Great Performance!)

    void Start()
    {
        if (name == "Tutorial" || SceneManager.GetActiveScene().name == "Cutscene")
        {
            TriggerDialogue();
        }
    }

    /* Called when you want to start dialogue */
    public void TriggerDialogue()
    {
        ReadTextFile(); // loads in the text file
        FindObjectOfType<StoryManager>().StartDialogue(dialogue); // Accesses Dialogue Manager and Starts Dialogue
    }

    /* loads in your text file */
    void ReadTextFile()
    {
        string txt = TextFileAsset.text;

        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray()); // Split dialogue lines by newline

        foreach (string line in lines) // for every line of dialogue
        {
            if (!string.IsNullOrEmpty(line))// ignore empty lines of dialogue
            {
                if (line.StartsWith("[")) // e.g [NAME=Michael] Hello, my name is Michael
                {
                    string special = line.Substring(0, line.IndexOf(']') + 1); // special = [NAME=Michael]
                    string curr = line.Substring(line.IndexOf(']') + 1); // curr = Hello, ...
                    dialogue.Enqueue(special); // adds to the dialogue to be printed
                    dialogue.Enqueue(curr);
                }
                else
                {
                    dialogue.Enqueue(line); // adds to the dialogue to be printed
                }
            }
        }
        dialogue.Enqueue("EndQueue");
    }
}
