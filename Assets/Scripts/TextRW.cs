using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextRW
{
    public static TextAsset upgradesFile;
    public static TextAsset objectivesFile;

    struct Upgrade
    {
        public string Name { get; set; }
        public int Turns { get; set; }
        public int BuildCost { get; set; }
        public int ResearchCost { get; set; }
        public int ManpowerCost { get; set; }
        public int Upkeep { get; set; }
    }

    static List<Upgrade> upgrades = new List<Upgrade>();
    static List<string> level1Objectives = new List<string>();
    static List<string> level2Objectives = new List<string>();
    static List<string> level3Objectives = new List<string>();
    static List<string> level4Objectives = new List<string>();
    static List<string> level5Objectives = new List<string>();

    public static List<string> GetObjectives(int level)
    {
        if (level == 1)
        {
            return level1Objectives;
        }
        else if (level == 2)
        {
            return level2Objectives;
        }
        else if (level == 3)
        {
            return level3Objectives;
        }
        else if (level == 4)
        {
            return level4Objectives;
        }
        else if (level == 5)
        {
            return level5Objectives;
        }
        else
        {
            return level1Objectives;
        }
    }

    public static void SetUpgrades()
    {
        string txt = upgradesFile.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        Upgrade curr = new Upgrade();
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("!"))
                {
                    curr = new Upgrade();
                    curr.Name = line.Substring(0, line.Length - 1);
                }
                else if (line.StartsWith("Build Cost:"))
                {
                    curr.BuildCost = Int32.Parse(line.Substring(11, line.Length));
                }
            }
        }
    }

    public static void SetObjectives()
    {
        string txt = objectivesFile.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        string currentLevel = "";
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))// ignore empty lines of dialogue
            {
                if (line.StartsWith("["))
                {
                    currentLevel = line.Substring(0, line.IndexOf(']') + 1);
                }
                else
                {
                    if (currentLevel == "Level1")
                    {
                        level1Objectives.Add(line);
                    }
                    else if (currentLevel == "Level2")
                    {
                        level1Objectives.Add(line);
                    }
                    else if (currentLevel == "Level3")
                    {
                        level1Objectives.Add(line);
                    }
                    else if (currentLevel == "Level4")
                    {
                        level1Objectives.Add(line);
                    }
                    else if (currentLevel == "Level5")
                    {
                        level1Objectives.Add(line);
                    }
                }
            }
        }
    }
}