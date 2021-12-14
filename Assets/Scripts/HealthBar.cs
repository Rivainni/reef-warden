using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Gradient gradient;
    [SerializeField] Image fill;

    public void SetHealth(float health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}