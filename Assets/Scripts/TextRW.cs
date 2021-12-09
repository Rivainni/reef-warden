using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextRW
{
    public static TextAsset upgradesFile;

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
}