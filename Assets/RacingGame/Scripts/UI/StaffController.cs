using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaffController : MonoBehaviour
{
    public Button backButton;
    public ResourceCounter moneyCounter;

    public StaffSubTab[] subTabs;
    public Button[] subTabClickables;

    public RectTransform listContent;
    public GameObject rowTemplate;
    public TMP_Text emptyLabel;

    public PersonDetailPopup detailPopup;

    private int currentTab = -1;
    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private static readonly Color RaceTag = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ReserveTag = new Color(0.45f, 0.45f, 0.52f);
    private static readonly Color TypeTag = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color AcademyTag = new Color(0.40f, 0.75f, 0.45f);

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

        moneyCounter.SetImmediate(GameManager.Instance.State.PlayerTeam.money);
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
        BuildList();
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
            Destroy(spawnedRows[i]);
        spawnedRows.Clear();
    }

    private PersonRow NewRow()
    {
        GameObject go = Instantiate(rowTemplate, listContent);
        go.SetActive(true);
        spawnedRows.Add(go);
        return go.GetComponent<PersonRow>();
    }

    private void BuildList()
    {
        ClearRows();
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;

        if (currentTab == 0) BuildDrivers(st, team);
        else if (currentTab == 1) BuildEngineers(st, team);
        else if (currentTab == 2) BuildLeaders(st, team);
        else if (currentTab == 3) BuildMarket(st, team);
        else BuildAcademy(st, team);
    }

    private bool IsRaceDriver(TeamData team, string id)
    {
        return team.raceDriverIds[0] == id || team.raceDriverIds[1] == id;
    }

    private void BuildDrivers(GameState st, TeamData team)
    {
        int shown = 0;
        for (int i = 0; i < team.driverIds.Count; i++)
        {
            DriverData d = st.GetDriver(team.driverIds[i]);
            if (d == null) continue;
            shown++;
            bool race = IsRaceDriver(team, d.id);
            string did = d.id;
            NewRow().Bind(d.FullName, "Age " + d.age + "    " + SalaryStr(d.salary) + "/yr",
                race ? "RACE" : "RESERVE", race ? RaceTag : ReserveTag, d.OverallSkill,
                () => OpenDriver(did), null, null, false);
        }
        SetEmpty(shown == 0, "No drivers");
    }

    private void BuildEngineers(GameState st, TeamData team)
    {
        int shown = 0;
        for (int i = 0; i < team.engineerIds.Count; i++)
        {
            EngineerData e = st.GetEngineer(team.engineerIds[i]);
            if (e == null) continue;
            shown++;
            string eid = e.id;
            NewRow().Bind(e.FullName, e.specialization + "    " + SalaryStr(e.salary) + "/yr",
                "ENG", TypeTag, e.skill, () => OpenEngineer(eid), null, null, false);
        }
        SetEmpty(shown == 0, "No engineers");
    }

    private void BuildLeaders(GameState st, TeamData team)
    {
        int shown = 0;
        StaffData tp = st.GetStaff(team.teamPrincipalId);
        if (tp != null)
        {
            shown++;
            string id = tp.id;
            NewRow().Bind(tp.FullName, "Team Principal    " + SalaryStr(tp.salary) + "/yr",
                "TP", TypeTag, tp.skill, () => OpenStaff(id), null, null, false);
        }
        StaffData td = st.GetStaff(team.technicalDirectorId);
        if (td != null)
        {
            shown++;
            string id = td.id;
            NewRow().Bind(td.FullName, "Technical Director    " + SalaryStr(td.salary) + "/yr",
                "TD", TypeTag, td.skill, () => OpenStaff(id), null, null, false);
        }
        SetEmpty(shown == 0, "No leaders");
    }

    private void BuildMarket(GameState st, TeamData team)
    {
        int shown = 0;
        long money = team.money;

        for (int i = 0; i < st.driverPool.Count; i++)
        {
            DriverData d = st.driverPool[i];
            if (d.teamId != "" || d.isAcademy) continue;
            shown++;
            string did = d.id;
            long cost = d.salary;
            NewRow().Bind(d.FullName, "Driver    Age " + d.age + "    " + SalaryStr(d.salary) + "/yr",
                "DRIVER", TypeTag, d.OverallSkill, () => OpenDriver(did),
                "Hire " + SalaryStr(cost), () => HireDriver(did), money >= cost);
        }

        for (int i = 0; i < st.engineerPool.Count; i++)
        {
            EngineerData e = st.engineerPool[i];
            if (e.teamId != "") continue;
            shown++;
            string eid = e.id;
            long cost = e.salary;
            NewRow().Bind(e.FullName, "Engineer " + e.specialization + "    " + SalaryStr(e.salary) + "/yr",
                "ENG", TypeTag, e.skill, () => OpenEngineer(eid),
                "Hire " + SalaryStr(cost), () => HireEngineer(eid), money >= cost);
        }

        for (int i = 0; i < st.staffPool.Count; i++)
        {
            StaffData s = st.staffPool[i];
            if (s.teamId != "") continue;
            shown++;
            string sid = s.id;
            long cost = s.salary;
            string roleStr = s.role == StaffRole.TeamPrincipal ? "Principal" : "Tech Director";
            NewRow().Bind(s.FullName, roleStr + "    " + SalaryStr(s.salary) + "/yr",
                "STAFF", TypeTag, s.skill, () => OpenStaff(sid),
                "Hire " + SalaryStr(cost), () => HireStaff(sid), money >= cost);
        }

        SetEmpty(shown == 0, "Market is empty");
    }

    private void BuildAcademy(GameState st, TeamData team)
    {
        int shown = 0;
        long money = team.money;
        for (int i = 0; i < st.driverPool.Count; i++)
        {
            DriverData d = st.driverPool[i];
            if (!d.isAcademy || d.teamId != "") continue;
            shown++;
            string did = d.id;
            long cost = d.salary / 2;
            NewRow().Bind(d.FullName, "Age " + d.age + "    POT " + d.potential + "    " + SalaryStr(d.salary) + "/yr",
                "PROSPECT", AcademyTag, d.OverallSkill, () => OpenDriver(did),
                "Sign " + SalaryStr(cost), () => SignAcademy(did), money >= cost);
        }
        SetEmpty(shown == 0, "No prospects");
    }

    private void SetEmpty(bool empty, string text)
    {
        emptyLabel.gameObject.SetActive(empty);
        emptyLabel.text = text;
    }

    private void OpenDriver(string id)
    {
        GameState st = GameManager.Instance.State;
        DriverData d = st.GetDriver(id);
        if (d == null) return;
        TeamData team = st.PlayerTeam;

        string[] labels = { "SPEED", "QUALIFYING", "CONSISTENCY", "TIRE MGMT", "FEEDBACK", "AGGRESSION", "WET", "POTENTIAL" };
        int[] vals = { d.speed, d.qualifying, d.consistency, d.tireManagement, d.feedback, d.aggression, d.wetSkill, d.potential };
        string info = "Age " + d.age + "    " + SalaryStr(d.salary) + "/yr    " + d.contractYears + "y contract";

        detailPopup.Show();
        detailPopup.SetPerson(d.FullName, info, d.OverallSkill, labels, vals);

        List<string> aLabels = new List<string>();
        List<Action> aCalls = new List<Action>();
        List<bool> aEnab = new List<bool>();

        if (d.teamId == team.id)
        {
            bool isR0 = team.raceDriverIds[0] == d.id;
            bool isR1 = team.raceDriverIds[1] == d.id;
            aLabels.Add("Race Seat 1"); aCalls.Add(() => AssignSeat(id, 0)); aEnab.Add(!isR0);
            aLabels.Add("Race Seat 2"); aCalls.Add(() => AssignSeat(id, 1)); aEnab.Add(!isR1);
            aLabels.Add("Release"); aCalls.Add(() => ReleaseDriver(id)); aEnab.Add(!(isR0 || isR1));
        }
        else
        {
            bool academy = d.isAcademy;
            long cost = academy ? d.salary / 2 : d.salary;
            aLabels.Add((academy ? "Sign " : "Hire ") + SalaryStr(cost));
            if (academy) aCalls.Add(() => SignAcademy(id)); else aCalls.Add(() => HireDriver(id));
            aEnab.Add(team.money >= cost);
        }

        detailPopup.ConfigureActions(aLabels.ToArray(), aCalls.ToArray(), aEnab.ToArray());
    }

    private void OpenEngineer(string id)
    {
        GameState st = GameManager.Instance.State;
        EngineerData e = st.GetEngineer(id);
        if (e == null) return;
        TeamData team = st.PlayerTeam;

        string[] labels = { "SKILL", "CREATIVITY", "EXPERIENCE" };
        int[] vals = { e.skill, e.creativity, e.experience };
        string info = e.specialization + "    " + SalaryStr(e.salary) + "/yr    " + e.contractYears + "y";

        detailPopup.Show();
        detailPopup.SetPerson(e.FullName, info, e.skill, labels, vals);

        List<string> aLabels = new List<string>();
        List<Action> aCalls = new List<Action>();
        List<bool> aEnab = new List<bool>();

        if (e.teamId == team.id)
        {
            aLabels.Add("Release"); aCalls.Add(() => ReleaseEngineer(id)); aEnab.Add(true);
        }
        else
        {
            long cost = e.salary;
            aLabels.Add("Hire " + SalaryStr(cost)); aCalls.Add(() => HireEngineer(id)); aEnab.Add(team.money >= cost);
        }

        detailPopup.ConfigureActions(aLabels.ToArray(), aCalls.ToArray(), aEnab.ToArray());
    }

    private void OpenStaff(string id)
    {
        GameState st = GameManager.Instance.State;
        StaffData s = st.GetStaff(id);
        if (s == null) return;
        TeamData team = st.PlayerTeam;

        string[] labels = { "SKILL", "EXPERIENCE" };
        int[] vals = { s.skill, s.experience };
        string role = s.role == StaffRole.TeamPrincipal ? "Team Principal" : "Technical Director";
        string info = role + "    " + SalaryStr(s.salary) + "/yr    " + s.contractYears + "y";

        detailPopup.Show();
        detailPopup.SetPerson(s.FullName, info, s.skill, labels, vals);

        List<string> aLabels = new List<string>();
        List<Action> aCalls = new List<Action>();
        List<bool> aEnab = new List<bool>();

        bool onTeam = s.id == team.teamPrincipalId || s.id == team.technicalDirectorId;
        if (!onTeam)
        {
            long cost = s.salary;
            aLabels.Add("Hire " + SalaryStr(cost)); aCalls.Add(() => HireStaff(id)); aEnab.Add(team.money >= cost);
        }

        detailPopup.ConfigureActions(aLabels.ToArray(), aCalls.ToArray(), aEnab.ToArray());
    }

    private void Pay(long amount)
    {
        TeamData team = GameManager.Instance.State.PlayerTeam;
        team.money -= amount;
        moneyCounter.AnimateTo(team.money);
    }

    private void AfterChange()
    {
        SaveManager.Instance.SaveGame(GameManager.Instance.State);
        BuildList();
    }

    private void CloseDetailIfOpen()
    {
        if (detailPopup.gameObject.activeSelf) detailPopup.Hide();
    }

    private void HireDriver(string id)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        DriverData d = st.GetDriver(id);
        if (d == null || team.money < d.salary) { SoundManager.Instance.PlayError(); return; }
        Pay(d.salary);
        d.teamId = team.id;
        team.driverIds.Add(d.id);
        SoundManager.Instance.PlaySuccess();
        CloseDetailIfOpen();
        AfterChange();
    }

    private void SignAcademy(string id)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        DriverData d = st.GetDriver(id);
        long cost = d != null ? d.salary / 2 : 0;
        if (d == null || team.money < cost) { SoundManager.Instance.PlayError(); return; }
        Pay(cost);
        d.teamId = team.id;
        team.driverIds.Add(d.id);
        SoundManager.Instance.PlaySuccess();
        CloseDetailIfOpen();
        AfterChange();
    }

    private void HireEngineer(string id)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        EngineerData e = st.GetEngineer(id);
        if (e == null || team.money < e.salary) { SoundManager.Instance.PlayError(); return; }
        Pay(e.salary);
        e.teamId = team.id;
        team.engineerIds.Add(e.id);
        SoundManager.Instance.PlaySuccess();
        CloseDetailIfOpen();
        AfterChange();
    }

    private void HireStaff(string id)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        StaffData s = st.GetStaff(id);
        if (s == null || team.money < s.salary) { SoundManager.Instance.PlayError(); return; }
        Pay(s.salary);
        string oldId = s.role == StaffRole.TeamPrincipal ? team.teamPrincipalId : team.technicalDirectorId;
        StaffData old = st.GetStaff(oldId);
        if (old != null) old.teamId = "";
        s.teamId = team.id;
        if (s.role == StaffRole.TeamPrincipal) team.teamPrincipalId = s.id;
        else team.technicalDirectorId = s.id;
        SoundManager.Instance.PlaySuccess();
        CloseDetailIfOpen();
        AfterChange();
    }

    private void ReleaseDriver(string id)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        DriverData d = st.GetDriver(id);
        if (d == null) return;
        if (IsRaceDriver(team, id)) { SoundManager.Instance.PlayError(); return; }
        d.teamId = "";
        team.driverIds.Remove(id);
        SoundManager.Instance.PlayBack();
        CloseDetailIfOpen();
        AfterChange();
    }

    private void ReleaseEngineer(string id)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        EngineerData e = st.GetEngineer(id);
        if (e == null) return;
        e.teamId = "";
        e.isAssigned = false;
        team.engineerIds.Remove(id);
        SoundManager.Instance.PlayBack();
        CloseDetailIfOpen();
        AfterChange();
    }

    private void AssignSeat(string id, int slot)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        team.raceDriverIds[slot] = id;
        SoundManager.Instance.PlaySuccess();
        CloseDetailIfOpen();
        AfterChange();
    }

    private static string SalaryStr(long v)
    {
        return ResourceCounter.FormatMoney(v);
    }
}
