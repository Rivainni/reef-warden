using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState")]
public class PlayerState : ScriptableObject
{
    [SerializeField] int money;
    [SerializeField] int research;
    [SerializeField] int manpower;
    [SerializeField] int tourists;
    [SerializeField] float morale;
    [SerializeField] float security;
    [SerializeField] float reefHealth;

    [SerializeField] int turn;
    [SerializeField] int checkHealthCD1;
    [SerializeField] int checkHealthCD2;
    [SerializeField] int checkHealthCD3;
    [SerializeField] int birdCD;
    [SerializeField] int clamCD;
    [SerializeField] int turtleCD;
    [SerializeField] int researchCD;

    [SerializeField] int income;
    [SerializeField] string[] possibleActions;


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
        return reefHealth;
    }

    public int GetTurn()
    {
        return turn;
    }

    public string[] GetPossibleActions()
    {
        return possibleActions;
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
    }

    public void Clean()
    {
        money = 10000;
        research = 250;
        manpower = 6;
        tourists = 0;
        morale = 50;
        security = 50;
        reefHealth = 100;
        turn = 1;
        checkHealthCD1 = 0;
        checkHealthCD2 = 0;
        checkHealthCD3 = 0;
        birdCD = 0;
        clamCD = 0;
        turtleCD = 0;
        researchCD = 0;
    }

    public void nextTurn()
    {
        turn++;
    }
}