using UnityEngine;

[CreateAssetMenu(fileName = "New Item Blueprint", menuName = "Item Blueprint")]
public class ItemBlueprint : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite itemIcon;
    public int fairPrice;
    public int netWeightGrams;
}