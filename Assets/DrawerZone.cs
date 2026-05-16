using System;
using UnityEngine;

public class DrawerZone : MonoBehaviour
{
    private void OnMouseEnter()
    {
        InteractionManager.Instance.HoveringDrawerZone = true;
    }
    
    private void OnMouseExit()
    {
        InteractionManager.Instance.HoveringDrawerZone = false;
    }
}
