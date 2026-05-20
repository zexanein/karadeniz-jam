using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagNotesPanel : MonoBehaviour
{
    public BalanceItem balanceItemPrefab;
    public Transform leftItemsParent;
    public Transform rightItemsParent;
    public TMP_Text descriptionText;
    public Button saveButton;
    public bool state;

    public Color buttonActiveColor;
    public Color buttonInactiveColor;

    private Sequence _sequence;

    private void OnEnable()
    {
        BalanceManager.Instance.OnBalanceUpdated += UpdateNotes;
        saveButton.onClick.AddListener(SaveNote);
        UpdateNotes();
    }

    private void OnDisable()
    {
        if (BalanceManager.Instance != null)
            BalanceManager.Instance.OnBalanceUpdated -= UpdateNotes;
    }

    private void SaveNote()
    {
        var left = BalanceManager.Instance.leftEntries;
        var right = BalanceManager.Instance.rightEntries;
        var comparison = GetCurrentComparison();

        EqualityNotesPanel.Instance.RegisterNote(left, right, comparison);

        // Buton state'ini UpdateNotes hallediyor, manuel set etmiyorum
        UpdateNotes();
    }

    public void ToggleState()
    {
        state = !state;
        Soundmanager.TurnPaperSound = true;
        var targetY = state ? -5 : -385;
        var angle = state ? -3 : 0;
        _sequence?.Kill();
        _sequence = DOTween.Sequence();

        _sequence.Join((transform as RectTransform)
            .DOAnchorPosY(targetY, 0.4f)
            .SetEase(state ? Ease.OutBack : Ease.InOutBack));
        _sequence.Join(transform.DORotate(Vector3.forward * angle, 0.4f).SetEase(state ? Ease.OutBack : Ease.InOutBack));
    }

    private void UpdateNotes()
    {
        foreach (Transform child in leftItemsParent)
            Destroy(child.gameObject);
        foreach (Transform child in rightItemsParent)
            Destroy(child.gameObject);

        float leftWeight = 0;
        float rightWeight = 0;

        foreach (var leftEntry in BalanceManager.Instance.leftEntries)
        {
            leftWeight += leftEntry.blueprint.netWeightGrams * leftEntry.quantity;
            var newBalanceItem = Instantiate(balanceItemPrefab, leftItemsParent);
            newBalanceItem.Initialize(leftEntry, true);
        }

        foreach (var rightEntry in BalanceManager.Instance.rightEntries)
        {
            rightWeight += rightEntry.blueprint.netWeightGrams * rightEntry.quantity;
            var newBalanceItem = Instantiate(balanceItemPrefab, rightItemsParent);
            newBalanceItem.Initialize(rightEntry, false);
        }

        string description;
        if (leftWeight > rightWeight)
            description = "Denge: Sol kefe daha ağır.";
        else if (rightWeight > leftWeight)
            description = "Denge: Sağ kefe daha ağır.";
        else
            description = "Denge: Ağırlıklar Eşit.";

        descriptionText.text = description;

        UpdateSaveButton();
    }

    private void UpdateSaveButton()
    {
        var left = BalanceManager.Instance.leftEntries;
        var right = BalanceManager.Instance.rightEntries;

        bool bothSidesHaveItems = left.Count > 0 && right.Count > 0;

        var saveButtonText = saveButton.GetComponentInChildren<TMP_Text>();
        var saveButtonImage = saveButton.GetComponentInChildren<Image>();

        if (!bothSidesHaveItems)
        {
            saveButtonImage.color = buttonInactiveColor;
            saveButton.interactable = false;
            saveButtonText.text = "Not Al";
            return;
        }

        var comparison = GetCurrentComparison();
        bool registered = EqualityNotesPanel.Instance.IsNoteRegistered(left, right, comparison);

        saveButtonImage.color = registered ? buttonInactiveColor : buttonActiveColor;
        saveButton.interactable = !registered;
        saveButtonText.text = registered ? "Not Alındı!" : "Not Al";
    }

    private ComparisonType GetCurrentComparison()
    {
        float leftWeight = 0;
        float rightWeight = 0;
        foreach (var e in BalanceManager.Instance.leftEntries)
            leftWeight += e.blueprint.netWeightGrams * e.quantity;
        foreach (var e in BalanceManager.Instance.rightEntries)
            rightWeight += e.blueprint.netWeightGrams * e.quantity;

        if (Mathf.Approximately(leftWeight, rightWeight)) return ComparisonType.Equal;
        return leftWeight > rightWeight ? ComparisonType.Greater : ComparisonType.Less;
    }
}