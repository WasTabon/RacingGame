using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    private Image fadeImage;
    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        GameObject canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject imgGo = new GameObject("FadeImage");
        imgGo.transform.SetParent(canvasGo.transform, false);
        fadeImage = imgGo.AddComponent<Image>();
        fadeImage.color = new Color(0.04f, 0.04f, 0.10f, 1f);
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
        fadeImage.raycastTarget = a > 0.01f;
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        fadeImage.raycastTarget = true;
        fadeImage.DOKill();
        SetAlpha(0f);
        fadeImage.DOFade(1f, 0.35f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
            DOVirtual.DelayedCall(0.05f, () =>
            {
                fadeImage.DOFade(0f, 0.35f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    fadeImage.raycastTarget = false;
                    isTransitioning = false;
                });
            });
        });
    }

    public void FadeIn(Action onComplete = null)
    {
        fadeImage.DOKill();
        SetAlpha(1f);
        fadeImage.DOFade(0f, 0.35f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            fadeImage.raycastTarget = false;
            if (onComplete != null) onComplete.Invoke();
        });
    }
}
