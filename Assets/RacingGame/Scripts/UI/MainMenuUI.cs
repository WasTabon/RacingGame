using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button newCareerButton;
    public Button continueButton;
    public Button settingsButton;
    public Button tutorialButton;
    public DifficultyPopup difficultyPopup;
    public SettingsPopup settingsPopup;
    public CanvasGroup continueGroup;

    private void Start()
    {
        bool hasSave = SaveManager.Instance.HasSave();
        continueButton.interactable = hasSave;
        if (continueGroup != null) continueGroup.alpha = hasSave ? 1f : 0.5f;

        newCareerButton.onClick.AddListener(OnNewCareer);
        continueButton.onClick.AddListener(OnContinue);
        settingsButton.onClick.AddListener(OnSettings);
        if (tutorialButton != null) tutorialButton.onClick.AddListener(OnTutorial);

        TransitionManager.Instance.FadeIn();
    }

    private void OnNewCareer()
    {
        difficultyPopup.Show();
    }

    private void OnContinue()
    {
        if (GameManager.Instance.LoadCareer())
            TransitionManager.Instance.LoadScene("Hub");
        else
            SoundManager.Instance.PlayError();
    }

    private void OnSettings()
    {
        settingsPopup.Show();
    }

    private void OnTutorial()
    {
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Tutorial");
    }
}
