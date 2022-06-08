using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SaveTime
{
    public string currentTime;
    public string targetTime;
}

public class TimeController : MonoBehaviour, IDataPersistence
{
    [SerializeField] float timeMultiplier;
    [SerializeField] float startHour;
    [SerializeField] float sunriseHour;
    [SerializeField] float sunsetHour;
    [SerializeField] Color dayAmbientLight;
    [SerializeField] Color nightAmbientLight;
    [SerializeField] AnimationCurve lightChangeCurve;
    [SerializeField] Light sunLight;
    [SerializeField] float maxSunLightIntensity;
    [SerializeField] Light moonLight;
    [SerializeField] float maxMoonLightIntensity;
    [SerializeField] Text timeIndicator;
    DateTime currentTime;
    DateTime targetTime;
    TimeSpan sunriseTime;
    TimeSpan sunsetTime;
    PlayerState playerState;
    bool day;
    bool pause;

    void Start()
    {
        StartCoroutine(WaitForLoad());
    }

    IEnumerator WaitForLoad()
    {
        yield return new WaitUntil(() => playerState != null);
        if (playerState.time.currentTime == "")
        {
            currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
            targetTime = currentTime;
        }
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
        timeIndicator.text = currentTime.ToString("HH:mm");
        pause = true;
        day = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ((targetTime - currentTime).TotalSeconds >= 0 && !pause)
        {
            UpdateTimeOfDay();
            RotateSun();
            UpdateLightSettings();
        }
        else if (!pause && playerState != null)
        {
            DateTime correctedTime = new DateTime();
            correctedTime = correctedTime.Date.AddHours(currentTime.Hour);
            targetTime = correctedTime;
            pause = true;
            timeIndicator.text = correctedTime.ToString("HH:mm");
        }
    }

    void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);
        timeIndicator.text = currentTime.ToString("HH:mm");
    }

    void RotateSun()
    {
        float sunLightRotation;

        if (currentTime.TimeOfDay >= sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
            day = true;
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
            day = false;
        }

        sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }

    void UpdateLightSettings()
    {
        float intensity = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        sunLight.intensity = Mathf.Lerp(0, maxSunLightIntensity, lightChangeCurve.Evaluate(intensity));
        moonLight.intensity = Mathf.Lerp(maxMoonLightIntensity, 0, lightChangeCurve.Evaluate(intensity));
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(intensity));
    }

    TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }

        return difference;
    }

    public bool IsDay()
    {
        return day;
    }

    public void ForwardTime()
    {
        if (pause)
        {
            pause = false;
        }
        else
        {
            pause = true;
        }
        targetTime = currentTime.AddHours(2);
    }

    public bool CheckPause()
    {
        return pause;
    }

    public void LoadData(PlayerState playerState)
    {
        this.playerState = playerState;

        SaveTime temp = playerState.time;
        if (temp.currentTime != "" && temp.targetTime != "")
        {
            this.currentTime = DateTime.ParseExact(temp.currentTime, "dd/MM/yyyy hh:mm:ss tt", null);
            this.targetTime = DateTime.ParseExact(temp.targetTime, "dd/MM/yyyy hh:mm:ss tt", null);

            DateTime correctedTime = new DateTime();
            correctedTime = correctedTime.Date.AddHours(currentTime.Hour);
            currentTime = correctedTime;
            targetTime = correctedTime;
            timeIndicator.text = correctedTime.ToString("HH:mm");
        }
    }
    public void SaveData(ref PlayerState playerState)
    {
        SaveTime saveTime = new SaveTime();

        if (currentTime.ToString() != "01/01/0001 12:00:00 AM")
        {
            saveTime.currentTime = currentTime.ToString("dd/MM/yyyy hh:mm:ss tt");
            saveTime.targetTime = (currentTime.AddHours(2)).ToString("dd/MM/yyyy hh:mm:ss tt");
            playerState.time = saveTime;
        }
    }
}
