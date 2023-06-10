using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] LevelLoader levelLoader;
    [SerializeField] DataPersistenceManager dataPersistenceManager;
    void Start()
    {
        if (!Debug.isDebugBuild)
        {
            Destroy(GameObject.Find("Start Game (Skip Tutorial)"));
        }
    }

    public void StartGame()
    {
        if (dataPersistenceManager.GetPlayerState().CheckTutorial())
        {
            levelLoader.LoadLevel("Cutscene");
        }
        else
        {
            levelLoader.LoadLevel("Main Game");
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
