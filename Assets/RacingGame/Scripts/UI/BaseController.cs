using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseController : MonoBehaviour
{
    public Button backButton;
    public ResourceCounter moneyCounter;

    public RectTransform listContent;
    public GameObject rowTemplate;
    public TMP_Text emptyLabel;

    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private static readonly Color BaseTag = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color FacTag = new Color(0.30f, 0.50f, 0.75f);

    private const int FacMaxLevel = 10;

    private void Start()
    {
        if (GameManager.Instance.State == null)
            GameManager.Instance.StartNewCareer(Difficulty.Normal);

        if (backButton != null) backButton.onClick.AddListener(GoBack);

        moneyCounter.SetImmediate(GameManager.Instance.State.PlayerTeam.money);
        BuildList();
        TransitionManager.Instance.FadeIn();
    }

    private void GoBack()
    {
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Hub");
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

    private void BuildList()
    {
        ClearRows();
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        long money = team.money;

        NewRow().Bind("Base HQ", "Headquarters facilities", "BASE", BaseTag, AverageLevel(team), null, null, null, false);

        for (int i = 0; i < team.facilities.Count; i++)
        {
            FacilityData f = team.facilities[i];
            bool max = f.level >= FacMaxLevel;
            long cost = f.upgradeCost;
            FacilityType ft = f.type;
            NewRow().Bind(FacilityName(f.type), FacilityEffect(f.type), "LV " + f.level, FacTag, f.level,
                null, max ? null : "Upgrade " + ResourceCounter.FormatMoney(cost),
                () => Upgrade(ft), !max && money >= cost);
        }

        emptyLabel.gameObject.SetActive(team.facilities.Count == 0);
        emptyLabel.text = "No facilities";
    }

    private int AverageLevel(TeamData team)
    {
        if (team.facilities.Count == 0) return 0;
        int sum = 0;
        for (int i = 0; i < team.facilities.Count; i++) sum += team.facilities[i].level;
        return Mathf.RoundToInt((float)sum / team.facilities.Count);
    }

    private void Upgrade(FacilityType type)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        FacilityData f = FindFacility(team, type);
        if (f == null || f.level >= FacMaxLevel) { SoundManager.Instance.PlayError(); return; }
        if (team.money < f.upgradeCost) { SoundManager.Instance.PlayError(); return; }

        Pay(f.upgradeCost);
        f.level++;
        f.upgradeCost = (f.level + 1) * 8000000;
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        SaveManager.Instance.SaveGame(st);
        BuildList();
    }

    private void Pay(long amount)
    {
        TeamData team = GameManager.Instance.State.PlayerTeam;
        team.money -= amount;
        moneyCounter.AnimateTo(team.money);
    }

    private FacilityData FindFacility(TeamData team, FacilityType type)
    {
        for (int i = 0; i < team.facilities.Count; i++)
            if (team.facilities[i].type == type) return team.facilities[i];
        return null;
    }

    private string FacilityName(FacilityType t)
    {
        if (t == FacilityType.WindTunnel) return "Wind Tunnel";
        if (t == FacilityType.MaterialLab) return "Material Lab";
        if (t == FacilityType.DataCenter) return "Data Center";
        return t.ToString();
    }

    private string FacilityEffect(FacilityType t)
    {
        if (t == FacilityType.Factory) return "Car build quality and speed";
        if (t == FacilityType.WindTunnel) return "Aero development efficiency";
        if (t == FacilityType.Simulator) return "Setup and driver gains";
        if (t == FacilityType.MaterialLab) return "Reliability and materials";
        return "Data analysis and strategy";
    }
}
