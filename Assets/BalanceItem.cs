using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BalanceItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TMP_Text amountText;
    public Button increaseButton;
    public Button decreaseButton;
    
    private ItemEntry _cachedEntry;
    public bool isLeft;

    private void OnEnable()
    {
        increaseButton.onClick.AddListener(IncreaseAmountByOne);
        decreaseButton.onClick.AddListener(DecreaseAmountByOne);
        InventoryManager.Instance.OnInventoryUpdated += UpdateButtonStates;
    }

    private void OnDisable()
    {
        increaseButton.onClick.RemoveListener(IncreaseAmountByOne);
        decreaseButton.onClick.AddListener(DecreaseAmountByOne);
        InventoryManager.Instance.OnInventoryUpdated -= UpdateButtonStates;
    }

    private void UpdateButtonStates()
    {
        increaseButton.interactable = InventoryManager.Instance.HasItem(_cachedEntry.blueprint.id, _cachedEntry.quantity + 1);
        decreaseButton.interactable = _cachedEntry.quantity > 0;
    }

    public void Initialize(ItemEntry entry, bool isLeftSide)
    {
        iconImage.sprite = entry.blueprint.itemIcon;
        amountText.text = "x" + entry.quantity;
        
        increaseButton.interactable = InventoryManager.Instance.HasItem(entry.blueprint.id, entry.quantity + 1);
        decreaseButton.interactable = entry.quantity > 0;
        
        isLeft = isLeftSide;
        
        _cachedEntry = entry;
    }
    
    private void DecreaseAmountByOne()
    {
        Debug.Log("Decrease");
        if (isLeft) BalanceManager.Instance.AddToLeft(_cachedEntry, -1);
        else BalanceManager.Instance.AddToRight(_cachedEntry, -1);
        
        InventoryManager.Instance.AddToDrawer(_cachedEntry.blueprint, 1);
    }
    
    private void IncreaseAmountByOne()
    {
        Debug.Log("IncreaseAmount");
        if (isLeft) BalanceManager.Instance.AddToLeft(_cachedEntry, 1);
        else BalanceManager.Instance.AddToRight(_cachedEntry, 1);
        
        if (InventoryManager.Instance.HasDrawerItem(_cachedEntry.blueprint.id))
            InventoryManager.Instance.AddToDrawer(_cachedEntry.blueprint, -1);

        else
        {
            ItemRegistry.Instance.RemoveAnyFromDeskItems();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor("pointer");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor("default");
    }
}
