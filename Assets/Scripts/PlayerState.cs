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

    public void Clean()
    {
        money = 0;
        research = 0;
        manpower = 0;
        tourists = 0;
        morale = 0;
        security = 0;
        reefHealth = 0;
        turn = 1;
    }

    public void endTurn()
    {
        turn++;
    }
}