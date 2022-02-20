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
        for (HexDirection i = HexDirection.NE; i <= HexDirection.NW; i++)
        {
            HexCell currentA = cell.GetNeighbor(i);
            if (currentA != null)
            {
                if (currentA.Type == "Land")
                {
                    return 1;
                }
                for (HexDirection j = HexDirection.NE; j <= HexDirection.NW; j++)
                {
                    HexCell currentB = currentA.GetNeighbor(j);
                    if (currentB.Type == "Land")
                    {
                        return 2;
                    }
                }
            }
        }
        return 0;
    }

    public static int IsAdjacentToBuoy(HexCell cell)
    {
        if (cell.Type != "Land")
        {
            for (HexDirection i = HexDirection.NE; i <= HexDirection.NW; i++)
            {
                HexCell currentA = cell.GetNeighbor(i);
                if (currentA != null && currentA.Structure != null)
                {
                    if (currentA.Structure.StructureType == "Buoy")
                    {
                        return 1;
                    }
                    for (HexDirection j = HexDirection.NE; j <= HexDirection.NW; j++)
                    {
                        HexCell currentB = currentA.GetNeighbor(j);
                        if (currentB != null && currentB.Structure != null)
                        {
                            if (currentB.Structure.StructureType == "Buoy")
                            {
                                return 2;
                            }
                        }
                    }
                }
            }
        }
        return 0;
    }

    public static int GetIsland(HexCell cell)
    {
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
                    return 2;
                }
                else
                {
                    return 3;
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
