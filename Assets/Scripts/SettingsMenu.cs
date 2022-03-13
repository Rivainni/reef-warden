using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Text volumeText;
    [SerializeField] Text scaleText;
    [SerializeField] Dropdown resolutionDropdown;
    float currentVolume;
    float currentScale;
    bool currentFullscreen;
    Resolution currentResolution;
    Resolution[] resolutions;
    int[] currentSettings;
    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    public void SetVolume(float volume)
    {
        currentVolume = volume;
        float conversion = (1 - (Mathf.Abs(volume) / 80)) * 100;
        int result = Mathf.RoundToInt(conversion);
        volumeText.text = result.ToString() + "%";
    }

    public void SetScale(float scale)
    {
        currentScale = scale;
        scaleText.text = scale.ToString() + "%";
    }

    public void SetFullScreen(bool isFullscreen)
    {
        currentFullscreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        currentResolution = resolutions[resolutionIndex];
    }

    public void Save()
    {
        Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
        Screen.fullScreen = currentFullscreen;
        audioMixer.SetFloat("volume", currentVolume);
        TextRW.WriteSettings(currentResolution.width, currentResolution.height, Convert.ToInt32(currentFullscreen), (int)currentScale, (int)currentVolume);
    }

    public void Cancel()
    {

    }
}
