using UnityEngine;
using System;
using System.Collections.Generic;
public static class GlobalCellCheck
{
    static TextAsset unsafeCells;
    static TextAsset escapeCells;
    static List<int> unsafeCellRanges = new List<int>();
    static List<int> escapeCellIndices = new List<int>();
    public static bool IsNotReachable(int cellIndex)
    {
        for (int i = 0; i < unsafeCellRanges.Count; i += 2)
        {
            if (cellIndex >= unsafeCellRanges[i] && cellIndex <= unsafeCellRanges[i + 1])
            {
                return true;
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

    public static int GetEscapeCell(int index)
    {
        return escapeCellIndices[index];
    }

    public static int GetEscapeCellCount()
    {
        return escapeCellIndices.Count;
    }

    public static void SetUnsafeCells(TextAsset textFile)
    {
        unsafeCells = textFile;
        string txt = unsafeCells.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                string[] ranges = line.Split(' ');
                int rangeA = Int32.Parse(ranges[0]);
                int rangeB = Int32.Parse(ranges[1]);
                unsafeCellRanges.Add(rangeA);
                unsafeCellRanges.Add(rangeB);
            }
        }
    }

    public static void SetEscapeCells(TextAsset textFile)
    {
        escapeCells = textFile;
        string txt = escapeCells.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                int index = Int32.Parse(line);
                escapeCellIndices.Add(index);
            }
        }
    }
}
