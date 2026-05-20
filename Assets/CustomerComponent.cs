using DG.Tweening;
using TMPro;
using UnityEngine;

public class CustomerComponent : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text refuseText;
    [SerializeField] private GameObject answerableButtons;
    [SerializeField] private GameObject unanswerableButtons;
    [SerializeField] private GameObject goodbyeButton;
    
    public CustomerData CustomerData { get; private set; }
    
    public void SetCustomer(CustomerData customer, Sprite sprite)
    {
        CustomerData = customer;
        spriteRenderer.sprite = sprite;
        SetIsActive(false, true);
    }
    
    public void ToggleCanvas(bool state)
    {
        canvas.SetActive(state);
    }
    
    public void SetIsActive(bool active, bool instant = false)
    {
        canvas.SetActive(false);
        var targetScale = active ? Vector3.one : Vector3.one * .9f;
        var startColor = active ? new Color32(100, 100, 100, 255) : (Color32) Color.white;
        var endColor = active ? (Color32) Color.white : new Color32(100, 100, 100, 255);
        spriteRenderer.color = startColor;
        
        if (instant)
        {
            spriteRenderer.color = endColor;
            transform.localScale = targetScale;
            canvas.SetActive(active);
            return;
        }
        
        var tw =
        spriteRenderer.DOColor(endColor, 0.6f).OnComplete(() => canvas.SetActive(active));
        transform.DOScale(targetScale, 0.6f).SetEase(Ease.OutBack);

        if (active)
        {
            if (CustomerData.isSeller)
            {
                var hasMoney = EconomyManager.Instance.Money >= CustomerData.offeredItemUnitCost * CustomerData.offeredItem.quantity;
                answerableButtons.SetActive(hasMoney);
                unanswerableButtons.SetActive(!hasMoney);
                refuseText.text = "Param yok...";
                var sellPrice = CustomerData.offeredItemUnitCost * CustomerData.offeredItem.quantity;
                dialogueText.text = $"<color=#006382>{sellPrice} altın</color> karşılığında <color=#006382>{CustomerData.offeredItem.quantity} adet {CustomerData.offeredItem.blueprint.itemName} satıyorum.</color> Almak ister misin?"; 
            }

            else
            {
                var hasItem = InventoryManager.Instance.HasItem(CustomerData.requestedItem.blueprint.id);
                answerableButtons.SetActive(hasItem);
                unanswerableButtons.SetActive(!hasItem);
                refuseText.text = "Elimizde yok...";
                dialogueText.text = $"<color=#006382>{CustomerData.TotalWeight} gram</color> {CustomerData.requestedItem.blueprint.itemName} gerekiyor. <color=#006382>{CustomerData.offeredMoney} altın</color> ödeyebilirim. Elinizde var mı?";
            }
            
            tw.onComplete += () =>
            {
                Soundmanager.ManTalkSound = true;
            };
        }
    }
    
    public void SetDescription(string description)
    {
        dialogueText.text = description;
    }

    public void Accept()
    {
        CustomerManager.Instance.AcceptCustomer(CustomerData);
        if (CustomerData.isSeller) ShowGoodbye("Güzel, Görüşmek üzere!");
        else answerableButtons.SetActive(false);
    }
    
    public void Decline()
    {
        ShowGoodbye();
    }
    
    public void ShowGoodbye(string customMessage = null, string buttonText = null)
    {
        answerableButtons.SetActive(false);
        unanswerableButtons.SetActive(false);
        goodbyeButton.SetActive(true);
        var message = customMessage ?? "Peki o zaman. Görüşürüz!";
        var buttonMsg = buttonText ?? "Görüşmek üzere!";
        SetGoodbyeButtonText(buttonMsg);
        SetDescription(message);
    }
    
    public void SetGoodbyeButtonText(string text)
    {
        goodbyeButton.GetComponentInChildren<TMP_Text>().text = text;
    }
    
    public void Goodbye()
    {
        CustomerManager.Instance.SkipTrade();
    }
}
