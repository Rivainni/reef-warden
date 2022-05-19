using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextUpdate : MonoBehaviour
{
    MainUI mainUI;
    Text current;

    // Start is called before the first frame update
    void Start()
    {
        mainUI = GetComponentInParent<MainUI>();
        current = GetComponent<Text>();
    }

    public void UpdateText()
    {
        PlayerState currentState = mainUI.GetPlayerState();

        if (current.text.Contains("₱: "))
        {
            if (mainUI.GetPlayerState().GetIncome() < 0)
            {
                current.text = "₱: " + currentState.GetMoney() + " (" + currentState.GetIncome() + ")";
            }
            else
            {
                current.text = "₱: " + currentState.GetMoney();
            }
        }
        else if (current.text.Contains("RP: "))
        {
            current.text = "RP: " + currentState.GetResearch();
        }
        else if (current.text.Contains("MANPOWER: "))
        {
            current.text = "MANPOWER: " + currentState.GetManpower();
        }
        else if (current.text.Contains("TOURISTS: "))
        {
            current.text = "TOURISTS: " + currentState.GetTourists();
        }
        else if (current.text.Contains("MOR: "))
        {
            current.text = "MOR: " + Mathf.Round(currentState.GetMorale());
        }
        else if (current.text.Contains("SEC: "))
        {
            current.text = "SEC: " + Mathf.Round(currentState.GetSecurity());
        }
        else if (current.text.Contains("HP: "))
        {
            if (currentState.GetLastHealthCheck() >= 5)
            {
                current.text = "HP: " + "!!!";
            }
            else
            {
                current.text = "HP: " + Mathf.Round(currentState.GetHealth());
            }
        }
        else if (current.text.Contains(":"))
        {
            current.text = ": " + currentState.GetMessage();
        }
    }

    public void UpdateObjective(string objective)
    {
        current.text = objective;
    }

    public void UpdateLog(string message)
    {
        current.text = message;
    }
}