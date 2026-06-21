using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DriverCard : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text infoText;
    public TMP_Text overallText;
    public Image[] statFills;
    public TMP_Text[] statValues;

    public void Bind(DriverData d)
    {
        nameText.text = d.FullName;
        infoText.text = "Age " + d.age + "   POT " + d.potential;
        overallText.text = d.OverallSkill.ToString();

        int[] stats = { d.speed, d.qualifying, d.consistency };
        for (int i = 0; i < statFills.Length; i++)
        {
            statValues[i].text = stats[i].ToString();
            statFills[i].fillAmount = 0f;
            statFills[i].DOFillAmount(stats[i] / 100f, 0.6f).SetEase(Ease.OutQuad);
        }
    }
}
