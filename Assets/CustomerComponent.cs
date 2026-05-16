using DG.Tweening;
using TMPro;
using UnityEngine;

public class CustomerComponent : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject answerableButtons;
    [SerializeField] private GameObject unanswerableButtons;
    
    public CustomerData CustomerData { get; private set; }
    
    public void SetCustomer(CustomerData customer)
    {
        CustomerData = customer;
        SetIsActive(false);
    }
    
    public void SetIsActive(bool active)
    {
        spriteRenderer.color = new Color32(100, 100, 100, 255);
        canvas.SetActive(false);

        if (active)
        {
            spriteRenderer.DOColor(Color.white, 0.6f).OnComplete(() => canvas.SetActive(true));
            transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack);
            var hasItem = InventoryManager.Instance.HasItem(CustomerData.requestedItem.blueprint.id);
            answerableButtons.SetActive(hasItem);
            unanswerableButtons.SetActive(!hasItem);
            dialogueText.text = $"Merhabalar. <u><color=#006382>{CustomerData.TotalWeight} gram</color></u> {CustomerData.requestedItem.blueprint.itemName} alabilir miyim?";
        }
    }

    public void Accept()
    {
        CustomerManager.Instance.ApplyTrade(CustomerData);
        answerableButtons.SetActive(false);
    }
    
    public void Decline()
    {
        CustomerManager.Instance.SkipTrade();
        canvas.SetActive(false);
    }
}
