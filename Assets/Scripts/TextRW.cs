using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class TextRW
{
    public static TextAsset upgradesFile;
    public static TextAsset dutiesFile;
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

    public struct InfoItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    static List<UpgradeItem> upgrades = new List<UpgradeItem>();
    static List<InfoItem> duties = new List<InfoItem>();
    static List<string> level1Objectives = new List<string>();
    static List<string> level2Objectives = new List<string>();
    static List<string> level3Objectives = new List<string>();
    static List<string> level4Objectives = new List<string>();
    static List<string> level5Objectives = new List<string>();
    static int[] currentSettings = { 1920, 1080, 1, 100, 100 };

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

    public static List<InfoItem> GetDuties()
    {
        return duties;
    }

    public static InfoItem GetDuty(string name)
    {
        foreach (InfoItem duty in duties)
        {
            if (duty.Name == name)
            {
                return duty;
            }
        }
        return duties[0];
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
                    if (!CheckIfLoaded(curr.Name, "upgrades"))
                    {
                        upgrades.Add(curr);
                    }
                }
                else
                {
                    curr.Description += line + "\n";
                }
            }
        }
    }

    public static void SetDuties(TextAsset text)
    {
        dutiesFile = text;
        string txt = dutiesFile.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        InfoItem curr = new InfoItem();
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("!"))
                {
                    curr = new InfoItem();
                    curr.Name = line.Substring(1, line.Length - 2);
                }
                else if (line.StartsWith("END"))
                {
                    if (!CheckIfLoaded(curr.Name, "duties"))
                    {
                        duties.Add(curr);
                    }
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
        level1Objectives.Clear();
        level2Objectives.Clear();
        level3Objectives.Clear();
        level4Objectives.Clear();
        level5Objectives.Clear();

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
                    currentLevel = line.Substring(1, line.IndexOf(']') - 1);
                }
                else
                {
                    if (currentLevel == "Level1")
                    {
                        level1Objectives.Add(line);
                    }
                    else if (currentLevel == "Level2")
                    {
                        level2Objectives.Add(line);
                    }
                    else if (currentLevel == "Level3")
                    {
                        level3Objectives.Add(line);
                    }
                    else if (currentLevel == "Level4")
                    {
                        level4Objectives.Add(line);
                    }
                    else if (currentLevel == "Level5")
                    {
                        level5Objectives.Add(line);
                    }
                }
            }
        }
    }

    public static int[] GetSettings()
    {
        return currentSettings;
    }

    public static int GetUIScale()
    {
        return currentSettings[3];
    }

    public static void WriteSettings(int resolutionW, int resolutionH, int fullscreen, int scale, int volume)
    {
        currentSettings[0] = resolutionW;
        currentSettings[1] = resolutionH;
        currentSettings[2] = fullscreen;
        currentSettings[3] = scale;
        currentSettings[4] = volume;

        string path = Path.Combine(Application.dataPath, "settings.txt");
        using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Create)))
        {
            writer.WriteLine("[ResolutionW]");
            writer.WriteLine(currentSettings[0]);
            writer.WriteLine("[ResolutionH]");
            writer.WriteLine(currentSettings[1]);
            writer.WriteLine("[Fullscreen]");
            writer.WriteLine(currentSettings[2]);
            writer.WriteLine("[Scale]");
            writer.WriteLine(currentSettings[3]);
            writer.WriteLine("[Volume]");
            writer.WriteLine(currentSettings[4]);
        }
    }

    public static void ReadSettings()
    {
        string path = Path.Combine(Application.dataPath, "settings.txt");
        string txt = "";
        try
        {
            using (StreamReader reader = new StreamReader(File.OpenRead(path)))
            {
                txt = reader.ReadToEnd();
            }
        }
        catch (FileNotFoundException e)
        {
            TextRW.WriteSettings(Screen.currentResolution.width, Screen.currentResolution.height, 1, 100, 100);
            Debug.Log(e);
        }

        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());
        if (lines.Length > 1)
        {
            ReadSettings(lines);
        }
    }

    static void ReadSettings(string[] lines)
    {
        string currentSetting = "";
        foreach (string line in lines)
        {
            if (line.StartsWith("["))
            {
                currentSetting = line.Substring(1, line.IndexOf(']') - 1);
            }
            else
            {
                int parsed = 0;
                if (Int32.TryParse(line, out parsed))
                {
                    if (currentSetting == "ResolutionW")
                    {
                        currentSettings[0] = parsed;
                    }
                    else if (currentSetting == "ResolutionH")
                    {
                        currentSettings[1] = parsed;
                    }
                    else if (currentSetting == "Fullscreen")
                    {
                        currentSettings[2] = parsed;
                    }
                    else if (currentSetting == "Scale")
                    {
                        currentSettings[3] = parsed;
                        Debug.Log(parsed);
                    }
                    else if (currentSetting == "Volume")
                    {
                        currentSettings[4] = parsed;
                    }
                }
            }
        }
    }

    static bool CheckIfLoaded(string toCheck, string type)
    {
        if (type == "upgrades")
        {
            foreach (UpgradeItem upgrade in upgrades)
            {
                if (toCheck == upgrade.Name)
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (InfoItem duty in duties)
            {
                if (toCheck == duty.Name)
                {
                    return true;
                }
            }
        }

        return false;
    }
}