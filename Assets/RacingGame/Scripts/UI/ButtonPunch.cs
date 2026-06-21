using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonPunch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool playClickSound = true;

    private Vector3 baseScale;
    private bool pressed;
    private Selectable selectable;

    private void Awake()
    {
        baseScale = transform.localScale;
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;
        pressed = true;
        transform.DOKill();
        transform.DOScale(baseScale * 0.95f, 0.08f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!pressed) return;
        pressed = false;
        transform.DOKill();
        transform.DOScale(baseScale, 0.2f).SetEase(Ease.OutBack);

        if (playClickSound) SoundManager.Instance.PlayClick();
        HapticManager.Instance.Light();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pressed) return;
        pressed = false;
        transform.DOKill();
        transform.DOScale(baseScale, 0.2f).SetEase(Ease.OutBack);
    }
}
