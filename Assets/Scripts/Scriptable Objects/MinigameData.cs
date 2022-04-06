using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MinigameData")]
public class MinigameData : ScriptableObject
{
    [SerializeField] TextAsset inspection;
    [SerializeField] List<string> shipName;
    [SerializeField] List<string> shipID;
    [SerializeField] List<string> crew;

    public string GenerateSet(int factor, int randomisation)
    {
        string toReturn = "";

        if (factor + randomisation > 30)
        {
            toReturn += "Name: " + shipName[factor];
            toReturn += "\nID: " + shipID[factor - randomisation];
            toReturn += "\nCrew: " + crew[factor - randomisation];
        }
        else
        {
            toReturn += "Name: " + shipName[factor];
            toReturn += "\nID: " + shipID[factor + randomisation];
            toReturn += "\nCrew: " + crew[factor + randomisation];
        }

        return toReturn;
    }

    public void SetInspection()
    {
        string txt = inspection.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());
        string currentList = "";

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("["))
                {
                    if (line.Contains("Name"))
                    {
                        currentList = "shipName";
                    }
                    else if (line.Contains("ID"))
                    {
                        currentList = "shipID";
                    }
                    else if (line.Contains("Crew"))
                    {
                        currentList = "crew";
                    }
                }
                else
                {
                    if (currentList == "shipName")
                    {
                        shipName.Add(line);
                    }
                    else if (currentList == "shipID")
                    {
                        shipID.Add(line);
                    }
                    else if (currentList == "crew")
                    {
                        crew.Add(line);
                    }
                }
            }
        }
    }
}