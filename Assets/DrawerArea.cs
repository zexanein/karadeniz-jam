using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawerArea : MonoBehaviour
{
    private void OnMouseEnter()
    {
        InteractionManager.Instance.HoveringDrawer = true;
    }
    
    private void OnMouseExit()
    {
        InteractionManager.Instance.HoveringDrawer = false;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        InventoryManager.Instance.OpenDrawer();
    }
}
