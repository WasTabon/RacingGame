using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;

    private bool hapticsOn = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        hapticsOn = PlayerPrefs.GetInt("hapticsOn", 1) == 1;
    }

    public void Light()
    {
        if (!hapticsOn) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void Medium()
    {
        if (!hapticsOn) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void Success()
    {
        if (!hapticsOn) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void SetEnabled(bool e)
    {
        hapticsOn = e;
        PlayerPrefs.SetInt("hapticsOn", e ? 1 : 0);
    }

    public bool IsEnabled() { return hapticsOn; }
}
