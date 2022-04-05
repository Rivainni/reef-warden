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
    [SerializeField] int level;
    [SerializeField] int checkHealthCD1;
    [SerializeField] int checkHealthCD2;
    [SerializeField] int checkHealthCD3;
    [SerializeField] int birdCD;
    [SerializeField] int clamCD1;
    [SerializeField] int clamCD2;
    [SerializeField] int clamCD3;
    [SerializeField] int turtleCD;
    [SerializeField] int researchCD;
    [SerializeField] int radarCD;
    [SerializeField] int basketballCD;
    [SerializeField] int recRoomCD;

    [SerializeField] int income;
    [SerializeField] string[] possibleActions;
    [SerializeField] List<string> unlockedUpgrades;
    [SerializeField] List<string> builtUpgrades;
    [SerializeField] Queue<UpgradeItem> upgradeQueue = new Queue<UpgradeItem>();
    [SerializeField] Queue<UpgradeItem> researchQueue = new Queue<UpgradeItem>();
    [SerializeField] int touristsInspected;
    [SerializeField] int fishermenCaught;
    string message;
    List<string> currentObjectives = new List<string>();
    const float moraleLambda = 0.01f;
    const float securityLambda = 0.04f;
    bool radarActive = false;
    bool inTutorial = false;
    bool AIS = false;
    bool SAT = false;
    bool SS = false;
    bool MA = false;
    bool daySpawn = false;
    bool nightSpawn = false;
    int sinceDaySpawn = 0;
    int sinceNightSpawn = 0;
    int sinceLastHealthCheck = 0;

    // counters for objectives
    int levelTurns = 0;
    int healthCount = 0;
    int patrols = 0;
    int birdCount = 0;
    int securityTurns = 0;
    int monitorClams = 0;
    int moraleTurns = 0;
    int tagCount = 0;

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

    public float GetTrueHealth()
    {
        return trueReefHealth;
    }

    public bool ReefDamaged()
    {
        return seenReefHealth > trueReefHealth;
    }

    public int GetTurn()
    {
        return turn;
    }

    public int GetLevel()
    {
        return level;
    }

    public void AddLevel()
    {
        level++;
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
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

    // when earning money, take morale into account. since the idea is that there might be additional pay for performance.
    public void AdjustMoney(int factor)
    {
        if (factor < 0)
        {
            money += factor;
        }
        else
        {
            money += Mathf.RoundToInt(factor + factor * morale);
        }
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

    public void SetTourists(int toSet)
    {
        tourists = toSet;
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
        else if (type.Equals("C1"))
        {
            return clamCD1;
        }
        else if (type.Equals("C2"))
        {
            return clamCD2;
        }
        else if (type.Equals("C3"))
        {
            return clamCD3;
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
        clamCD1 = (clamCD1 > 0) ? clamCD1 -= 1 : clamCD1;
        clamCD2 = (clamCD2 > 0) ? clamCD2 -= 1 : clamCD2;
        clamCD3 = (clamCD3 > 0) ? clamCD3 -= 1 : clamCD3;
        turtleCD = (turtleCD > 0) ? turtleCD -= 1 : turtleCD;
        researchCD = (researchCD > 0) ? researchCD -= 1 : researchCD;
        radarCD = (radarCD > 0) ? radarCD -= 1 : radarCD;
        basketballCD = (basketballCD > 0) ? basketballCD -= 1 : basketballCD;
        recRoomCD = (recRoomCD > 0) ? recRoomCD -= 1 : recRoomCD;
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
            birdCD = 3;
        }
        else if (type.Equals("C1"))
        {
            clamCD1 = 4;
        }
        else if (type.Equals("C2"))
        {
            clamCD2 = 4;
        }
        else if (type.Equals("C3"))
        {
            clamCD3 = 4;
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
        else if (type.Equals("BB"))
        {
            basketballCD = 5;
        }
        else if (type.Equals("RR"))
        {
            recRoomCD = 5;
        }
    }

    public void QueueUpgrade(string upgradeType, int constructionTime)
    {
        upgradeQueue.Enqueue(new UpgradeItem(upgradeType, constructionTime));
    }

    void UpdateUpgradeQueue()
    {
        UpgradeItem current = upgradeQueue.Dequeue();

        current.Turns -= 1;
        if (current.Turns > 0)
        {
            upgradeQueue.Enqueue(current);
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
        UpgradeItem current = researchQueue.Dequeue();

        current.Turns -= 1;
        if (current.Turns > 0)
        {
            researchQueue.Enqueue(current);
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

    public void AddAIS()
    {
        AIS = true;
    }

    public void RemoveAIS()
    {
        AIS = false;
    }

    public bool CheckAIS()
    {
        return AIS;
    }

    public void AddSAT()
    {
        SAT = true;
    }

    public void RemoveSAT()
    {
        SAT = false;
    }

    public bool CheckSAT()
    {
        return SAT;
    }

    public void AddSS()
    {
        SS = true;
    }

    public void RemoveSS()
    {
        SS = false;
    }

    public bool CheckSS()
    {
        return SS;
    }

    public void AddMA()
    {
        MA = true;
    }

    public void RemoveMA()
    {
        MA = false;
    }

    public bool CheckMA()
    {
        return MA;
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

    public void ResetFisherman()
    {
        fishermen = 0;
    }

    public int GetFishermen()
    {
        return fishermen;
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
        if (username == "")
        {
            username = "Juan";
        }

        money = 10000;
        income = 0;
        research = 250;
        manpower = 6;
        tourists = 0;
        fishermen = 0;
        morale = 50;
        security = 50;
        trueReefHealth = 100;
        seenReefHealth = 50;
        turn = 1;

        if (inTutorial)
        {
            level = 0;
        }
        else
        {
            level = 1;
        }

        checkHealthCD1 = 0;
        checkHealthCD2 = 0;
        checkHealthCD3 = 0;
        birdCD = 0;
        clamCD1 = 0;
        clamCD2 = 0;
        clamCD3 = 0;
        turtleCD = 0;
        researchCD = 0;
        unlockedUpgrades = new List<string> { "Basketball Court", "Radio", "Service Boat" };
        builtUpgrades = new List<string> { "Radio" };
        touristsInspected = 0;
        fishermenCaught = 0;
        radarActive = false;
        message = "";
        currentObjectives = new List<string>();
        daySpawn = false;
        nightSpawn = false;
        radarActive = false;
        AIS = false;
        SAT = false;
        SS = false;
        levelTurns = 0;
        healthCount = 0;
        patrols = 0;
        birdCount = 0;
        securityTurns = 0;
        monitorClams = 0;
        moraleTurns = 0;
        tagCount = 0;
    }

    public void EndTurn()
    {
        turn++;
        incrementLevelCounters("level");
        money += income;
        sinceLastHealthCheck++;
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
        if (security >= 55 && level == 3)
        {
            incrementLevelCounters("security");
        }
        if (morale >= 45 && level == 4)
        {
            incrementLevelCounters("morale");
        }

        ReduceCD();
        UpdateObjectives();
        if (currentObjectives.Count == 0 && !CheckTutorial())
        {
            AddLevel();
            SetObjectives(TextRW.GetObjectives(level));
        }
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

    public List<string> GetObjectives()
    {
        return currentObjectives;
    }

    public void SetObjectives(List<string> replace)
    {
        currentObjectives = replace;
    }
    public void SetObjectives(string replace)
    {
        currentObjectives.Clear();
        currentObjectives.Add(replace);
    }

    public void RemoveObjective(string objective)
    {
        currentObjectives.Remove(objective);
    }

    public void incrementLevelCounters(string name)
    {
        if (name == "level")
        {
            levelTurns++;
        }
        else if (name == "health")
        {
            healthCount++;
        }
        else if (name == "bird")
        {
            birdCount++;
        }
        else if (name == "patrol")
        {
            patrols++;
        }
        else if (name == "security")
        {
            security++;
        }
        else if (name == "monitor")
        {
            monitorClams++;
        }
        else if (name == "morale")
        {
            moraleTurns++;
        }
        else if (name == "tag")
        {
            tagCount++;
        }
    }

    public void resetLevelCounters(string name)
    {
        if (name == "level")
        {
            levelTurns = 0;
        }
        else if (name == "health")
        {
            healthCount = 0;
        }
        else if (name == "patrol")
        {
            patrols = 0;
        }
        else if (name == "security")
        {
            security = 0;
        }
        else if (name == "monitor")
        {
            monitorClams = 0;
        }
        else if (name == "morale")
        {
            moraleTurns = 0;
        }
        else if (name == "tag")
        {
            tagCount = 0;
        }
    }

    public void UpdateObjectives()
    {
        List<string> toRemove = new List<string>();
        foreach (string item in currentObjectives)
        {
            // honestly, we could put all these under one big if statement pero for the sake of readability I won't.
            switch (level)
            {
                case 1:
                    if (CheckBuilt("RADAR") && item.Contains("RADAR"))
                    {
                        toRemove.Add(item);
                    }
                    else if (healthCount >= 3 && item.Contains("health"))
                    {
                        toRemove.Add(item);
                    }
                    else if (patrols >= 2 && item.Contains("patrols"))
                    {
                        toRemove.Add(item);
                    }
                    else if (levelTurns >= 6 && item.Contains("since Level"))
                    {
                        toRemove.Add(item);
                        levelTurns = 0;
                    }
                    break;
                case 2:
                    if (CheckBuilt("Souvenir Stand") && item.Contains("Souvenir"))
                    {
                        toRemove.Add(item);
                    }
                    else if (birdCount >= 2 && item.Contains("bird"))
                    {
                        toRemove.Add(item);
                    }
                    else if (levelTurns >= 6 && item.Contains("since Level"))
                    {
                        toRemove.Add(item);
                        levelTurns = 0;
                    }
                    break;
                case 3:
                    if (CheckResearched("Double-engine Patrol Boat") && item.Contains("patrol boat"))
                    {
                        toRemove.Add(item);
                    }
                    else if (securityTurns >= 4 && item.Contains("security"))
                    {
                        toRemove.Add(item);
                    }
                    else if (monitorClams >= 1 && item.Contains("clams"))
                    {
                        toRemove.Add(item);
                    }
                    else if (levelTurns >= 6 && item.Contains("since Level"))
                    {
                        toRemove.Add(item);
                        levelTurns = 0;
                    }
                    break;
                case 4:
                    if (moraleTurns >= 2 && item.Contains("morale"))
                    {
                        toRemove.Add(item);
                    }
                    else if ((CheckBuilt("Rec Room") || CheckBuilt("Basketball Court")) && item.Contains("personnel"))
                    {
                        toRemove.Add(item);
                    }
                    else if (tagCount >= 2 && item.Contains("turtles"))
                    {
                        toRemove.Add(item);
                    }
                    else if (levelTurns >= 6 && item.Contains("since Level"))
                    {
                        toRemove.Add(item);
                        levelTurns = 0;
                    }
                    break;
                case 5:
                    if (CheckResearched("Total Protection") && item.Contains("Research"))
                    {
                        toRemove.Add(item);
                    }
                    break;
            }
        }

        foreach (string item in toRemove)
        {
            currentObjectives.Remove(item);
        }
    }

    public bool SpawnedDay()
    {
        return daySpawn;
    }

    public bool SpawnedNight()
    {
        return nightSpawn;
    }

    public int daySpawnCounter()
    {
        return sinceDaySpawn;
    }

    public int nightSpawnCounter()
    {
        return sinceNightSpawn;
    }

    public int GetLastHealthCheck()
    {
        return sinceLastHealthCheck;
    }

    public void ToggleDaySpawn()
    {
        if (daySpawn)
        {
            daySpawn = false;
        }
        else
        {
            daySpawn = true;
        }
    }

    public void ToggleNightSpawn()
    {
        if (nightSpawn)
        {
            nightSpawn = false;
        }
        else
        {
            nightSpawn = true;
        }
    }

    public void ResetDaySpawn()
    {
        sinceDaySpawn = 0;
    }
    public void ResetNightSpawn()
    {
        sinceNightSpawn = 0;
    }

    public void AddDaySpawn()
    {
        sinceDaySpawn++;
    }

    public void AddNightSpawn()
    {
        sinceNightSpawn++;
    }

    public void ResetHealthWarning()
    {
        sinceLastHealthCheck = 0;
    }
}