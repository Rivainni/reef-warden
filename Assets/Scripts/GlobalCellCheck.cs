using UnityEngine;
using System;
using System.Collections.Generic;
public static class GlobalCellCheck
{
    static TextAsset unsafeCells;
    static TextAsset escapeCells;
    static TextAsset adjacentChecks;
    static List<int> unsafeCellRanges = new List<int>();
    static List<int> escapeCellIndices = new List<int>();
    static List<int> adjacentToShore = new List<int>();
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

    public static int IsAdjacentToShore(HexCell cell)
    {
        if (cell.Type == "Land")
        {
            // int cmp = 0;
            for (int i = 0; i < adjacentToShore.Count; i += 2)
            {
                if (cell.Index >= adjacentToShore[i] && cell.Index <= adjacentToShore[i + 1])
                {
                    if (i == 0)
                    {
                        return 1;
                    }
                    else if (i == 2)
                    {
                        // cmp = Distance(cell.Index, adjacentToShore[i + 1]);
                        return 2;
                    }
                    else
                    {
                        // int currA = Distance(cell.Index, adjacentToShore[i]);
                        // int currB = Distance(cell.Index, adjacentToShore[i + 1]);
                        // if (cmp <= currA && cmp <= currB)
                        // {
                        //     return 2;
                        // }
                        // else
                        // {
                        //     return 3;
                        // }
                        return 3;
                    }
                }
            }
        }
        return 0;
    }

    static int Distance(int a, int b)
    {
        return Mathf.Abs(a - b);
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

    public static void SetAdjacentChecks(TextAsset textFile)
    {
        adjacentChecks = textFile;
        string txt = adjacentChecks.text;
        string[] lines = txt.Split(System.Environment.NewLine.ToCharArray());

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                string[] ranges = line.Split(' ');
                int rangeA = Int32.Parse(ranges[0]);
                int rangeB = Int32.Parse(ranges[1]);
                adjacentToShore.Add(rangeA);
                adjacentToShore.Add(rangeB);
            }
        }
    }
}
