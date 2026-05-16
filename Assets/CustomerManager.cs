using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerManager : BehaviourSingleton<CustomerManager>
{
    [Header("Spawn")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Transform customerSpawnpoint;
    [SerializeField] private Transform[] customerTargets; // 0 = en ön, son = en arka
    [SerializeField] private float spawnIntervalMin = 5f;
    [SerializeField] private float spawnIntervalMax = 10f;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f; // birim/saniye
    [SerializeField] private Ease moveEase = Ease.Linear;

    private CustomerComponent[] _slots; // slot index -> o slottaki müşteri (boşsa null)

    public int MaxCustomers => customerTargets != null ? customerTargets.Length : 0;
    public bool IsQueueFull => GetFirstEmptySlotIndex() == -1;
    public CustomerComponent FrontCustomer => _slots != null && _slots.Length > 0 ? _slots[0] : null;

    private void Awake()
    {
        _slots = new CustomerComponent[customerTargets?.Length ?? 0];
    }

    private void Start()
    {
        StartCoroutine(SpawnCustomerCoroutine());
    }

    private IEnumerator SpawnCustomerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            if (!IsQueueFull)
                SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        if (customerPrefab == null || customerSpawnpoint == null) return;

        int targetSlot = GetFirstEmptySlotIndex();
        if (targetSlot == -1) return;

        // Spawnpoint'te oluştur, dünya uzayında hareket etsin diye parent'sız bırakıyoruz
        var customerObject = Instantiate(customerPrefab, customerSpawnpoint.position, customerSpawnpoint.rotation);
        var customerComponent = customerObject.GetComponent<CustomerComponent>();
        if (customerComponent == null)
        {
            Destroy(customerObject);
            return;
        }

        var data = GenerateRandomCustomer();
        customerComponent.SetCustomer(data);

        _slots[targetSlot] = customerComponent;
        MoveCustomerToSlot(customerComponent, targetSlot);
    }

    public void ApplyTrade(CustomerData customerData)
    {
        
    }
    
    public void SkipTrade()
    {
        DequeueFront();
    }

    /// <summary>
    /// Öndeki müşteri servisini aldı ve gidiyor — sıradakileri öne kaydır.
    /// CustomerComponent kendini destroy etmeden önce bunu çağırmalı.
    /// </summary>
    public void DequeueFront()
    {
        if (_slots == null || _slots.Length == 0) return;

        // Önceki müşteriyi sıradan çıkar
        var frontCustomer = _slots[0];
        if (frontCustomer != null)        {
            frontCustomer.transform.DOBlendableMoveBy(new Vector3(10f, 0f, 0f), 3f).SetEase(Ease.Linear).OnComplete(() => Destroy(frontCustomer.gameObject));
            _slots[0] = null;
        }

        for (int i = 1; i < _slots.Length; i++)
        {
            if (_slots[i] == null) continue;

            var customer = _slots[i];
            _slots[i - 1] = customer;
            _slots[i] = null;
            MoveCustomerToSlot(customer, i - 1);
        }
    }
    
    private void MoveCustomerToSlot(CustomerComponent customer, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= customerTargets.Length) return;

        var targetPos = customerTargets[slotIndex].position;
        var startPos = customer.transform.position;
        var distance = Vector3.Distance(startPos, targetPos);

        if (distance < 0.01f)
        {
            customer.transform.position = targetPos;
            if (slotIndex == 0)
                OnCustomerArrived(customer);
            return;
        }

        var duration = distance / moveSpeed;

        customer.transform
            .DOMove(targetPos, duration)
            .SetEase(moveEase)
            .SetLink(customer.gameObject)
            .OnComplete(() =>
            {
                customer.transform.position = targetPos;
                if (slotIndex == 0)
                    OnCustomerArrived(customer);
            });
    }
    
    private void OnCustomerArrived(CustomerComponent customer)
    {
        customer.SetIsActive(true);
    }

    private int GetFirstEmptySlotIndex()
    {
        if (_slots == null) return -1;
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i] == null) return i;
        return -1;
    }

    private CustomerData GenerateRandomCustomer()
    {
        var customerNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve" };
        var item = ItemRegistry.Instance.GetRandomBlueprint();
        var amount = Random.Range(1, 5);
        var requestedItem = new ItemEntry { blueprint = item, quantity = amount };
        var customerName = customerNames[Random.Range(0, customerNames.Length)];
        return new CustomerData { name = customerName, requestedItem =  requestedItem };
    }
}

[Serializable]
public class CustomerData
{
    public string name;
    public Sprite customerSprite;
    public ItemEntry requestedItem;
    public float TotalWeight => requestedItem != null ? requestedItem.blueprint.netWeightGrams * requestedItem.quantity : 0f;
}