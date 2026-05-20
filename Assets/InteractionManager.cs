using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : BehaviourSingleton<InteractionManager>
{
    private DraggableItem _currentSelectedItem;
    private DraggableItem _currentHoveredItem;
    
    public bool HoveringDrawer { get; set; }
    public bool HoveringBalanceLeft { get; set; }
    public bool HoveringBalanceRight { get; set; }
    public bool HoveringDrawerZone { get; set; }

    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

    private void Update()
    {
        // Drag sırasında hover/select işlemi atlanır (zaten bir şey tutuyoruz)
        if (_currentSelectedItem == null)
        {
            UpdateHover();

            if (Input.GetMouseButtonDown(0) && _currentHoveredItem != null)
                OnItemSelect(_currentHoveredItem);
        }
        else
        {
            OnItemDrag(_currentSelectedItem);

            if (Input.GetMouseButtonUp(0))
                DropItem();
        }
    }

    private void UpdateHover()
    {
        var topItem = GetTopItemUnderMouse();

        if (topItem == _currentHoveredItem) return;

        if (_currentHoveredItem != null)
            OnItemUnhover(_currentHoveredItem);

        _currentHoveredItem = topItem;

        if (_currentHoveredItem != null)
            OnItemHover(_currentHoveredItem);
    }

    private DraggableItem GetTopItemUnderMouse()
    {
        if (Camera.main == null) return null;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = new Vector2(worldPos.x, worldPos.y);

        int hitCount = Physics2D.OverlapPointNonAlloc(point, _colliderBuffer);
        if (hitCount == 0) return null;

        DraggableItem best = null;
        int bestOrder = int.MinValue;
        int bestLayerValue = int.MinValue;

        for (int i = 0; i < hitCount; i++)
        {
            var col = _colliderBuffer[i];
            if (col == null) continue;

            var item = col.GetComponentInParent<DraggableItem>();
            if (item == null || !item.IsInteractable) continue;

            var sr = item.SpriteRenderer;
            if (sr == null) continue;

            int layerValue = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
            int order = sr.sortingOrder;

            // Önce sorting layer, sonra sorting order
            if (layerValue > bestLayerValue ||
                (layerValue == bestLayerValue && order > bestOrder))
            {
                best = item;
                bestLayerValue = layerValue;
                bestOrder = order;
            }
        }

        return best;
    }

    private readonly Collider2D[] _colliderBuffer = new Collider2D[16];

    public void OnItemHover(DraggableItem item)
    {
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

        // Hover state'ini temizle, drop sonrası tutarlı başlasın
        if (_currentHoveredItem == item)
        {
            OnItemUnhover(_currentHoveredItem);
            _currentHoveredItem = null;
        }

        if (!InventoryManager.Instance.IsDrawerOpen)
        {
            //BalanceManager.Instance.ShowArrow();
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
        if (_currentSelectedItem == null) return;

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