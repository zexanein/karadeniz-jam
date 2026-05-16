
using System;
using NUnit.Framework;using UnityEngine;
using UnityEngine.UI;

public class CursorManager : BehaviourSingleton<CursorManager>
{
    public CursorEntry[] cursorEntries;
    private CursorEntry _currentEntry;

    private void Awake()
    {
        SetCursor("default");
    }

    public void SetCursor(string id)
    {
        var entry = Array.Find(cursorEntries, e => e.id == id);
        if (entry == null)
        {
            Debug.LogWarning($"Cursor with id '{id}' not found.");
            return;
        }
        
        Cursor.SetCursor(entry.texture, entry.pivot, CursorMode.Auto);
        _currentEntry = entry;
    }
}

[Serializable]
public class CursorEntry
{
    public string id;
    public Texture2D texture;
    public Vector2 pivot = new(0.5f, 0.5f);
}
