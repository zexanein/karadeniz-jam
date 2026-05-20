using UnityEngine;

[CreateAssetMenu(fileName = "New Item Blueprint", menuName = "Item Blueprint")]
public class ItemBlueprint : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite itemIcon;
    public int fairPrice;
    public int netWeightGrams;
    public Vector3 panOffset;
    public Vector3 panRotation;
    public float colliderRadius = 0.7f;
    public Vector2 randomRange = Vector2.up;
}