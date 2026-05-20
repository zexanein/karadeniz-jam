
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

        Vector2 hotspot = entry.centered
            ? new Vector2(entry.texture.width * 0.5f, entry.texture.height * 0.5f)
            : entry.pivot; // pixel cinsinden offset, sol-üst origin

        Cursor.SetCursor(entry.texture, hotspot, CursorMode.Auto);
        _currentEntry = entry;
    }

    [Serializable]
    public class CursorEntry
    {
        public string id;
        public Texture2D texture;
        public bool centered = true;     // merkez tıklamalı cursorlar için
        public Vector2 pivot = Vector2.zero; // sol-üst tıklamalı için (0,0)
    }
}
