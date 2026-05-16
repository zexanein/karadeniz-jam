using System;
using System.Collections.Generic;
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
    
    public event Action OnBalanceUpdated;
    
    public RectTransform arrowTransform;
    private Tween _arrowTween;
    

    public List<ItemEntry> leftEntries = new();
    public List<ItemEntry> rightEntries = new();

    public TMP_Text leftText;
    public Button leftIncreaseButton;
    public Button leftDecreaseButton;
    
    [Header("Balance Settings")]
    [SerializeField] private float maxAngle = 25f;
    [SerializeField] private float weightDifferenceForMaxAngle = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Pan Settings")]
    [SerializeField] private bool keepPansUpright = true;
    
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
        if (found != null)        {
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

        if (quantity < 0)
        {
        }
        
        if (found is { quantity: <= 0 })
        {
            leftEntries.Remove(found);
        }

        if (leftEntries.Count == 1)
        {
            leftItemRenderer.sprite = leftEntries[0].blueprint.itemIcon;
        }
        
        leftItemRenderer.gameObject.SetActive(leftEntries.Count == 1);
        leftBag.SetActive(leftEntries.Count > 1);
        
        OnBalanceUpdated?.Invoke();
    }

    public void AddToRight(ItemEntry entry, int quantity)
    {
        if (entry == null || entry.blueprint == null) return;
        var found = rightEntries.Find(e => e.blueprint == entry.blueprint);
        if (found != null)        {
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
            rightItemRenderer.sprite = rightEntries[0].blueprint.itemIcon;
        }
        
        rightItemRenderer.gameObject.SetActive(rightEntries.Count == 1);
        rightBag.SetActive(rightEntries.Count > 1);

        
        OnBalanceUpdated?.Invoke();
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

        var difference = leftWeight - rightWeight;

        var normalized = Mathf.Clamp(difference / weightDifferenceForMaxAngle, -1f, 1f);
        var targetAngle = normalized * maxAngle;

        var targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        balanceTransform.localRotation = Quaternion.Lerp(
            balanceTransform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (keepPansUpright)
            UpdatePansUpright();
    }

    private void UpdatePansUpright()
    {
        // Kol z ekseninde dönerken kefelerin dünyada dik kalması için
        // local rotation'larını parent'ın dönüşünün tersi yap.
        float armZ = balanceTransform.localEulerAngles.z;
        // localEulerAngles 0-360 döndürür, -25 yerine 335 verir. Düzelt:
        if (armZ > 180f) armZ -= 360f;

        Quaternion counterRotation = Quaternion.Euler(0f, 0f, -armZ);

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