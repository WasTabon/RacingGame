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

[RequireComponent(typeof(Image))]
public class IconImage : MonoBehaviour
{
    public string iconName = "settings";
    public Color iconColor = Color.white;

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        Image img = GetComponent<Image>();
        img.sprite = IconFactory.Get(iconName, Color.white);
        img.color = iconColor;
        img.preserveAspect = true;
    }
}

public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rect;
    private Rect lastSafeArea;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea) Apply();
    }

    private void Apply()
    {
        Rect safe = Screen.safeArea;
        lastSafeArea = safe;

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
