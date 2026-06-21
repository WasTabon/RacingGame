using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaffSubTab : MonoBehaviour
{
    public TMP_Text label;
    public Image indicator;

    private static readonly Color Inactive = new Color(0.6f, 0.6f, 0.66f);

    public void SetSelected(bool selected)
    {
        label.color = selected ? Color.white : Inactive;
        if (indicator != null) indicator.enabled = selected;
    }
}
