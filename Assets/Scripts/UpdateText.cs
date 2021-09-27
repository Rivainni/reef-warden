using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateText : MonoBehaviour
{
    MainUI mainUI;
    Text current;

    // Start is called before the first frame update
    void Start()
    {
        mainUI = GetComponentInParent<MainUI>();
        current = GetComponent<Text>();
    }

    public void UpdateUIElement()
    {
        PlayerState currentState = mainUI.GetPlayerState();

        if (current.text.Contains("GP: "))
        {
            current.text = "GP: " + currentState.GetMoney() + " + (" + currentState.GetIncome() + ")";
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
            current.text = "HP: " + Mathf.Round(currentState.GetHealth());
        }
    }
}