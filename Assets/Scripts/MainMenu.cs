using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject settingsPrefab;
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

    }

    public void Exit()
    {
        Application.Quit();
    }
}
