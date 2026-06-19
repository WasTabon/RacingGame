using UnityEngine.UI;

public class DifficultyPopup : PopupBase
{
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button extremeButton;
    public Button closeButton;

    private bool wired;

    private void OnEnable()
    {
        Wire();
    }

    private void Wire()
    {
        if (wired) return;
        wired = true;
        easyButton.onClick.AddListener(() => Choose(Difficulty.Easy));
        normalButton.onClick.AddListener(() => Choose(Difficulty.Normal));
        hardButton.onClick.AddListener(() => Choose(Difficulty.Hard));
        extremeButton.onClick.AddListener(() => Choose(Difficulty.Extreme));
        closeButton.onClick.AddListener(Hide);
    }

    private void Choose(Difficulty difficulty)
    {
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        GameManager.Instance.StartNewCareer(difficulty);
        TransitionManager.Instance.LoadScene("Hub");
    }
}
