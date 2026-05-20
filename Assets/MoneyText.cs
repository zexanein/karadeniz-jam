using System;
using TMPro;
using UnityEngine;

public class MoneyText : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    
    private void OnEnable()
    {
        EconomyManager.Instance.OnMoneyChanged += UpdateMoneyText;
        UpdateMoneyText(EconomyManager.Instance.Money);
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnMoneyChanged -= UpdateMoneyText;
    }

    private void UpdateMoneyText(int money)
    {
        moneyText.text = money.ToString();
    }
}
