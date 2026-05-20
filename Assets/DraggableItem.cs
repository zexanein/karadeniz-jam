using DG.Tweening;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    public ItemEntry ItemEntry { get; private set; }
    [SerializeField] private CircleCollider2D colliderSelf;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsInteractable => colliderSelf != null && colliderSelf.enabled;
    
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
        colliderSelf.radius = entry.blueprint.colliderRadius;
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
}