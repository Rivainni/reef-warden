using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MinigameData")]
public class MinigameData : ScriptableObject
{
    [SerializeField] List<string> shipName;
    [SerializeField] List<string> shipID;
    [SerializeField] List<string> crew;

    public string GenerateSet(int factor, int randomisation)
    {
        string toReturn = "";

        toReturn += "Name: " + shipName[factor];
        toReturn += "\nID: " + shipID[factor + randomisation];
        toReturn += "\nCrew: " + crew[factor + randomisation];

        return toReturn;
    }
}