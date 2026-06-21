using UnityEngine;
using TMPro;
using DG.Tweening;

public class ResourceCounter : MonoBehaviour
{
    public TMP_Text label;
    public bool moneyFormat;

    private long current;
    private Tween tween;

    public void SetImmediate(long value)
    {
        if (tween != null) tween.Kill();
        current = value;
        Render(value);
    }

    public void AnimateTo(long value, float duration = 0.9f)
    {
        if (tween != null) tween.Kill();
        long start = current;
        tween = DOVirtual.Float(0f, 1f, duration, t =>
        {
            double v = start + (value - start) * (double)t;
            long lv = (long)System.Math.Round(v);
            current = lv;
            Render(lv);
        }).SetEase(Ease.OutQuad);
    }

    private void Render(long v)
    {
        label.text = moneyFormat ? FormatMoney(v) : v.ToString();
    }

    public static string FormatMoney(long v)
    {
        if (v >= 1000000) return "$" + (v / 1000000f).ToString("0.0") + "M";
        if (v >= 1000) return "$" + (v / 1000f).ToString("0.0") + "K";
        return "$" + v.ToString();
    }
}
