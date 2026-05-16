using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe")]
public class RecipeBlueprint : ScriptableObject
{
    public RecipeEntry[] ingredients;
}

[Serializable]
public class RecipeEntry
{
    public ItemBlueprint blueprint;
    public int requiredWeight;
}
