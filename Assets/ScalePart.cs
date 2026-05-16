using DG.Tweening;
using UnityEngine;

public class ScalePart : MonoBehaviour
{
    [Header("Hover Swing")]
    [SerializeField] private float swingDuration = 0.8f;
    [SerializeField] private float swingAmplitude = 5f;  // başlangıç açısı (derece)
    [SerializeField] private float swingFrequency = 4f;  // saniyedeki tam salınım sayısı
    [SerializeField] private float swingDamping = 4f;    // ne kadar hızlı sönsün (yüksek = hızlı söner)
    
    public bool isLeft;
    public bool isRight;
    
    private Tween _hoverSwingTween;
    private Quaternion _baseLocalRotation;
    
    private void OnMouseEnter()
    {
        if (isLeft)
            InteractionManager.Instance.HoveringBalanceLeft = true;
        else if (isRight)
            InteractionManager.Instance.HoveringBalanceRight = true;
        
        StartSwing();
    }
    
    private void OnMouseExit()
    {
        if (isLeft)
            InteractionManager.Instance.HoveringBalanceLeft = false;
        else if (isRight)
            InteractionManager.Instance.HoveringBalanceRight = false;
    }private void StartSwing()
    {
        if (_hoverSwingTween != null && _hoverSwingTween.IsActive())
            return;

        var target = transform.parent;
        _baseLocalRotation = target.localRotation;

        // Mouse pozisyonuna göre başlangıç yönü
        // Mouse sağdaysa sola (negatif Z), soldaysa sağa (pozitif Z) sallansın
        float mouseScreenX = Input.mousePosition.x;
        float objectScreenX = Camera.main.WorldToScreenPoint(transform.position).x;
        float direction = mouseScreenX > objectScreenX ? -1f : 1f;

        float t = 0f;
        _hoverSwingTween = DOTween.To(
                () => t,
                v =>
                {
                    t = v;
                    float angle = direction
                                  * swingAmplitude
                                  * Mathf.Sin(t * swingFrequency * Mathf.PI * 2f)
                                  * Mathf.Exp(-swingDamping * t);
                    target.localRotation = _baseLocalRotation * Quaternion.Euler(0f, 0f, angle);
                },
                swingDuration,
                swingDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => target.localRotation = _baseLocalRotation);
    }
}