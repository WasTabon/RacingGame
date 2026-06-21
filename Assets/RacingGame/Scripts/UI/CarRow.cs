using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CarRow : MonoBehaviour
{
    public TMP_Text nameText;
    public Image perfFill;
    public TMP_Text perfText;

    public void Bind(CarData c)
    {
        nameText.text = c.carName;
        perfText.text = Mathf.RoundToInt(c.OverallPerformance).ToString();
        perfFill.fillAmount = 0f;
        perfFill.DOFillAmount(c.OverallPerformance / 100f, 0.7f).SetEase(Ease.OutQuad);
    }
}
