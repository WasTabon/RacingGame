using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PopupBase : MonoBehaviour
{
    public RectTransform content;
    public Image backdrop;

    private bool isOpen;

    public virtual void Show()
    {
        gameObject.SetActive(true);
        isOpen = true;

        backdrop.DOKill();
        Color c = backdrop.color;
        c.a = 0f;
        backdrop.color = c;
        backdrop.raycastTarget = true;
        backdrop.DOFade(0.6f, 0.25f).SetEase(Ease.OutQuad);

        content.DOKill();
        content.localScale = Vector3.zero;
        content.DOScale(1f, 0.35f).SetEase(Ease.OutBack);

        SoundManager.Instance.PlayPopup();
    }

    public virtual void Hide()
    {
        if (!isOpen) return;
        isOpen = false;

        backdrop.DOKill();
        backdrop.DOFade(0f, 0.2f).SetEase(Ease.InQuad);
        backdrop.raycastTarget = false;

        content.DOKill();
        content.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        SoundManager.Instance.PlayBack();
    }
}
