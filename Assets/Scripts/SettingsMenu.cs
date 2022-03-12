using System.Collections;
using System.Collections.Generic;
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
    Resolution[] resolutions;

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
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void Save()
    {
        audioMixer.SetFloat("volume", currentVolume);
    }

    public void Cancel()
    {

    }
}
