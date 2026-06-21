using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimingRow : MonoBehaviour
{
    public Image bg;
    public Image swatch;
    public TMP_Text posText;
    public TMP_Text nameText;
    public TMP_Text gapText;

    public void Set(int position, string driverName, Color teamColor, string gap, bool highlight)
    {
        posText.text = position.ToString();
        nameText.text = driverName;
        swatch.color = teamColor;
        gapText.text = gap;
        bg.color = highlight ? new Color(0.20f, 0.30f, 0.48f) : new Color(0.15f, 0.15f, 0.24f);
    }
}
