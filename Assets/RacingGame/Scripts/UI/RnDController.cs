using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RnDController : MonoBehaviour
{
    public Button backButton;
    public ResourceCounter moneyCounter;

    public StaffSubTab[] subTabs;
    public Button[] subTabClickables;

    public RectTransform listContent;
    public GameObject rowTemplate;
    public TMP_Text emptyLabel;

    private int currentTab = -1;
    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private static readonly Color DeptTag = new Color(0.30f, 0.50f, 0.75f);
    private static readonly Color TreeTag = new Color(0.55f, 0.40f, 0.75f);
    private static readonly Color UnlockedTag = new Color(0.30f, 0.65f, 0.40f);
    private static readonly Color NextTag = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color LockedTag = new Color(0.45f, 0.45f, 0.52f);

    private const int DeptMaxLevel = 5;

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
        if (currentTab == 0) BuildDepartments();
        else BuildTech();
    }

    private void BuildDepartments()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        long money = team.money;
        for (int i = 0; i < team.departments.Count; i++)
        {
            DepartmentData dep = team.departments[i];
            int engs = dep.assignedEngineerIds.Count;
            string sub = "Output " + Mathf.RoundToInt(dep.outputRating) + "    " + engs + " engineers";
            long cost = UpgradeCost(dep.level);
            bool max = dep.level >= DeptMaxLevel;
            DepartmentType t = dep.type;
            NewRow().Bind(DeptName(dep.type), sub, "LV " + dep.level, DeptTag, Mathf.RoundToInt(dep.outputRating),
                null, max ? null : "Upgrade " + ResourceCounter.FormatMoney(cost),
                () => UpgradeDept(t), !max && money >= cost);
        }
        SetEmpty(team.departments.Count == 0, "No departments");
    }

    private void BuildTech()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        long money = team.money;
        for (int t = 0; t < team.techTrees.Count; t++)
        {
            TechTree tree = team.techTrees[t];
            NewRow().Bind(TreeName(tree.type), "Tech tree    " + tree.CurrentLevel + "/5 unlocked", "TREE", TreeTag,
                tree.CurrentLevel, null, null, null, false);

            int nextLevel = NextLockedLevel(tree);
            for (int n = 0; n < tree.nodes.Count; n++)
            {
                TechNode node = tree.nodes[n];
                bool isNext = !node.unlocked && node.level == nextLevel;
                string tag = node.unlocked ? "UNLOCKED" : (isNext ? "NEXT" : "LOCKED");
                Color tagC = node.unlocked ? UnlockedTag : (isNext ? NextTag : LockedTag);
                string sub = "+" + Mathf.RoundToInt(node.performanceBonus) + " performance    Lv " + node.level;
                TechTreeType tt = tree.type;
                string nid = node.id;
                NewRow().Bind(node.nodeName, sub, tag, tagC, Mathf.RoundToInt(node.performanceBonus),
                    null, isNext ? "Unlock " + ResourceCounter.FormatMoney(node.unlockCost) : null,
                    () => UnlockNode(tt, nid), isNext && money >= node.unlockCost);
            }
        }
        SetEmpty(team.techTrees.Count == 0, "No tech trees");
    }

    private int NextLockedLevel(TechTree tree)
    {
        int best = int.MaxValue;
        for (int i = 0; i < tree.nodes.Count; i++)
            if (!tree.nodes[i].unlocked && tree.nodes[i].level < best) best = tree.nodes[i].level;
        return best;
    }

    private void SetEmpty(bool empty, string text)
    {
        emptyLabel.gameObject.SetActive(empty);
        emptyLabel.text = text;
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

    private long UpgradeCost(int level)
    {
        return level * 4000000L;
    }

    private void UpgradeDept(DepartmentType type)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        DepartmentData dep = FindDept(team, type);
        if (dep == null || dep.level >= DeptMaxLevel) { SoundManager.Instance.PlayError(); return; }
        long cost = UpgradeCost(dep.level);
        if (team.money < cost) { SoundManager.Instance.PlayError(); return; }
        Pay(cost);
        dep.level++;
        dep.outputRating = dep.level * 20f;
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        AfterChange();
    }

    private void UnlockNode(TechTreeType type, string nodeId)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        TechTree tree = FindTree(team, type);
        if (tree == null) return;
        TechNode node = null;
        for (int i = 0; i < tree.nodes.Count; i++) if (tree.nodes[i].id == nodeId) node = tree.nodes[i];
        if (node == null || node.unlocked) return;
        if (node.level != NextLockedLevel(tree)) { SoundManager.Instance.PlayError(); return; }
        if (team.money < node.unlockCost) { SoundManager.Instance.PlayError(); return; }
        Pay(node.unlockCost);
        node.unlocked = true;
        ApplyTreeBonus(team, type, node.performanceBonus);
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        AfterChange();
    }

    private void ApplyTreeBonus(TeamData team, TechTreeType type, float bonus)
    {
        for (int i = 0; i < team.cars.Count; i++)
        {
            CarData c = team.cars[i];
            if (type == TechTreeType.Aerodynamics)
            {
                c.frontWing = Clamp(c.frontWing + bonus);
                c.rearWing = Clamp(c.rearWing + bonus);
                c.floor = Clamp(c.floor + bonus);
            }
            else if (type == TechTreeType.Engine)
            {
                c.enginePower = Clamp(c.enginePower + bonus);
                c.engineEfficiency = Clamp(c.engineEfficiency + bonus);
            }
            else if (type == TechTreeType.Materials)
            {
                c.weight = Mathf.Clamp(c.weight - bonus, 20f, 80f);
                c.reliability = Clamp(c.reliability + bonus);
            }
            else
            {
                c.balance = Clamp(c.balance + bonus);
                c.suspension = Clamp(c.suspension + bonus);
            }
        }
    }

    private static float Clamp(float v) { return Mathf.Clamp(v, 0f, 100f); }

    private DepartmentData FindDept(TeamData team, DepartmentType type)
    {
        for (int i = 0; i < team.departments.Count; i++)
            if (team.departments[i].type == type) return team.departments[i];
        return null;
    }

    private TechTree FindTree(TeamData team, TechTreeType type)
    {
        for (int i = 0; i < team.techTrees.Count; i++)
            if (team.techTrees[i].type == type) return team.techTrees[i];
        return null;
    }

    private string DeptName(DepartmentType t) { return t.ToString(); }
    private string TreeName(TechTreeType t) { return t.ToString(); }
}
