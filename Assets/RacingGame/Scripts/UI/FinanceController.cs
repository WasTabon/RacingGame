using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinanceController : MonoBehaviour
{
    public Button backButton;
    public ResourceCounter moneyCounter;

    public StaffSubTab[] subTabs;
    public Button[] subTabClickables;

    public GameObject overviewPanel;
    public TMP_Text overviewText;

    public GameObject sponsorsScroll;
    public RectTransform listContent;
    public GameObject rowTemplate;

    private int currentTab = -1;
    private readonly List<GameObject> spawnedRows = new List<GameObject>();
    private readonly List<SponsorData> offers = new List<SponsorData>();

    private const int SalaryRaces = 22;
    private const long UpkeepPerLevel = 150000L;

    private static readonly Color ActiveTag = new Color(0.30f, 0.65f, 0.40f);
    private static readonly Color OfferTag = new Color(0.30f, 0.50f, 0.75f);
    private static readonly Color LockedTag = new Color(0.45f, 0.45f, 0.52f);

    private static readonly string[] SponsorNames =
    {
        "Apex Fuels", "Veloce Tech", "Nimbus Air", "Strata Bank", "Volt Energy",
        "Orion Tyres", "Quantum Chips", "Meridian Watches", "Helix Pharma", "Zephyr Airlines"
    };

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

        GenerateOffers();
        moneyCounter.SetImmediate(GameManager.Instance.State.PlayerTeam.money);
        SelectTab(0, false);
        TransitionManager.Instance.FadeIn();
    }

    private void GoBack()
    {
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Hub");
    }

    private void GenerateOffers()
    {
        offers.Clear();
        for (int i = 0; i < SponsorNames.Length; i++)
        {
            SponsorData s = new SponsorData();
            s.id = "sponsor_" + i;
            s.sponsorName = SponsorNames[i];
            s.reputationRequired = i * 9;
            s.perRacePayout = 800000 + i * 600000;
            s.signingBonus = s.perRacePayout * 3;
            offers.Add(s);
        }
    }

    private void SelectTab(int index, bool playSound)
    {
        if (currentTab == index) return;
        currentTab = index;
        for (int i = 0; i < subTabs.Length; i++)
            if (subTabs[i] != null) subTabs[i].SetSelected(i == index);
        if (playSound) SoundManager.Instance.PlayClick();

        overviewPanel.SetActive(index == 0);
        sponsorsScroll.SetActive(index == 1);

        if (index == 0) BuildOverview();
        else BuildSponsors();
    }

    private void BuildOverview()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;

        long sponsorIncome = SponsorIncome(team);
        long salaries = PerRaceSalaries(team);
        long upkeep = UpkeepPerRace(team);
        long net = sponsorIncome - salaries - upkeep;

        overviewText.text =
            "Balance:  " + ResourceCounter.FormatMoney(team.money) + "\n" +
            "Reputation:  " + team.reputation + " / 100\n" +
            "Research Points:  " + team.researchPoints + "\n\n" +
            "— Per race —\n" +
            "Sponsors income:  +" + ResourceCounter.FormatMoney(sponsorIncome) + "\n" +
            "Salaries:  -" + ResourceCounter.FormatMoney(salaries) + "\n" +
            "Facilities upkeep:  -" + ResourceCounter.FormatMoney(upkeep) + "\n" +
            "Net (excl. prize):  " + (net >= 0 ? "+" : "-") + ResourceCounter.FormatMoney(Math.Abs(net)) + "\n\n" +
            "Active sponsors:  " + team.sponsors.Count + " / " + SponsorNames.Length;
    }

    private void BuildSponsors()
    {
        ClearRows();
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;

        for (int i = 0; i < team.sponsors.Count; i++)
        {
            SponsorData s = team.sponsors[i];
            string sub = ResourceCounter.FormatMoney(s.perRacePayout) + " / race";
            string sid = s.id;
            NewRow().Bind(s.sponsorName, sub, "ACTIVE", ActiveTag, Mil(s.perRacePayout),
                null, "END DEAL", () => Drop(sid), true);
        }

        for (int i = 0; i < offers.Count; i++)
        {
            SponsorData o = offers[i];
            if (IsSigned(team, o.id)) continue;

            bool unlocked = team.reputation >= o.reputationRequired;
            string sub = ResourceCounter.FormatMoney(o.perRacePayout) + " / race   •   bonus "
                + ResourceCounter.FormatMoney(o.signingBonus)
                + (unlocked ? "" : "   •   rep " + o.reputationRequired);
            string oid = o.id;

            if (unlocked)
                NewRow().Bind(o.sponsorName, sub, "OFFER", OfferTag, Mil(o.perRacePayout),
                    null, "SIGN", () => Sign(oid), team.money >= o.signingBonus);
            else
                NewRow().Bind(o.sponsorName, sub, "LOCKED", LockedTag, Mil(o.perRacePayout),
                    null, null, null, false);
        }
    }

    private void Sign(string offerId)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        SponsorData o = FindOffer(offerId);
        if (o == null || IsSigned(team, offerId)) return;
        if (team.reputation < o.reputationRequired) { SoundManager.Instance.PlayError(); return; }
        if (team.money < o.signingBonus) { SoundManager.Instance.PlayError(); return; }

        team.money -= o.signingBonus;
        moneyCounter.AnimateTo(team.money);

        SponsorData copy = new SponsorData();
        copy.id = o.id;
        copy.sponsorName = o.sponsorName;
        copy.perRacePayout = o.perRacePayout;
        copy.signingBonus = o.signingBonus;
        copy.reputationRequired = o.reputationRequired;
        team.sponsors.Add(copy);

        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        SaveManager.Instance.SaveGame(st);
        BuildSponsors();
    }

    private void Drop(string sponsorId)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        for (int i = team.sponsors.Count - 1; i >= 0; i--)
            if (team.sponsors[i].id == sponsorId) team.sponsors.RemoveAt(i);

        SoundManager.Instance.PlayClick();
        SaveManager.Instance.SaveGame(st);
        BuildSponsors();
    }

    private bool IsSigned(TeamData team, string id)
    {
        for (int i = 0; i < team.sponsors.Count; i++) if (team.sponsors[i].id == id) return true;
        return false;
    }

    private SponsorData FindOffer(string id)
    {
        for (int i = 0; i < offers.Count; i++) if (offers[i].id == id) return offers[i];
        return null;
    }

    private long SponsorIncome(TeamData team)
    {
        long sum = 0;
        for (int i = 0; i < team.sponsors.Count; i++) sum += team.sponsors[i].perRacePayout;
        return sum;
    }

    private long PerRaceSalaries(TeamData team)
    {
        GameState st = GameManager.Instance.State;
        long annual = 0;
        for (int i = 0; i < team.driverIds.Count; i++)
        {
            DriverData d = st.GetDriver(team.driverIds[i]);
            if (d != null) annual += d.salary;
        }
        for (int i = 0; i < team.engineerIds.Count; i++)
        {
            EngineerData e = st.GetEngineer(team.engineerIds[i]);
            if (e != null) annual += e.salary;
        }
        StaffData tp = st.GetStaff(team.teamPrincipalId);
        if (tp != null) annual += tp.salary;
        StaffData td = st.GetStaff(team.technicalDirectorId);
        if (td != null) annual += td.salary;
        return annual / SalaryRaces;
    }

    private long UpkeepPerRace(TeamData team)
    {
        long levels = 0;
        for (int i = 0; i < team.facilities.Count; i++) levels += team.facilities[i].level;
        return levels * UpkeepPerLevel;
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

    private int Mil(int amount) { return Mathf.RoundToInt(amount / 1000000f); }
}
