using UnityEngine;

public class InteractionManager : BehaviourSingleton<InteractionManager>
{
    private DraggableItem _currentSelectedItem;
    //[SerializeField] private SpriteRenderer drawerIndicator;
    public bool HoveringDrawer { get; set; }
    public bool HoveringBalanceLeft { get; set; }
    public bool HoveringBalanceRight { get; set; }
    public bool HoveringDrawerZone { get; set; }
    
    public void OnItemHover(DraggableItem item)
    {
        if (_currentSelectedItem != null) return;
        TooltipManager.Instance.Show($"{item.ItemEntry.quantity}x {item.ItemEntry.blueprint.itemName}");
        item.ScaleUp();
    }
    
    public void OnItemUnhover(DraggableItem item)
    {
        item.ScaleDown();
        TooltipManager.Instance.Hide();
    }
    
    public void OnItemSelect(DraggableItem item)
    {
        _currentSelectedItem = item;
        _currentSelectedItem.ToggleCollider(false);
        CursorManager.Instance.SetCursor("drag");


        if (!InventoryManager.Instance.IsDrawerOpen)
        {
            BalanceManager.Instance.ShowArrow();
            InventoryManager.Instance.SetDrawerSpriteState(true);
        }
    }
    
    public void OnItemDrag(DraggableItem item)
    {
        if (_currentSelectedItem != item) return;
        
        if (!InventoryManager.Instance.IsDrawerOpen)
            InventoryManager.Instance.SetDrawerSpriteColor(HoveringDrawer);

        if (item.InDrawer && !HoveringDrawerZone)
        {
            item.SetDefaultSortingLayer();
            InventoryManager.Instance.AddToDrawer(_currentSelectedItem.ItemEntry.blueprint, -1);
            ItemRegistry.Instance.RemoveFromDrawerItems(item.gameObject);
            item.InDrawer = false;
        }
        
        if (!item.InDrawer && HoveringDrawerZone)
        {
            item.SetDrawerSortingLayer();
            ItemRegistry.Instance.AddToDrawerItems(item.gameObject);
            InventoryManager.Instance.AddToDrawer(_currentSelectedItem.ItemEntry.blueprint, 1);
            item.InDrawer = true;
        }
        
        var mousePosition = Input.mousePosition;
        mousePosition.z = 10f;
        
        if (Camera.main == null) return;
        var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        item.transform.position = worldPosition;
    }
    
    public void DropItem()
    {
        if (HoveringDrawer)
        {
            InventoryManager.Instance.AddToDrawer(_currentSelectedItem.ItemEntry.blueprint, 1);
            Destroy(_currentSelectedItem.gameObject);
        }
        
        else if (HoveringBalanceLeft)
        {
            Debug.Log($"Adding {_currentSelectedItem.ItemEntry.blueprint.itemName} to left balance");
            BalanceManager.Instance.AddToLeft(_currentSelectedItem.ItemEntry, 1);
            ItemRegistry.Instance.RemoveFromDeskItems(_currentSelectedItem);
            Destroy(_currentSelectedItem.gameObject);
        }
        
        else if (HoveringBalanceRight)
        {
            Debug.Log($"Adding {_currentSelectedItem.ItemEntry.blueprint.itemName} to right balance");
            BalanceManager.Instance.AddToRight(_currentSelectedItem.ItemEntry, 1);
            ItemRegistry.Instance.RemoveFromDeskItems(_currentSelectedItem);
            Destroy(_currentSelectedItem.gameObject);
        }

        else
        {
            _currentSelectedItem.ToggleCollider(true);
        }
        
        TooltipManager.Instance.Hide();
        _currentSelectedItem = null;
        CursorManager.Instance.SetCursor("default");
        BalanceManager.Instance.HideArrow();
        if (!InventoryManager.Instance.IsDrawerOpen)
            InventoryManager.Instance.SetDrawerSpriteState(false);
    }
}
