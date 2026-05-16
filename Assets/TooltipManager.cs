using System;
using UnityEngine;
using TMPro;

public class TooltipManager : BehaviourSingleton<TooltipManager>
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TMP_Text contentText;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(15f, -15f);
    [SerializeField] private bool clampToScreen = true;

    private bool isVisible;

    private void Awake()
    {
        Hide();
    }

    private void LateUpdate()
    {
        if (!isVisible) return;
        FollowMouse();
    }

    public void Show(string content)
    {
        contentText.text = content;
        canvasGroup.alpha = 1f;
        isVisible = true;
        FollowMouse();
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        isVisible = false;
    }

    public void SetContent(string content)
    {
        contentText.text = content;
    }

    private void FollowMouse()
    {
        Vector2 targetPos = (Vector2)Input.mousePosition + offset;

        if (clampToScreen)
            targetPos = ClampToScreen(targetPos);

        tooltipRect.position = targetPos;
    }

    private Vector2 ClampToScreen(Vector2 pos)
    {
        Vector2 size = tooltipRect.rect.size;

        float minX = size.x * tooltipRect.pivot.x;
        float maxX = Screen.width - size.x * (1f - tooltipRect.pivot.x);
        float minY = size.y * tooltipRect.pivot.y;
        float maxY = Screen.height - size.y * (1f - tooltipRect.pivot.y);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }
}