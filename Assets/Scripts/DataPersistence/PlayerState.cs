using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerState : IDataPersistence
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

    public List<SaveUnit> units;
    public List<SaveUpgrade> upgrades;
    public SaveTime time;

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
        return Mathf.Clamp(morale, 0, 100);
    }

    public float GetSecurity()
    {
        return Mathf.Clamp(security, 0, 100);
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
            money += Mathf.RoundToInt(factor + (factor * morale / 100));
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
        if (morale >= 100.0f)
        {
            morale = 100.0f;
        }
    }

    public void AddSecurity(float toAdd)
    {
        security += toAdd;
        if (security >= 100.0f)
        {
            security = 100.0f;
        }
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
        else if (type.Equals("BB"))
        {
            return basketballCD;
        }
        else if (type.Equals("RR"))
        {
            return recRoomCD;
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

    public void EndTurn()
    {
        turn++;
        incrementLevelCounters("level");
        money += income;
        if (money <= 0)
        {
            money = 0;
        }
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

    public bool CheckTutorial()
    {
        return level == 0;
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
            securityTurns++;
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
            securityTurns = 0;
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

    public List<string> GetUnlockedUpgrades()
    {
        return unlockedUpgrades;
    }

    public List<string> GetBuiltUpgrades()
    {
        return builtUpgrades;
    }


    public void ResetData()
    {
        if (username == "")
        {
            username = "Juan";
        }

        money = 5000;
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
        level = 0;
        checkHealthCD1 = 0;
        checkHealthCD2 = 0;
        checkHealthCD3 = 0;
        birdCD = 0;
        clamCD1 = 0;
        clamCD2 = 0;
        clamCD3 = 0;
        turtleCD = 0;
        researchCD = 0;
        radarCD = 0;
        basketballCD = 0;
        recRoomCD = 0;

        unlockedUpgrades = new List<string> { "Basketball Court", "Radio", "Service Boat" };
        builtUpgrades = new List<string> { "Radio" };
        touristsInspected = 0;
        fishermenCaught = 0;
        radarActive = false;
        message = "";
        currentObjectives = new List<string>();
        daySpawn = false;
        nightSpawn = false;
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

        units = new List<SaveUnit>();
        upgrades = new List<SaveUpgrade>();
        time = new SaveTime();
    }

    public void LoadData(PlayerState playerState)
    {
        this.username = playerState.username;
        this.money = playerState.GetMoney();
        this.income = playerState.GetIncome();
        this.research = playerState.GetResearch();
        this.manpower = playerState.GetManpower();
        this.tourists = playerState.GetTourists();
        this.fishermen = playerState.GetFishermen();
        this.morale = playerState.GetMorale();
        this.security = playerState.GetSecurity();
        this.trueReefHealth = playerState.GetTrueHealth();
        this.seenReefHealth = playerState.GetHealth();
        this.turn = playerState.GetTurn();
        this.level = playerState.GetLevel();

        this.checkHealthCD1 = playerState.FetchCD("CH1");
        this.checkHealthCD2 = playerState.FetchCD("CH2");
        this.checkHealthCD3 = playerState.FetchCD("CH3");
        this.birdCD = playerState.FetchCD("B");
        this.clamCD1 = playerState.FetchCD("C1");
        this.clamCD2 = playerState.FetchCD("C2");
        this.clamCD3 = playerState.FetchCD("C3");
        this.turtleCD = playerState.FetchCD("T");
        this.researchCD = playerState.FetchCD("R");
        this.radarCD = playerState.FetchCD("RADAR");
        this.basketballCD = playerState.FetchCD("BB");
        this.recRoomCD = playerState.FetchCD("RR");

        this.unlockedUpgrades = playerState.GetUnlockedUpgrades();
        this.builtUpgrades = playerState.GetBuiltUpgrades();
        this.touristsInspected = playerState.GetTouristScore();
        this.fishermenCaught = playerState.GetCatchScore();
        this.radarActive = playerState.GetRadarState();
        this.message = "";
        this.currentObjectives = playerState.GetObjectives();
        this.daySpawn = playerState.SpawnedDay();
        this.nightSpawn = playerState.SpawnedNight();
        this.AIS = playerState.CheckAIS();
        this.SAT = playerState.CheckSAT();
        this.SS = playerState.CheckSS();
        this.levelTurns = playerState.levelTurns;
        this.healthCount = playerState.healthCount;
        this.patrols = playerState.patrols;
        this.birdCount = playerState.birdCount;
        this.securityTurns = playerState.securityTurns;
        this.monitorClams = playerState.monitorClams;
        this.moraleTurns = playerState.moraleTurns;
        this.tagCount = playerState.tagCount;

        this.units = playerState.units;
        this.upgrades = playerState.upgrades;
        this.time = playerState.time;
    }

    public void SaveData(ref PlayerState playerState)
    {
        playerState.username = this.username;
        playerState.money = this.GetMoney();
        playerState.income = this.GetIncome();
        playerState.research = this.GetResearch();
        playerState.manpower = this.GetManpower();
        playerState.tourists = this.GetTourists();
        playerState.fishermen = this.GetFishermen();
        playerState.morale = this.GetMorale();
        playerState.security = this.GetSecurity();
        playerState.trueReefHealth = this.GetTrueHealth();
        playerState.seenReefHealth = this.GetHealth();
        playerState.turn = this.GetTurn();
        playerState.level = this.GetLevel();

        playerState.checkHealthCD1 = this.FetchCD("CH1");
        playerState.checkHealthCD2 = this.FetchCD("CH2");
        playerState.checkHealthCD3 = this.FetchCD("CH3");
        playerState.birdCD = this.FetchCD("B");
        playerState.clamCD1 = this.FetchCD("C1");
        playerState.clamCD2 = this.FetchCD("C2");
        playerState.clamCD3 = this.FetchCD("C3");
        playerState.turtleCD = this.FetchCD("T");
        playerState.researchCD = this.FetchCD("R");
        playerState.radarCD = this.FetchCD("RADAR");
        playerState.basketballCD = this.FetchCD("BB");
        playerState.recRoomCD = this.FetchCD("RR");

        playerState.unlockedUpgrades = this.GetUnlockedUpgrades();
        playerState.builtUpgrades = this.GetBuiltUpgrades();
        playerState.touristsInspected = this.GetTouristScore();
        playerState.fishermenCaught = this.GetCatchScore();
        playerState.radarActive = this.GetRadarState();
        playerState.message = "";
        playerState.currentObjectives = this.GetObjectives();
        playerState.daySpawn = this.SpawnedDay();
        playerState.nightSpawn = this.SpawnedNight();
        playerState.AIS = this.CheckAIS();
        playerState.SAT = this.CheckSAT();
        playerState.SS = this.CheckSS();
        playerState.levelTurns = this.levelTurns;
        playerState.healthCount = this.healthCount;
        playerState.patrols = this.patrols;
        playerState.birdCount = this.birdCount;
        playerState.securityTurns = this.securityTurns;
        playerState.monitorClams = this.monitorClams;
        playerState.moraleTurns = this.moraleTurns;
        playerState.tagCount = this.tagCount;

        playerState.units = this.units;
        playerState.upgrades = this.upgrades;
        playerState.time = this.time;
    }
}