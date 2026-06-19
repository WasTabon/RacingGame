using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : PopupBase
{
    public Slider sfxSlider;
    public Slider musicSlider;
    public Button sfxMuteButton;
    public Button musicMuteButton;
    public Button closeButton;
    public Image sfxMuteIcon;
    public Image musicMuteIcon;

    private bool wired;

    private void OnEnable()
    {
        Wire();
        RefreshValues();
    }

    private void Wire()
    {
        if (wired) return;
        wired = true;
        sfxSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetSfxVolume(v));
        musicSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetMusicVolume(v));
        sfxMuteButton.onClick.AddListener(OnSfxMute);
        musicMuteButton.onClick.AddListener(OnMusicMute);
        closeButton.onClick.AddListener(Hide);
    }

    private void RefreshValues()
    {
        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.GetSfxVolume());
        musicSlider.SetValueWithoutNotify(SoundManager.Instance.GetMusicVolume());
        UpdateMuteIcons();
    }

    private void OnSfxMute()
    {
        SoundManager.Instance.ToggleSfxMute();
        UpdateMuteIcons();
    }

    private void OnMusicMute()
    {
        SoundManager.Instance.ToggleMusicMute();
        UpdateMuteIcons();
    }

    private void UpdateMuteIcons()
    {
        Color on = new Color(0.29f, 0.56f, 0.89f);
        Color off = new Color(0.55f, 0.55f, 0.6f);

        bool sfxM = SoundManager.Instance.IsSfxMuted();
        sfxMuteIcon.sprite = IconFactory.Get(sfxM ? "mute" : "sfx", Color.white);
        sfxMuteIcon.color = sfxM ? off : on;
        sfxMuteIcon.preserveAspect = true;

        bool musM = SoundManager.Instance.IsMusicMuted();
        musicMuteIcon.sprite = IconFactory.Get(musM ? "mute" : "music", Color.white);
        musicMuteIcon.color = musM ? off : on;
        musicMuteIcon.preserveAspect = true;
    }
}
