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
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider volumeSlider;
    float currentVolume;
    float currentScale;
    bool currentFullscreen;
    Resolution currentResolution;
    Resolution[] resolutions;
    void Start()
    {
        resolutions = Screen.resolutions;
        UpdateSettings();
    }

    public void SetVolume(float volume)
    {
        currentVolume = volume;
        int result = VolumeToInt(volume);
        volumeText.text = result.ToString() + "%";
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
        Screen.fullScreen = currentFullscreen;
        Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
        audioMixer.SetFloat("volume", currentVolume);
        TextRW.WriteSettings(currentResolution.width, currentResolution.height, Convert.ToInt32(currentFullscreen), (int)currentScale, VolumeToInt(currentVolume));
        UpdateSettings();
    }

    public void Cancel()
    {

    }

    void UpdateSettings()
    {
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

        fullscreenToggle.isOn = Convert.ToBoolean(TextRW.GetSettings()[2]);
        scaleText.text = TextRW.GetSettings()[3].ToString() + "%";
        volumeSlider.value = VolumeToFloat(TextRW.GetSettings()[4]);
        volumeText.text = TextRW.GetSettings()[4].ToString() + "%";
    }

    int VolumeToInt(float volume)
    {
        float conversion = (1 - (Mathf.Abs(volume) / 80)) * 100;
        return Mathf.RoundToInt(conversion);
    }

    float VolumeToFloat(int volume)
    {
        float conversion = (1 - ((float)volume / 100)) * -80;
        return conversion;
    }
}
