using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        if (!Debug.isDebugBuild)
        {
            Destroy(GameObject.Find("Start Game (Skip Tutorial)"));
        }
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
