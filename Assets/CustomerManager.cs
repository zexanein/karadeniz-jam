using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public Sprite[] customerSprites;
    
    private CustomerComponent[] _slots; // slot index -> o slottaki müşteri (boşsa null)
    
    public CustomerData ActiveCustomerData { get; private set; }

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
        customerComponent.SetCustomer(data, customerSprites.Length > 0 ? customerSprites[Random.Range(0, customerSprites.Length)] : null);

        _slots[targetSlot] = customerComponent;
        MoveCustomerToSlot(customerComponent, targetSlot);
    }

    public void AcceptCustomer(CustomerData customerData)
    {
        if (customerData == null) return;

        if (customerData.isSeller)
        {
             var spend = EconomyManager.Instance.TrySpendMoney(customerData.offeredItemUnitCost * customerData.offeredItem.quantity);
             if (!spend) return;
            InventoryManager.Instance.AddItem(customerData.offeredItem.blueprint, customerData.offeredItem.quantity);
            InventoryManager.Instance.AddToDrawer(customerData.offeredItem.blueprint, customerData.offeredItem.quantity);
             
        }

        else
        {
            ActiveCustomerData = customerData;
            BalanceManager.Instance.UpdateGiveButtons();
        }
        
    }
    
    public void GiveItemToCustomer(int count)
    {
        if (FrontCustomer == null || ActiveCustomerData == null) return;

        var givenGrams = count * ActiveCustomerData.requestedItem.blueprint.netWeightGrams;
        var requestedGrams = ActiveCustomerData.requestedItem.blueprint.netWeightGrams * ActiveCustomerData.requestedItem.quantity;
        
        if (givenGrams < requestedGrams)
        {
            FrontCustomer.ShowGoodbye("Sadece bu kadar mı? Kazıklanmış gibi hissediyorum ama neyse...", "Peki, görüşürüz");
            ReputationBar.Instance.AddReputation(-ReputationBar.Instance.GetReputation());
            TrustBar.Instance.AddTrust(3);
        }
        
        else if (givenGrams > requestedGrams)
        {
            FrontCustomer.ShowGoodbye("Bu kadar fazla olmasını beklemiyordum, teşekkürler!", "Ben de beklemiyordum. Görüşürüz");
            ReputationBar.Instance.AddReputation(ReputationBar.Instance.GetReputation());
            TrustBar.Instance.AddTrust(-3);
        }

        else
        {
            FrontCustomer.ShowGoodbye("Tam istediğim gibi, teşekkürler!", "Görüşürüz!");
            ReputationBar.Instance.AddReputation(0);
            TrustBar.Instance.AddTrust(0);
        }

        InventoryManager.Instance.RemoveItem(ActiveCustomerData.requestedItem.blueprint.id, ActiveCustomerData.requestedItem.quantity);
        ActiveCustomerData = null;
    }
    
    public void SkipTrade()
    {
        FrontCustomer.ToggleCanvas(false);
        FrontCustomer.SetIsActive(false);
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
    
    //+90 = 1g
    //+78 = 10g
    //+67 = 100g
    //+55 = 500g
    //+40 = 1kg
    //+20 = 5kg

    private CustomerData GenerateRandomCustomer()
    {
        var isSeller = Random.value < 0.5f;
        var isKiloSeller = isSeller && Random.value < 0.75f;
        
        var itemEntry = GetSellerItem(isKiloSeller);
        
        CustomerData customer;
        var multiplier = Random.Range(0.8f, 1.2f);
        
        if (isSeller)
        {
            customer = new CustomerData
            {
                isSeller = true,
                offeredItem = itemEntry,
                offeredItemUnitCost = Mathf.RoundToInt(itemEntry.blueprint.fairPrice * multiplier)
            };
        }
        
        else
        {
            customer = new CustomerData
            {
                isSeller = false,
                requestedItem = itemEntry,
                offeredMoney = Mathf.RoundToInt(itemEntry.blueprint.fairPrice * itemEntry.quantity * multiplier)
            };
        }
        
        return customer;
    }
    
    private string[] kiloSellerItems = { "five_kg", "one_kg", "half_kg", "hundred_gram", "ten_gram", "one_gram" };

    private ItemEntry GetSellerItem(bool isKiloSeller)
    {
        ItemBlueprint item = null; 
        
        while (item == null)
        {
            item = ItemRegistry.Instance.GetRandomBlueprint();
            
            if (isKiloSeller && !kiloSellerItems.Contains(item.id))
                item = null;
            
            else if (!isKiloSeller && kiloSellerItems.Contains(item.id))
                item = null;
        }

        var amount = Random.Range((int)item.randomRange.x, (int)item.randomRange.y + 1);
        
        return new ItemEntry { blueprint = item, quantity = amount };
    }

    public List<Entry> kiloEntries = new List<Entry>();

    [Serializable]
    public class Entry
    {
        public int minTr;
        public ItemBlueprint blueprint;
    }
}

[Serializable]
public class CustomerData
{
    public Sprite customerSprite;
    public ItemEntry requestedItem;
    public int offeredMoney;
    
    public bool isSeller;
    public ItemEntry offeredItem;
    public int offeredItemUnitCost;
    
    public float TotalWeight => requestedItem != null ? requestedItem.blueprint.netWeightGrams * requestedItem.quantity : 0f;
}