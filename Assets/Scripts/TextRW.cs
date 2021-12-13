using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextRW
{
    public static TextAsset upgradesFile;
    public static TextAsset objectivesFile;

    public struct UpgradeItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Turns { get; set; }
        public int BuildCost { get; set; }
        public int ResearchCost { get; set; }
        public int ManpowerCost { get; set; }
        public int Upkeep { get; set; }
    }

    static List<UpgradeItem> upgrades = new List<UpgradeItem>();
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

    public static List<UpgradeItem> GetUpgrades()
    {
        return upgrades;
    }

    public static UpgradeItem GetUpgrade(string name)
    {
        foreach (UpgradeItem upgrade in upgrades)
        {
            if (upgrade.Name == name)
            {
                return upgrade;
            }
        }
        return upgrades[0];
    }

    public static void SetUpgrades(TextAsset text)
    {
        upgradesFile = text;
        string txt = upgradesFile.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        UpgradeItem curr = new UpgradeItem();
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("!"))
                {
                    curr = new UpgradeItem();
                    curr.Name = line.Substring(1, line.Length - 2);
                }
                else if (line.StartsWith("Build Cost:"))
                {
                    curr.BuildCost = Int32.Parse(line.Substring(12));
                }
                else if (line.StartsWith("Research Cost:"))
                {
                    curr.ResearchCost = Int32.Parse(line.Substring(15));
                }
                else if (line.StartsWith("Time:"))
                {
                    curr.Turns = Int32.Parse(line.Substring(6));
                }
                else if (line.StartsWith("Manpower:"))
                {
                    curr.ManpowerCost = Int32.Parse(line.Substring(10));
                }
                else if (line.StartsWith("Upkeep:"))
                {
                    curr.Upkeep = Int32.Parse(line.Substring(8));
                }
                else if (line.StartsWith("END"))
                {
                    upgrades.Add(curr);
                }
                else
                {
                    curr.Description += line + "\n";
                }
            }
        }
    }

    public static void SetObjectives(TextAsset text)
    {
        objectivesFile = text;
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