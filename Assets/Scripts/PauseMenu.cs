using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] MainUI mainUI;
    [SerializeField] CameraController cameraController;
    public void Resume()
    {
        mainUI.FreezeInput(false);
        cameraController.FreezeCamera(false);
        gameObject.SetActive(false);
    }

    public void Exit()
    {
        SceneManager.LoadScene("Main Menu");
    }
}