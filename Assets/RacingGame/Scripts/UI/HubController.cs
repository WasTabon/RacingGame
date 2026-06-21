using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HubController : MonoBehaviour
{
    public ResourceCounter moneyCounter;
    public ResourceCounter repCounter;
    public TMP_Text seasonWeekText;
    public Button settingsButton;
    public SettingsPopup settingsPopup;
    public CalendarPopup calendarPopup;

    public TMP_Text teamNameText;
    public TMP_Text philosophyText;

    public DriverCard[] driverCards;
    public CarRow[] carRows;

    public Button nextRaceButton;
    public TMP_Text nextRaceRoundText;
    public TMP_Text nextRaceTrackText;
    public TMP_Text nextRaceInfoText;

    public HubNavButton[] navButtons;
    public Button[] navClickables;
    public CanvasGroup[] tabPanels;

    private int currentTab = -1;

    private static readonly string[] tabScenes = { null, "Staff", "RnD", "Car", "Base" };

    private void Start()
    {
        if (GameManager.Instance.State == null)
            GameManager.Instance.StartNewCareer(Difficulty.Normal);

        WireNav();
        if (settingsButton != null) settingsButton.onClick.AddListener(() => settingsPopup.Show());
        if (nextRaceButton != null) nextRaceButton.onClick.AddListener(() => { SoundManager.Instance.PlayClick(); TransitionManager.Instance.LoadScene("Weekend"); });

        SelectTab(0, false);
        PopulateTeam();

        TransitionManager.Instance.FadeIn();
        DOVirtual.DelayedCall(0.2f, AnimateTopBar);
    }

    private void WireNav()
    {
        for (int i = 0; i < navClickables.Length; i++)
        {
            if (navClickables[i] == null) continue;
            int idx = i;
            navClickables[i].onClick.AddListener(() => OnNavClicked(idx));
        }
    }

    private void OnNavClicked(int idx)
    {
        if (idx >= 0 && idx < tabScenes.Length && !string.IsNullOrEmpty(tabScenes[idx]))
        {
            SoundManager.Instance.PlayClick();
            TransitionManager.Instance.LoadScene(tabScenes[idx]);
            return;
        }
        SelectTab(idx, true);
    }

    private void AnimateTopBar()
    {
        TeamData team = GameManager.Instance.State.PlayerTeam;
        moneyCounter.AnimateTo(team.money);
        repCounter.AnimateTo(team.reputation);
    }

    private void PopulateTeam()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;

        moneyCounter.SetImmediate(0);
        repCounter.SetImmediate(0);
        seasonWeekText.text = "Season " + st.season.year + "    Week " + st.currentWeek;

        teamNameText.text = team.teamName;
        philosophyText.text = team.philosophy.ToString().ToUpper() + " PHILOSOPHY";

        for (int i = 0; i < driverCards.Length; i++)
        {
            if (driverCards[i] == null)
            {
                Debug.LogWarning("HubController: driverCards[" + i + "] is not assigned. Re-run menu 'RacingGame/Iteration 2 - HQ Hub'.");
                continue;
            }
            DriverData d = st.GetDriver(team.raceDriverIds[i]);
            if (d == null)
            {
                Debug.LogWarning("HubController: no driver found for id '" + team.raceDriverIds[i] + "'. The loaded save may be from an older build, start a New Career.");
                continue;
            }
            driverCards[i].Bind(d);
        }

        for (int i = 0; i < carRows.Length; i++)
        {
            if (carRows[i] == null)
            {
                Debug.LogWarning("HubController: carRows[" + i + "] is not assigned. Re-run menu 'RacingGame/Iteration 2 - HQ Hub'.");
                continue;
            }
            if (i >= team.cars.Count)
            {
                Debug.LogWarning("HubController: team has no car at index " + i + ".");
                continue;
            }
            carRows[i].Bind(team.cars[i]);
        }

        RaceData next = st.season.NextRace;
        if (next != null)
        {
            nextRaceRoundText.text = "ROUND " + next.round;
            nextRaceTrackText.text = next.trackName;
            nextRaceInfoText.text = next.country + "    " + next.laps + " laps    Wet " + Mathf.RoundToInt(next.weatherWetChance * 100f) + "%";
        }
    }

    private void SelectTab(int index, bool playSound)
    {
        if (currentTab == index) return;
        currentTab = index;

        for (int i = 0; i < tabPanels.Length; i++)
        {
            bool active = (i == index);
            CanvasGroup cg = tabPanels[i];
            if (cg != null)
            {
                cg.gameObject.SetActive(active);
                if (active)
                {
                    cg.DOKill();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
                }
            }
            if (navButtons[i] != null) navButtons[i].SetSelected(active);
        }

        if (playSound) SoundManager.Instance.PlayClick();
    }
}
