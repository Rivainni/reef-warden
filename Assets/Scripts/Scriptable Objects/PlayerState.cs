using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState")]
public class PlayerState : ScriptableObject
{
    [SerializeField] string username;
    [SerializeField] int money;
    [SerializeField] int research;
    [SerializeField] int manpower;
    [SerializeField] int tourists;
    [SerializeField] int fishermen;
    [SerializeField] float morale;
    [SerializeField] float security;
    [SerializeField] float trueReefHealth;
    float seenReefHealth;

    [SerializeField] int turn;
    [SerializeField] int checkHealthCD1;
    [SerializeField] int checkHealthCD2;
    [SerializeField] int checkHealthCD3;
    [SerializeField] int birdCD;
    [SerializeField] int clamCD;
    [SerializeField] int turtleCD;
    [SerializeField] int researchCD;
    [SerializeField] int radarCD;

    [SerializeField] int income;
    [SerializeField] string[] possibleActions;
    [SerializeField] List<string> unlockedUpgrades;
    [SerializeField] List<string> builtUpgrades;
    [SerializeField] Queue<UpgradeItem> upgradeQueue = new Queue<UpgradeItem>();
    [SerializeField] Queue<UpgradeItem> researchQueue = new Queue<UpgradeItem>();
    [SerializeField] int touristsInspected;
    [SerializeField] int fishermenCaught;
    string message;
    const float moraleLambda = 0.04f;
    const float securityLambda = 0.04f;
    bool radarActive = false;
    bool inTutorial;

    struct UpgradeItem
    {
        public UpgradeItem(string name, int turns)
        {
            Name = name;
            Turns = turns;
        }

        public string Name { get; set; }
        public int Turns { get; set; }
    }

    public string GetName()
    {
        return username;
    }

    public void SetName(string newName)
    {
        username = newName;
    }

    public int GetMoney()
    {
        return money;
    }

    public int GetResearch()
    {
        return research;
    }

    public int GetManpower()
    {
        return manpower;
    }

    public int GetTourists()
    {
        return tourists;
    }

    public float GetMorale()
    {
        return morale;
    }

    public float GetSecurity()
    {
        return security;
    }

    public float GetHealth()
    {
        return seenReefHealth;
    }

    public int GetTurn()
    {
        return turn;
    }

    public string[] GetPossibleActions()
    {
        return possibleActions;
    }

    public string GetMessage()
    {
        return message;
    }

    public void SetMessage(string message)
    {
        this.message = message;
    }

    public int GetIncome()
    {
        return income;
    }

    public void SetIncome(int replacement)
    {
        income = replacement;
    }

    public void AdjustIncome(int factor)
    {
        income += factor;
    }

    public void AdjustMoney(int factor)
    {
        money += factor;
    }

    public void AddResearch(int RP)
    {
        research += RP;
    }

    public void AddManpower(int RP)
    {
        manpower += RP;
    }


    public void AddTourists(int toAdd)
    {
        tourists += toAdd;
    }


    public void AddMorale(float toAdd)
    {
        morale += toAdd;
    }

    public void AddSecurity(float toAdd)
    {
        security += toAdd;
    }

    public void DecreaseHealth(float toDecrease)
    {
        trueReefHealth -= toDecrease;
    }

    public void UpdateHealth()
    {
        seenReefHealth = trueReefHealth;
    }

    public bool CheckHealthNeeded()
    {
        return !(seenReefHealth == trueReefHealth);
    }


    public int FetchCD(string type)
    {
        if (type.Equals("CH1"))
        {
            return checkHealthCD1;
        }
        else if (type.Equals("CH2"))
        {
            return checkHealthCD2;
        }
        else if (type.Equals("CH3"))
        {
            return checkHealthCD3;
        }
        else if (type.Equals("B"))
        {
            return birdCD;
        }
        else if (type.Equals("C"))
        {
            return clamCD;
        }
        else if (type.Equals("T"))
        {
            return turtleCD;
        }
        else if (type.Equals("R"))
        {
            return researchCD;
        }
        else if (type.Equals("RADAR"))
        {
            return radarCD;
        }
        else
        {
            return 0;
        }
    }

    public void ReduceCD()
    {
        checkHealthCD1 = (checkHealthCD1 > 0) ? checkHealthCD1 -= 1 : checkHealthCD1;
        checkHealthCD2 = (checkHealthCD2 > 0) ? checkHealthCD2 -= 1 : checkHealthCD2;
        checkHealthCD3 = (checkHealthCD3 > 0) ? checkHealthCD3 -= 1 : checkHealthCD3;
        birdCD = (birdCD > 0) ? birdCD -= 1 : birdCD;
        clamCD = (clamCD > 0) ? clamCD -= 1 : clamCD;
        turtleCD = (turtleCD > 0) ? turtleCD -= 1 : turtleCD;
        researchCD = (researchCD > 0) ? researchCD -= 1 : researchCD;
        radarCD = (radarCD > 0) ? radarCD -= 1 : radarCD;
    }

    public void ResetCD(string type)
    {
        if (type.Equals("CH1"))
        {
            checkHealthCD1 = 2;
        }
        else if (type.Equals("CH2"))
        {
            checkHealthCD2 = 2;
        }
        else if (type.Equals("CH3"))
        {
            checkHealthCD3 = 2;
        }
        else if (type.Equals("B"))
        {
            birdCD = 5;
        }
        else if (type.Equals("C"))
        {
            clamCD = 10;
        }
        else if (type.Equals("T"))
        {
            turtleCD = 10;
        }
        else if (type.Equals("R"))
        {
            researchCD = 10;
        }
        else if (type.Equals("RADAR"))
        {
            radarCD = 5;
        }
    }

    public void QueueUpgrade(string upgradeType, int constructionTime)
    {
        upgradeQueue.Enqueue(new UpgradeItem(upgradeType, constructionTime));
    }

    void UpdateUpgradeQueue()
    {
        UpgradeItem current = upgradeQueue.Peek();

        current.Turns -= 1;
        if (current.Turns <= 0)
        {
            upgradeQueue.Dequeue();
        }
    }

    public int CheckUpgrade(string upgradeType)
    {
        if (upgradeQueue.Count > 0)
        {
            foreach (UpgradeItem item in upgradeQueue)
            {
                if (item.Name.Contains(upgradeType))
                {
                    return item.Turns;
                }
            }
        }

        return 0;
    }

    public bool CheckBuilt(string upgradeType)
    {
        foreach (string item in builtUpgrades)
        {
            if (item == upgradeType)
            {
                return true;
            }
        }

        return false;
    }
    public void AddUpgrade(string upgradeType)
    {
        builtUpgrades.Add(upgradeType);
    }

    public void QueueResearch(string name, int researchTime)
    {
        researchQueue.Enqueue(new UpgradeItem(name, researchTime));
    }

    void UpdateResearchQueue()
    {
        UpgradeItem current = researchQueue.Peek();

        current.Turns -= 1;
        if (current.Turns <= 0)
        {
            researchQueue.Dequeue();
        }
    }

    public int CheckResearchQueue(string name)
    {
        if (researchQueue.Count > 0)
        {
            foreach (UpgradeItem item in researchQueue)
            {
                if (item.Name.Contains(name))
                {
                    return item.Turns;
                }
            }
        }

        return 0;
    }

    public bool CheckResearched(string name)
    {
        foreach (string item in unlockedUpgrades)
        {
            if (item == name)
            {
                return true;
            }
        }

        return false;
    }

    public void UnlockUpgrade(string name)
    {
        unlockedUpgrades.Add(name);
    }

    public bool GetRadarState()
    {
        return radarActive;
    }

    public void ActivateRadar()
    {
        radarActive = true;
        ResetCD("RADAR");
    }

    public void DeactivateRadar()
    {
        radarActive = false;
    }

    public void AddTouristScore()
    {
        touristsInspected++;
    }

    public void AddCatchScore()
    {
        fishermenCaught++;
    }

    public void AddFisherman(int count)
    {
        fishermen += count;
    }

    public int GetTouristScore()
    {
        return touristsInspected;
    }

    public int GetCatchScore()
    {
        return fishermenCaught;
    }

    public void Clean()
    {
        money = 10000;
        research = 250;
        manpower = 6;
        tourists = 0;
        fishermen = 0;
        morale = 50;
        security = 50;
        trueReefHealth = 100;
        seenReefHealth = 50;
        turn = 1;
        checkHealthCD1 = 0;
        checkHealthCD2 = 0;
        checkHealthCD3 = 0;
        birdCD = 0;
        clamCD = 0;
        turtleCD = 0;
        researchCD = 0;
        unlockedUpgrades = new List<string> { "Basketball Court, Radio, Service Boat" };
        builtUpgrades.Clear();
        touristsInspected = 0;
        fishermenCaught = 0;
        radarActive = false;
        message = "";
    }

    public void EndTurn()
    {
        turn++;
        money += income;
        morale *= Mathf.Exp(-moraleLambda * 1);
        security *= Mathf.Exp(-securityLambda * 1);

        if (upgradeQueue.Count > 0)
        {
            UpdateUpgradeQueue();
        }
        if (researchQueue.Count > 0)
        {
            UpdateResearchQueue();
        }
        if (fishermen > 0)
        {
            DecreaseHealth(fishermen * 2);
        }
        ReduceCD();
    }

    public void StartTutorial()
    {
        inTutorial = true;
    }

    public void EndTutorial()
    {
        inTutorial = false;
    }

    public bool CheckTutorial()
    {
        return inTutorial;
    }
}