using UnityEngine;
using System;
public static class GlobalCellCheck
{
    static TextAsset unsafeCells;
    public static bool IsNotReachable(int cellIndex)
    {
        string txt = unsafeCells.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                string[] ranges = line.Split(' ');
                int rangeA = Int32.Parse(ranges[0]);
                int rangeB = Int32.Parse(ranges[1]);

                if (cellIndex >= rangeA && cellIndex <= rangeB)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsImpassable(HexCell cell)
    {
        if (cell.Type == "Land")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void SetUnsafeCells(TextAsset textFile)
    {
        unsafeCells = textFile;
    }
}
