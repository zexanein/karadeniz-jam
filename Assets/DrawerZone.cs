using UnityEngine;

public class DrawerZone : MonoBehaviour
{
    [SerializeField] private Collider2D zoneCollider;
    [SerializeField] private Collider2D areaCollider;

    private bool _isHovering;

    private void Reset()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (areaCollider == null || Camera.main == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool inside = areaCollider.OverlapPoint(worldPos);

        if (inside != _isHovering)
        {
            _isHovering = inside;
            InteractionManager.Instance.HoveringDrawerZone = _isHovering;
        }
    }

    private void OnDisable()
    {
        // Component kapatılırsa state'i temizle, asılı kalmasın
        if (_isHovering)
        {
            _isHovering = false;
            if (InteractionManager.Instance != null)
                InteractionManager.Instance.HoveringDrawerZone = false;
        }
    }
}