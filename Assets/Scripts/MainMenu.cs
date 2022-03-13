using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene("Cutscene");
    }

    public void Skip()
    {
        SceneManager.LoadScene("Testing");
    }

    public void Settings()
    {
        TextRW.ReadSettings();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
