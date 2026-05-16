using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    public ItemEntry ItemEntry { get; private set; }
    [SerializeField] private Collider2D colliderSelf;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float _initialScale;
    private Tween _scaleTween;
    public bool InDrawer { get; set; }

    private void Awake()
    {
        _initialScale = transform.localScale.x;
    }
    
    public void SetEntry(ItemEntry entry)
    {
        ItemEntry = entry;
        spriteRenderer.sprite = entry.blueprint.itemIcon;
    }
    
    public void SetDrawerSortingLayer()
    {
        spriteRenderer.sortingLayerName = "Drawer";
    }
    
    public void SetDefaultSortingLayer()
    {
        spriteRenderer.sortingLayerName = "Default";
    }

    private void OnMouseOver()
    {
        InteractionManager.Instance.OnItemHover(this);
    }

    private void OnMouseExit()
    {
        InteractionManager.Instance.OnItemUnhover(this);
    }

    private void OnMouseDown()
    {
        InteractionManager.Instance.OnItemSelect(this);
    }
    
    private void OnMouseUp()
    {
        InteractionManager.Instance.DropItem();
    }
    
    public void ScaleUp()
    {
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(_initialScale * 1.1f, 0.1f).SetEase(Ease.OutBack);
    }
    
    public void ScaleDown()
    {
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(_initialScale, 0.1f).SetEase(Ease.OutBack);
    }
    
    public void ToggleCollider(bool state)
    {
        colliderSelf.enabled = state;
    }

    private void OnMouseDrag()
    {
        InteractionManager.Instance.OnItemDrag(this);
    }
}
