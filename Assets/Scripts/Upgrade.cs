using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : HexStructure
{
    public string UpgradeType
    {
        get
        {
            return upgradeType;
        }
        set
        {
            upgradeType = value;
        }
    }
    string upgradeType;

    int buildTime;
    int researchCost;
    int buildCost;

    bool built = false;

    public void SetBuildTime(int turns)
    {
        buildTime = turns;
    }
    public void SetResearchCost(int cost)
    {
        researchCost = cost;
    }
    public void SetBuildCost(int cost)
    {
        buildCost = cost;
    }

    public void SetBuilt()
    {
        built = true;
    }

    public int GetBuildTime()
    {
        return buildTime;
    }

    public int GetResearchCost()
    {
        return researchCost;
    }

    public int GetBuildCost()
    {
        return buildCost;
    }

    public bool CheckBuilt()
    {
        return built;
    }
}
