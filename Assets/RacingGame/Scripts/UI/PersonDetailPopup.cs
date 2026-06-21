using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PersonDetailPopup : PopupBase
{
    public Button closeButton;
    public TMP_Text nameText;
    public TMP_Text infoText;
    public TMP_Text ratingText;
    public TMP_Text[] statLabels;
    public Image[] statFills;
    public TMP_Text[] statValues;
    public Button[] actionButtons;
    public TMP_Text[] actionLabels;

    private bool wired;

    private void Awake()
    {
        Wire();
    }

    private void Wire()
    {
        if (wired) return;
        wired = true;
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    public void SetPerson(string personName, string info, int rating, string[] labels, int[] vals)
    {
        Wire();
        nameText.text = personName;
        infoText.text = info;
        ratingText.text = rating.ToString();

        for (int i = 0; i < statLabels.Length; i++)
        {
            bool on = i < labels.Length;
            statLabels[i].transform.parent.gameObject.SetActive(on);
            if (on)
            {
                statLabels[i].text = labels[i];
                statValues[i].text = vals[i].ToString();
                statFills[i].fillAmount = Mathf.Clamp01(vals[i] / 100f);
            }
        }
    }

    public void ConfigureActions(string[] labels, Action[] calls, bool[] enabled)
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            bool on = labels != null && i < labels.Length;
            actionButtons[i].gameObject.SetActive(on);
            if (!on) continue;
            actionLabels[i].text = labels[i];
            actionButtons[i].interactable = enabled[i];
            actionButtons[i].onClick.RemoveAllListeners();
            Action call = calls[i];
            if (call != null) actionButtons[i].onClick.AddListener(() => call());
        }
    }
}
