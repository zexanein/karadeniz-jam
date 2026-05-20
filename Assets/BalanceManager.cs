using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BalanceManager : BehaviourSingleton<BalanceManager>
{
    public Transform balanceTransform;
    public Transform leftPanTransform;
    public Transform rightPanTransform;
    public GameObject leftBag;
    public SpriteRenderer leftItemRenderer;
    public GameObject rightBag;
    public SpriteRenderer rightItemRenderer;
    
    public GameObject leftGiveButton;
    public GameObject rightGiveButton;
    
    public event Action OnBalanceUpdated;
    
    public RectTransform arrowTransform;
    private Tween _arrowTween;
    
    public List<ItemEntry> leftEntries = new();
    public List<ItemEntry> rightEntries = new();

    [Header("Balance Settings")]
    [SerializeField] private float maxAngle = 25f;
    
    [Header("Response Tuning")]
    [Tooltip("Yay sertliği. Yüksek = hızlı tepki, sert dönüş")]
    [SerializeField] private float springStiffness = 80f;
    [Tooltip("Sönümleme. Düşük = uzun salınım, yüksek = çabuk söner")]
    [SerializeField] private float damping = 6f;
    [Tooltip("Küçük oransal farkları abartır. 1 = lineer, 2-3 = hassas terazi hissi")]
    [SerializeField] private float sensitivityCurve = 2f;
    
    [Header("Pan Settings")]
    [SerializeField] private bool keepPansUpright = true;

    private float _currentAngle;
    private float _angularVelocity;
    
    public void ShowArrow()
    {
        arrowTransform.gameObject.SetActive(true);
        _arrowTween?.Kill();
        arrowTransform.anchoredPosition = Vector2.up * 0.3f;
        _arrowTween = arrowTransform.DOAnchorPosY(0.1f, 0.2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    public void HideArrow()
    {
        _arrowTween?.Kill();
        arrowTransform.gameObject.SetActive(false);
    }
    
    public void AddToLeft(ItemEntry entry, int quantity)
    {
        Debug.Log("AddToLeft: " + entry.blueprint.id + " x" + quantity);
        if (entry == null || entry.blueprint == null) return;
        var found = leftEntries.Find(e => e.blueprint == entry.blueprint);
        if (found != null)
        {
            found.quantity += quantity;
        }
        else
        {
            var newEntry = new ItemEntry
            {
                blueprint = entry.blueprint,
                quantity = quantity
            };
            leftEntries.Add(newEntry);
        }
        
        if (found is { quantity: <= 0 })
        {
            leftEntries.Remove(found);
        }

        if (leftEntries.Count == 1)
        {
            var blueprint = leftEntries[0].blueprint;
            leftItemRenderer.sprite = blueprint.itemIcon;
            leftItemRenderer.transform.localPosition = blueprint.panOffset;
            leftItemRenderer.transform.localRotation = Quaternion.Euler(blueprint.panRotation);
        }
        
        leftItemRenderer.gameObject.SetActive(leftEntries.Count == 1);
        leftBag.SetActive(leftEntries.Count > 1);
        OnBalanceChanged();
    }

    public void AddToRight(ItemEntry entry, int quantity)
    {
        if (entry == null || entry.blueprint == null) return;
        var found = rightEntries.Find(e => e.blueprint == entry.blueprint);
        if (found != null)
        {
            found.quantity += quantity;
        }
        else
        {
            var newEntry = new ItemEntry
            {
                blueprint = entry.blueprint,
                quantity = quantity
            };
            rightEntries.Add(newEntry);
        }

        if (found is { quantity: <= 0 })
        {
            rightEntries.Remove(found);
        }

        if (rightEntries.Count == 1)
        {
            var blueprint = rightEntries[0].blueprint;
            rightItemRenderer.sprite = blueprint.itemIcon;
            rightItemRenderer.transform.localPosition = blueprint.panOffset;
            rightItemRenderer.transform.localRotation = Quaternion.Euler(blueprint.panRotation);
        }
        
        rightItemRenderer.gameObject.SetActive(rightEntries.Count == 1);
        rightBag.SetActive(rightEntries.Count > 1);
        OnBalanceChanged();
    }
    
    public void GiveItemToCustomer(bool fromLeft)
    {
        var count = fromLeft ? leftEntries.Sum(e => e.quantity) : rightEntries.Sum(e => e.quantity);
        if (fromLeft)
        {
            var copy = new List<ItemEntry>(leftEntries);
            
            foreach (var entry in copy)
            {
                AddToLeft(entry, -entry.quantity);
                InventoryManager.Instance.RemoveItem(entry.blueprint.id, entry.quantity);
            }
        }
        
        else
        {
            var copy = new List<ItemEntry>(rightEntries);
            foreach (var entry in copy)
            {
                AddToRight(entry, -entry.quantity);
                InventoryManager.Instance.RemoveItem(entry.blueprint.id, entry.quantity);
            }
        }
        
        CustomerManager.Instance.GiveItemToCustomer(count);
    }
    
    private void OnBalanceChanged()
    {
        Soundmanager.ScaleEffectSound = true;
        UpdateGiveButtons();
        OnBalanceUpdated?.Invoke();
    }

    public void UpdateGiveButtons()
    {
        var requested = CustomerManager.Instance.ActiveCustomerData?.requestedItem;
        if (requested == null || requested.blueprint == null)
        {
            leftGiveButton.SetActive(false);
            rightGiveButton.SetActive(false);
            return;
        }
        
        var leftMatch = leftEntries.All(lE => lE.blueprint.id == requested.blueprint.id);
        var rightMatch = rightEntries.All(rE => rE.blueprint.id == requested.blueprint.id);
        
        leftGiveButton.SetActive(leftEntries.Count > 0 && leftMatch);
        rightGiveButton.SetActive(rightEntries.Count > 0 && rightMatch);
    }

    private void Update()
    {
        UpdateBalance();
    }

    public void UpdateBalance()
    {
        if (balanceTransform == null) return;

        var leftWeight = CalculateWeight(leftEntries);
        var rightWeight = CalculateWeight(rightEntries);

        var ratio = CalculateBalanceRatio(leftWeight, rightWeight);

        // Non-linear hassasiyet: küçük oransal farklar abartılır
        var sign = Mathf.Sign(ratio);
        var absRatio = Mathf.Abs(ratio);
        var curved = sign * Mathf.Pow(absRatio, 1f / sensitivityCurve);

        var targetAngle = curved * maxAngle;

        // Spring-damper integration
        var force = -springStiffness * (_currentAngle - targetAngle) - damping * _angularVelocity;
        _angularVelocity += force * Time.deltaTime;
        _currentAngle += _angularVelocity * Time.deltaTime;

        balanceTransform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);

        if (keepPansUpright)
            UpdatePansUpright();
    }

    /// <summary>
    /// -1 (tam sağa) ile +1 (tam sola) arası oransal denge.
    /// (left - right) / max(left, right) formülü.
    /// </summary>
    private float CalculateBalanceRatio(float left, float right)
    {
        // İkisi de boşsa denge mükemmel
        if (left <= 0f && right <= 0f) return 0f;

        float heavier = Mathf.Max(left, right);
        return (left - right) / heavier;
    }

    private void UpdatePansUpright()
    {
        Quaternion counterRotation = Quaternion.Euler(0f, 0f, -_currentAngle);
        if (leftPanTransform != null)
            leftPanTransform.localRotation = counterRotation;
        if (rightPanTransform != null)
            rightPanTransform.localRotation = counterRotation;
    }

    private float CalculateWeight(List<ItemEntry> entries)
    {
        var total = 0f;
        foreach (var entry in entries)
        {
            if (entry != null && entry.blueprint != null)
                total += entry.blueprint.netWeightGrams * entry.quantity;
        }
        return total;
    }
}