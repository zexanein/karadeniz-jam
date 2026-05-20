using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : BehaviourSingleton<ItemRegistry>
{
    [SerializeField] private DraggableItem itemPrefab;
    [SerializeField] private List<ItemBlueprint> blueprints;
    [SerializeField] private List<Transform> deskSpawnPoints;
    [SerializeField] private BoxCollider2D drawerSpawnArea;
    
    private List<GameObject> _spawnedDrawerItems = new();
    private List<DraggableItem> _spawnedDeskItems = new();
    
    private int _spawnIndex = 0;
    
    public ItemBlueprint GetRandomBlueprint()
    {
        if (blueprints.Count == 0) return null;
        var randomIndex = UnityEngine.Random.Range(0, blueprints.Count);
        return blueprints[randomIndex];
    }
    
    public List<ItemBlueprint> GetAllBlueprints()
    {
        return blueprints;
    }

    public List<ItemEntry> starterItems = new();

    private void Start()
    {
        foreach (var entry in starterItems)
        {   
            InventoryManager.Instance.AddItem(entry.blueprint, entry.quantity);
            InventoryManager.Instance.AddToDrawer(entry.blueprint, entry.quantity);
        }
    }
    
    public void SpawnItemToDesk(string itemId, int quantity = 1)
    {
        var blueprint = blueprints.Find(b => b.id == itemId);
        if (blueprint == null)
        {
            Debug.LogError($"Item with ID {itemId} not found in registry.");
            return;
        }

        for (var i = 0; i < quantity; i++)
        {
            var spawnPoint = deskSpawnPoints[_spawnIndex];
            var newItem = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
            newItem.SetEntry(new ItemEntry { blueprint = blueprint, quantity = 1 });
            _spawnIndex = (_spawnIndex + 1) % deskSpawnPoints.Count;
            _spawnedDeskItems.Add(newItem);
        }
        
        InventoryManager.Instance.AddItem(blueprint, quantity);
    }

    public void AddToDrawerItems(GameObject item)
    {
        _spawnedDrawerItems.Add(item);
    }
    
    public void RemoveFromDeskItems(DraggableItem item)
    {
        _spawnedDeskItems.Remove(item);
    }
    
    public void RemoveAnyFromDeskItems()
    {
        if (_spawnedDeskItems.Count == 0) return;
        var item = _spawnedDeskItems[0];
        if (item != null)
        {
            Destroy(item.gameObject);
        }
        _spawnedDeskItems.RemoveAt(0);
    }
    
    public void RemoveFromDrawerItems(GameObject item)
    {
        _spawnedDrawerItems.Remove(item);
    }
    
    public void ClearDrawerItems()
    {
        foreach (var item in _spawnedDrawerItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _spawnedDrawerItems.Clear();
    }
    
    public void SpawnItemToDrawer(string itemId, int quantity = 1)
    {
        var blueprint = blueprints.Find(b => b.id == itemId);
        if (blueprint == null)
        {
            return;
        }

        for (var i = 0; i < quantity; i++)
        {
            var randomPosition = new Vector2(
                UnityEngine.Random.Range(drawerSpawnArea.bounds.min.x, drawerSpawnArea.bounds.max.x),
                UnityEngine.Random.Range(drawerSpawnArea.bounds.min.y, drawerSpawnArea.bounds.max.y)
            );
            
            var randomRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));
            
            var newItem = Instantiate(itemPrefab, randomPosition, randomRotation);
            newItem.InDrawer = true;
            newItem.SetDrawerSortingLayer();
            newItem.SetEntry(new ItemEntry { blueprint = blueprint, quantity = 1 });
            _spawnedDrawerItems.Add(newItem.gameObject);
        }
    }
}
