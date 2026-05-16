using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : BehaviourSingleton<InventoryManager>
{
    private readonly Dictionary<string, ItemEntry> _items = new();
    private readonly Dictionary<string, ItemEntry> _drawerItems = new();
    [SerializeField] private GameObject drawerOverlay;
    [SerializeField] private SpriteRenderer drawerOpen;
    public bool IsDrawerOpen { get; private set; }
    public event Action OnInventoryUpdated;

    public void SetDrawerSpriteState(bool open)
    {
        drawerOpen.gameObject.SetActive(open);
        if (open) SetDrawerSpriteColor(false);
    }

    public void SetDrawerSpriteColor(bool interactable)
    {
        var color = interactable ? new Color32(165,200,100, 255) : (Color32) Color.white;
        drawerOpen.color = color;
    }
    
    public void OpenDrawer()
    {
        if (IsDrawerOpen) return;
        drawerOverlay.SetActive(true);
        SetDrawerSpriteState(true);

        foreach (var item in _drawerItems.Values)
        {
            ItemRegistry.Instance.SpawnItemToDrawer(item.blueprint.id, item.quantity);
        }
        
        IsDrawerOpen = true;
    }

    public void CloseDrawer()
    {
        if (!IsDrawerOpen) return;
        SetDrawerSpriteState(false);
        ItemRegistry.Instance.ClearDrawerItems();
        drawerOverlay.SetActive(false);
        IsDrawerOpen = false;
    }
    
    public bool HasDrawerItem(string itemId, int quantity = 1)
    {
        return _drawerItems.TryGetValue(itemId, out var item) && item.quantity >= quantity;
    }
    
    public void AddToDrawer(ItemBlueprint blueprint, int quantity)
    {
        if (_drawerItems.TryGetValue(blueprint.id, out var item))
        {
            item.quantity += quantity;
        }
        
        else
        {
            _drawerItems[blueprint.id] = new ItemEntry { blueprint = blueprint, quantity = quantity };
        }
    }
    
    public void AddItem(ItemBlueprint blueprint, int quantity)
    {
        if (_items.TryGetValue(blueprint.id, out var item))
        {
            item.quantity += quantity;
        }
        
        else
        {
            _items[blueprint.id] = new ItemEntry { blueprint = blueprint, quantity = quantity };
        }
        
        OnInventoryUpdated?.Invoke();
    }
    
    public void RemoveItem(string itemId, int quantity)
    {
        if (!_items.TryGetValue(itemId, out var item)) return;
        
        item.quantity -= quantity;
        if (item.quantity <= 0) _items.Remove(itemId);
        
        OnInventoryUpdated?.Invoke();
    }
    
    public bool HasItem(string itemId, int quantity = 1)
    {
        return _items.TryGetValue(itemId, out var item) && item.quantity >= quantity;
    }
}

[Serializable]
public class ItemEntry
{
    public ItemBlueprint blueprint;
    public int quantity;
}