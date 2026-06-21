using UnityEngine;
using UnityEngine.UI;

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
