using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReputationBar : BehaviourSingleton<ReputationBar>
{
    public int maxReputation = 100;
    public int CurrentReputation { get; private set; } = 0;
    public int initialReputation = 20;
    
    public Slider slider;
    public GameObject reputationLoseScreen;

    private void OnEnable()
    {
        CurrentReputation = initialReputation;
        slider.maxValue = maxReputation;
        slider.value = CurrentReputation;
        UpdateThresholdIndex();
    }
    
    private int _thresholdIndex = 0;
    
    public int GetThresholdIndex()
    {
        return _thresholdIndex;
    }
    
    public int GetReputation()
    {
        if (reputationThresholds.Count == 0) return 5;
        return reputationThresholds[_thresholdIndex].reputationIncrease;
    }
    
    public List<ReputationThreshold> reputationThresholds = new();
    
    [Serializable]
    public class ReputationThreshold
    {
        public int threshold;
        public int reputationIncrease;
    }
    
    private void UpdateThresholdIndex()
    {
        _thresholdIndex = 0;
        
        for (var i = 0; i < reputationThresholds.Count; i++)
        {
            if (CurrentReputation >= reputationThresholds[i].threshold)
            {
                _thresholdIndex = i;
            }
            else break;
        }
    }

    public void AddReputation(int count)
    {
        CurrentReputation = Mathf.Clamp(CurrentReputation + count, 0, maxReputation);
        
        slider.value = CurrentReputation;
        
        if (CurrentReputation <= 0)
        {
            reputationLoseScreen.SetActive(true);
        }
        
        UpdateThresholdIndex();
    }
}
