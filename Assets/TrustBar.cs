using System;
using UnityEngine;
using UnityEngine.UI;

public class TrustBar : BehaviourSingleton<TrustBar>
{
    public int maxTrust = 100;
    public int CurrentTrust { get; private set; } = 0;
    public int initialTrust = 50;
    
    public Slider slider;
    public GameObject trustLoseScreen;

    private void OnEnable()
    {
        CurrentTrust = initialTrust;
        slider.maxValue = maxTrust;
        slider.value = CurrentTrust;
    }

    public void AddTrust(int count)
    {
        CurrentTrust = Mathf.Clamp(CurrentTrust + count, 0, maxTrust);
        slider.value = CurrentTrust;
        
        if (CurrentTrust <= 0)
        {
            trustLoseScreen.SetActive(true);
        }
    }
}