using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PersonRow : MonoBehaviour
{
    public Button rowButton;
    public TMP_Text nameText;
    public TMP_Text subtitleText;
    public TMP_Text tagText;
    public Image tagBg;
    public TMP_Text ratingText;
    public Button actionButton;
    public TMP_Text actionLabel;

    public void Bind(string personName, string subtitle, string tag, Color tagColor, int rating, Action onTap, string actionText, Action onAction, bool actionEnabled)
    {
        nameText.text = personName;
        subtitleText.text = subtitle;
        tagText.text = tag;
        tagBg.color = tagColor;
        ratingText.text = rating.ToString();

        rowButton.onClick.RemoveAllListeners();
        if (onTap != null) rowButton.onClick.AddListener(() => onTap());

        if (string.IsNullOrEmpty(actionText))
        {
            actionButton.gameObject.SetActive(false);
            return;
        }
        actionButton.gameObject.SetActive(true);
        actionLabel.text = actionText;
        actionButton.interactable = actionEnabled;
        actionButton.onClick.RemoveAllListeners();
        if (onAction != null) actionButton.onClick.AddListener(() => onAction());
    }
}
