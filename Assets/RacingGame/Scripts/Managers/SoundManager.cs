using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private List<AudioSource> sfxSources = new List<AudioSource>();
    private AudioSource musicSource;
    private int sfxPoolSize = 6;

    private float sfxVolume = 1f;
    private float musicVolume = 0.5f;
    private bool sfxMuted;
    private bool musicMuted;

    private AudioClip clickClip;
    private AudioClip backClip;
    private AudioClip popupClip;
    private AudioClip successClip;
    private AudioClip errorClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject go = new GameObject("SFXSource_" + i);
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxSources.Add(src);
        }

        GameObject musicGo = new GameObject("MusicSource");
        musicGo.transform.SetParent(transform);
        musicSource = musicGo.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        LoadPrefs();
        GenerateClips();
    }

    private void LoadPrefs()
    {
        sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        sfxMuted = PlayerPrefs.GetInt("sfxMuted", 0) == 1;
        musicMuted = PlayerPrefs.GetInt("musicMuted", 0) == 1;
    }

    private void GenerateClips()
    {
        clickClip = CreateTone(660f, 0.06f, 0.3f);
        backClip = CreateTone(330f, 0.08f, 0.3f);
        popupClip = CreateTone(880f, 0.10f, 0.25f);
        successClip = CreateChord(new float[] { 523f, 659f, 784f }, 0.25f, 0.25f);
        errorClip = CreateTone(160f, 0.18f, 0.3f);
    }

    private AudioClip CreateTone(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / duration));
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }
        AudioClip clip = AudioClip.Create("tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateChord(float[] frequencies, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / duration));
            float v = 0f;
            for (int f = 0; f < frequencies.Length; f++)
            {
                v += Mathf.Sin(2f * Mathf.PI * frequencies[f] * t);
            }
            v /= frequencies.Length;
            samples[i] = v * volume * envelope;
        }
        AudioClip clip = AudioClip.Create("chord", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioSource GetFreeSource()
    {
        for (int i = 0; i < sfxSources.Count; i++)
        {
            if (!sfxSources[i].isPlaying) return sfxSources[i];
        }
        return sfxSources[0];
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxMuted) return;
        AudioSource src = GetFreeSource();
        src.clip = clip;
        src.volume = sfxVolume;
        src.Play();
    }

    public void PlayClick() { PlaySfx(clickClip); }
    public void PlayBack() { PlaySfx(backClip); }
    public void PlayPopup() { PlaySfx(popupClip); }
    public void PlaySuccess() { PlaySfx(successClip); }
    public void PlayError() { PlaySfx(errorClip); }

    public void SetSfxVolume(float v)
    {
        sfxVolume = v;
        PlayerPrefs.SetFloat("sfxVolume", v);
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        musicSource.volume = musicMuted ? 0f : v;
        PlayerPrefs.SetFloat("musicVolume", v);
    }

    public void ToggleSfxMute()
    {
        sfxMuted = !sfxMuted;
        PlayerPrefs.SetInt("sfxMuted", sfxMuted ? 1 : 0);
    }

    public void ToggleMusicMute()
    {
        musicMuted = !musicMuted;
        musicSource.volume = musicMuted ? 0f : musicVolume;
        PlayerPrefs.SetInt("musicMuted", musicMuted ? 1 : 0);
    }

    public float GetSfxVolume() { return sfxVolume; }
    public float GetMusicVolume() { return musicVolume; }
    public bool IsSfxMuted() { return sfxMuted; }
    public bool IsMusicMuted() { return musicMuted; }
}
