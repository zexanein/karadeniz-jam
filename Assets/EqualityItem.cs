using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EqualityItem : MonoBehaviour
{
    public List<EqualityItemEntry> leftEntries;
    public List<EqualityItemEntry> rightEntries;
    public TMP_Text text;

    public void SetEntries(List<ItemEntry> noteLeftEntries, List<ItemEntry> noteRightEntries, ComparisonType comparison)
    {
        foreach (var leftEntry in leftEntries)
        {
            leftEntry.gameObject.SetActive(false);
        }
    
        foreach (var rightEntry in rightEntries)
        {
            rightEntry.gameObject.SetActive(false);
        }
    
        for (int i = 0; i < noteLeftEntries.Count; i++)
        {
            var entry = noteLeftEntries[i];
            if (i >= leftEntries.Count) break;
            var uiEntry = leftEntries[i];
            uiEntry.gameObject.SetActive(true);
            uiEntry.amountText.text = entry.quantity.ToString();
            uiEntry.iconImage.sprite = entry.blueprint.itemIcon;
        }
    
        for (int i = 0; i < noteRightEntries.Count; i++)
        {
            var entry = noteRightEntries[i];
            if (i >= rightEntries.Count) break;
            var uiEntry = rightEntries[i];
            uiEntry.gameObject.SetActive(true);
            uiEntry.amountText.text = entry.quantity.ToString();
            uiEntry.iconImage.sprite = entry.blueprint.itemIcon;
        }

        text.text = comparison switch
        {
            ComparisonType.Equal => "=",
            ComparisonType.Greater => ">",
            ComparisonType.Less => "<",
            _ => "="
        };
    }

    [Serializable]
    public class EqualityItemEntry
    {
        public GameObject gameObject;
        public TMP_Text amountText;
        public Image iconImage;
    }
}