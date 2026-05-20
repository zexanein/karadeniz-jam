using System;
using Unity.VisualScripting;
using UnityEngine;

public class EconomyManager : BehaviourSingleton<EconomyManager>
{
    public int Money { get; private set; } = 1000;
    
    public bool TrySpendMoney(int amount)
    {
        if (Money < amount) return false;
        AddMoney(-amount);
        return true;
    }
    
    public void AddMoney(int amount)
    {
        if (amount == 0) return;
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }
    
    public event Action<int> OnMoneyChanged;
}
