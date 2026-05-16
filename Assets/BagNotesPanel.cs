using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BagNotesPanel : MonoBehaviour
{
    public BalanceItem balanceItemPrefab;
    public BalanceItem leftItem;
    public Transform leftItemsParent;
    public Transform rightItemsParent;
    public TMP_Text descriptionText;
    public bool state;
    
    private void OnEnable()
    {
        BalanceManager.Instance.OnBalanceUpdated += UpdateNotes;
    }

    private Sequence _sequence;
    
    public void ToggleState()
    {
        state = !state;
        var targetY = state ? -5 : -385;
        var angle = state ? 3 : 0;
        _sequence?.Kill();
        _sequence = DOTween.Sequence();

        _sequence.Join((transform as RectTransform)
            .DOAnchorPosY(targetY, 0.5f)
            .SetEase(state ? Ease.InSine : Ease.OutSine));
        _sequence.Join(transform.DORotate(Vector3.forward * angle, 0.5f).SetEase(state ? Ease.InSine : Ease.OutSine));
    }
    
    private void OnDisable() => BalanceManager.Instance.OnBalanceUpdated -= UpdateNotes;

    private void UpdateNotes()
    {
        foreach (Transform child in leftItemsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in rightItemsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var leftEntry in BalanceManager.Instance.leftEntries)
        {
            var newBalanceItem = Instantiate(balanceItemPrefab, leftItemsParent);
            newBalanceItem.Initialize(leftEntry, true);
        }

        foreach (var rightEntry in BalanceManager.Instance.rightEntries)
        {
            var newBalanceItem = Instantiate(balanceItemPrefab, rightItemsParent);
            newBalanceItem.Initialize(rightEntry, false);
        }
    }
}
