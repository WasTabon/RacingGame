using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubNavButton : MonoBehaviour
{
    public Image icon;
    public TMP_Text label;
    public Image indicator;
    public string iconName;

    private static readonly Color Active = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color Inactive = new Color(0.5f, 0.5f, 0.58f);

    private void Awake()
    {
        icon.sprite = IconFactory.Get(iconName, Color.white);
        icon.preserveAspect = true;
    }

    public void SetSelected(bool selected)
    {
        icon.color = selected ? Active : Inactive;
        label.color = selected ? Active : Inactive;
        if (indicator != null) indicator.enabled = selected;
    }
}
