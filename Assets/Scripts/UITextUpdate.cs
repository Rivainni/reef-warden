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

        if (current.name == "Money")
        {
            if (mainUI.GetPlayerState().GetIncome() < 0)
            {
                current.text = currentState.GetMoney() + " (" + currentState.GetIncome() + ")";
            }
            else
            {
                current.text = "" + currentState.GetMoney();
            }
        }
        else if (current.name == "Research")
        {
            current.text = "" + currentState.GetResearch();
        }
        else if (current.name == "Manpower")
        {
            current.text = "" + currentState.GetManpower();
        }
        else if (current.name == "Tourists")
        {
            current.text = "" + currentState.GetTourists();
        }
        else if (current.name == "Morale")
        {
            current.text = "" + Mathf.Round(currentState.GetMorale());
        }
        else if (current.name == "Security")
        {
            current.text = "" + Mathf.Round(currentState.GetSecurity());
        }
        else if (current.name == "HP")
        {
            if (currentState.GetLastHealthCheck() >= 5)
            {
                current.text = "" + "!!!";
            }
            else
            {
                current.text = "" + Mathf.Round(currentState.GetHealth());
            }
        }
        else if (current.text.Contains(":"))
        {
            current.text = "" + currentState.GetMessage();
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