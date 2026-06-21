using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StandingsController : MonoBehaviour
{
    public Button backButton;
    public TMP_Text seasonText;

    public StaffSubTab[] subTabs;
    public Button[] subTabClickables;

    public RectTransform listContent;
    public GameObject rowTemplate;
    public TMP_Text emptyLabel;

    private int currentTab = -1;
    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private static readonly Color Accent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color Primary = new Color(0.30f, 0.50f, 0.75f);

    private void Start()
    {
        if (GameManager.Instance.State == null)
            GameManager.Instance.StartNewCareer(Difficulty.Normal);

        if (backButton != null) backButton.onClick.AddListener(GoBack);

        for (int i = 0; i < subTabClickables.Length; i++)
        {
            if (subTabClickables[i] == null) continue;
            int idx = i;
            subTabClickables[i].onClick.AddListener(() => SelectTab(idx, true));
        }

        SeasonData season = GameManager.Instance.State.season;
        seasonText.text = "Season " + season.year + "   •   Round " + season.currentRound;

        SelectTab(0, false);
        TransitionManager.Instance.FadeIn();
    }

    private void GoBack()
    {
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Hub");
    }

    private void SelectTab(int index, bool playSound)
    {
        if (currentTab == index) return;
        currentTab = index;
        for (int i = 0; i < subTabs.Length; i++)
            if (subTabs[i] != null) subTabs[i].SetSelected(i == index);
        if (playSound) SoundManager.Instance.PlayClick();

        if (index == 0) BuildDrivers();
        else BuildTeams();
    }

    private void BuildDrivers()
    {
        ClearRows();
        GameState st = GameManager.Instance.State;
        List<StandingEntry> list = new List<StandingEntry>(st.season.driverStandings);
        list.Sort((a, b) => b.points.CompareTo(a.points));

        for (int i = 0; i < list.Count; i++)
        {
            DriverData d = st.GetDriver(list[i].id);
            if (d == null) continue;
            bool isPlayer = d.teamId == st.playerTeamId;
            string sub = TeamName(d.teamId) + (list[i].wins > 0 ? "   •   " + list[i].wins + " wins" : "");
            int rank = i + 1;
            NewRow().Bind(d.FullName, sub, "P" + rank, TagColor(rank, isPlayer), list[i].points, null, null, null, false);
        }

        emptyLabel.gameObject.SetActive(list.Count == 0);
        emptyLabel.text = "No standings";
    }

    private void BuildTeams()
    {
        ClearRows();
        GameState st = GameManager.Instance.State;
        List<StandingEntry> list = new List<StandingEntry>(st.season.constructorStandings);
        list.Sort((a, b) => b.points.CompareTo(a.points));

        for (int i = 0; i < list.Count; i++)
        {
            bool isPlayer = list[i].id == st.playerTeamId;
            string sub = list[i].wins + (list[i].wins == 1 ? " win" : " wins");
            int rank = i + 1;
            NewRow().Bind(TeamName(list[i].id), sub, "P" + rank, TagColor(rank, isPlayer), list[i].points, null, null, null, false);
        }

        emptyLabel.gameObject.SetActive(list.Count == 0);
        emptyLabel.text = "No standings";
    }

    private Color TagColor(int rank, bool isPlayer)
    {
        if (isPlayer) return Accent;
        if (rank == 1) return new Color(0.95f, 0.78f, 0.25f);
        if (rank == 2) return new Color(0.70f, 0.74f, 0.80f);
        if (rank == 3) return new Color(0.80f, 0.52f, 0.30f);
        return Primary;
    }

    private string TeamName(string teamId)
    {
        GameState st = GameManager.Instance.State;
        for (int i = 0; i < st.teams.Count; i++) if (st.teams[i].id == teamId) return st.teams[i].teamName;
        return teamId;
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++) Destroy(spawnedRows[i]);
        spawnedRows.Clear();
    }

    private PersonRow NewRow()
    {
        GameObject go = Instantiate(rowTemplate, listContent);
        go.SetActive(true);
        spawnedRows.Add(go);
        return go.GetComponent<PersonRow>();
    }
}
