using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarController : MonoBehaviour
{
    public Button backButton;
    public ResourceCounter moneyCounter;

    public StaffSubTab[] subTabs;
    public Button[] subTabClickables;

    public GameObject buildContainer;
    public RectTransform listContent;
    public GameObject rowTemplate;
    public TMP_Text emptyLabel;

    public GameObject setupContainer;
    public StaffSubTab[] carSelTabs;
    public Button[] carSelClickables;
    public Slider downforceSlider;
    public Slider balanceSlider;
    public Slider reliabilitySlider;
    public TMP_Text downforceValue;
    public TMP_Text balanceValue;
    public TMP_Text reliabilityValue;
    public TMP_Text setupRatingText;
    public Image setupRatingFill;
    public Button autoSetupButton;
    public TMP_Text trackInfoText;

    private int currentTab = -1;
    private int selectedCar = 0;
    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private static readonly Color CarTag = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color AeroTag = new Color(0.30f, 0.50f, 0.75f);
    private static readonly Color EngineTag = new Color(0.85f, 0.45f, 0.30f);
    private static readonly Color ChassisTag = new Color(0.55f, 0.40f, 0.75f);
    private static readonly Color RelTag = new Color(0.30f, 0.65f, 0.40f);

    private const float DevStep = 5f;

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
        for (int i = 0; i < carSelClickables.Length; i++)
        {
            if (carSelClickables[i] == null) continue;
            int idx = i;
            carSelClickables[i].onClick.AddListener(() => SelectCar(idx));
        }

        downforceSlider.onValueChanged.AddListener(v => OnSlider(0, v));
        balanceSlider.onValueChanged.AddListener(v => OnSlider(1, v));
        reliabilitySlider.onValueChanged.AddListener(v => OnSlider(2, v));
        autoSetupButton.onClick.AddListener(AutoSetup);

        moneyCounter.SetImmediate(GameManager.Instance.State.PlayerTeam.money);
        SelectTab(0, false);
        TransitionManager.Instance.FadeIn();
    }

    private void GoBack()
    {
        Save();
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Hub");
    }

    private void SelectTab(int index, bool playSound)
    {
        if (currentTab == index) return;
        if (currentTab == 1 && index != 1) Save();
        currentTab = index;
        for (int i = 0; i < subTabs.Length; i++)
            if (subTabs[i] != null) subTabs[i].SetSelected(i == index);
        if (playSound) SoundManager.Instance.PlayClick();

        buildContainer.SetActive(index == 0);
        setupContainer.SetActive(index == 1);

        if (index == 0) BuildList();
        else RefreshSetup();
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

        for (int c = 0; c < team.cars.Count; c++)
        {
            CarData car = team.cars[c];
            NewRow().Bind(car.carName, "Overall performance", "CAR", CarTag,
                Mathf.RoundToInt(car.OverallPerformance), null, null, null, false);

            AddGroup(car, c, 0, "Aerodynamics", "Front, rear wings and floor", AeroTag, money);
            AddGroup(car, c, 1, "Engine", "Power and efficiency", EngineTag, money);
            AddGroup(car, c, 2, "Chassis", "Balance, suspension, weight", ChassisTag, money);
            AddGroup(car, c, 3, "Reliability", "Mechanical reliability", RelTag, money);
        }
        emptyLabel.gameObject.SetActive(team.cars.Count == 0);
        emptyLabel.text = "No cars";
    }

    private void AddGroup(CarData car, int carIndex, int group, string label, string sub, Color tag, long money)
    {
        float rating = GroupRating(car, group);
        bool max = rating >= 99.5f;
        long cost = DevCost(rating);
        int ci = carIndex;
        int g = group;
        NewRow().Bind(label, sub, "DEV", tag, Mathf.RoundToInt(rating),
            null, max ? null : "Develop " + ResourceCounter.FormatMoney(cost),
            () => DevelopGroup(ci, g), !max && money >= cost);
    }

    private float GroupRating(CarData car, int group)
    {
        if (group == 0) return car.AeroRating;
        if (group == 1) return car.EngineRating;
        if (group == 2) return car.ChassisRating;
        return car.reliability;
    }

    private long DevCost(float rating)
    {
        return 1000000L + Mathf.RoundToInt(rating) * 100000L;
    }

    private void DevelopGroup(int carIndex, int group)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        if (carIndex >= team.cars.Count) return;
        CarData car = team.cars[carIndex];
        float rating = GroupRating(car, group);
        if (rating >= 99.5f) { SoundManager.Instance.PlayError(); return; }
        long cost = DevCost(rating);
        if (team.money < cost) { SoundManager.Instance.PlayError(); return; }

        Pay(cost);
        if (group == 0)
        {
            car.frontWing = Clamp(car.frontWing + DevStep);
            car.rearWing = Clamp(car.rearWing + DevStep);
            car.floor = Clamp(car.floor + DevStep);
        }
        else if (group == 1)
        {
            car.enginePower = Clamp(car.enginePower + DevStep);
            car.engineEfficiency = Clamp(car.engineEfficiency + DevStep);
        }
        else if (group == 2)
        {
            car.balance = Clamp(car.balance + DevStep);
            car.suspension = Clamp(car.suspension + DevStep);
            car.weight = Mathf.Clamp(car.weight - DevStep, 20f, 80f);
        }
        else
        {
            car.reliability = Clamp(car.reliability + DevStep);
        }

        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        Save();
        BuildList();
    }

    private void SelectCar(int index)
    {
        if (selectedCar == index)
        {
            SoundManager.Instance.PlayClick();
            return;
        }
        Save();
        selectedCar = index;
        SoundManager.Instance.PlayClick();
        RefreshSetup();
    }

    private void RefreshSetup()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        if (selectedCar >= team.cars.Count) selectedCar = 0;

        for (int i = 0; i < carSelTabs.Length; i++)
            if (carSelTabs[i] != null) carSelTabs[i].SetSelected(i == selectedCar);

        CarData car = team.cars[selectedCar];
        downforceSlider.SetValueWithoutNotify(car.downforceSetup);
        balanceSlider.SetValueWithoutNotify(car.balanceSetup);
        reliabilitySlider.SetValueWithoutNotify(car.reliabilitySetup);
        downforceValue.text = Pct(car.downforceSetup);
        balanceValue.text = Pct(car.balanceSetup);
        reliabilityValue.text = Pct(car.reliabilitySetup);

        RaceData next = st.season.NextRace;
        trackInfoText.text = next != null
            ? car.carName + "  •  tune for " + next.trackName
            : car.carName + "  •  no upcoming race";

        UpdateRating(car, false);
    }

    private void OnSlider(int which, float v)
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        if (selectedCar >= team.cars.Count) return;
        CarData car = team.cars[selectedCar];

        if (which == 0) { car.downforceSetup = v; downforceValue.text = Pct(v); }
        else if (which == 1) { car.balanceSetup = v; balanceValue.text = Pct(v); }
        else { car.reliabilitySetup = v; reliabilityValue.text = Pct(v); }

        UpdateRating(car, true);
    }

    private void AutoSetup()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        if (selectedCar >= team.cars.Count) return;
        CarData car = team.cars[selectedCar];

        float d, b, r;
        Optimum(st.season.NextRace, out d, out b, out r);
        car.downforceSetup = d;
        car.balanceSetup = b;
        car.reliabilitySetup = r;

        downforceSlider.SetValueWithoutNotify(d);
        balanceSlider.SetValueWithoutNotify(b);
        reliabilitySlider.SetValueWithoutNotify(r);
        downforceValue.text = Pct(d);
        balanceValue.text = Pct(b);
        reliabilityValue.text = Pct(r);

        UpdateRating(car, true);
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        Save();
    }

    private void UpdateRating(CarData car, bool animate)
    {
        float rating = SetupRating(car);
        setupRatingText.text = Mathf.RoundToInt(rating).ToString();
        float target = Mathf.Clamp01(rating / 100f);
        if (animate) setupRatingFill.fillAmount = target;
        else
        {
            setupRatingFill.fillAmount = 0f;
            setupRatingFill.fillAmount = target;
        }
    }

    private float SetupRating(CarData car)
    {
        float d, b, r;
        Optimum(GameManager.Instance.State.season.NextRace, out d, out b, out r);
        float dev = (Mathf.Abs(car.downforceSetup - d) + Mathf.Abs(car.balanceSetup - b) + Mathf.Abs(car.reliabilitySetup - r)) / 3f;
        return Mathf.Clamp(100f * (1f - dev), 0f, 100f);
    }

    private void Optimum(RaceData r, out float downforce, out float balance, out float reliability)
    {
        if (r == null)
        {
            downforce = 0.5f;
            balance = 0.5f;
            reliability = 0.5f;
            return;
        }
        downforce = Mathf.Clamp01(0.75f - (r.trackLengthKm - 5f) * 0.07f + r.weatherWetChance * 0.25f);
        balance = Mathf.Clamp01(0.45f + (r.trackLengthKm - 5f) * 0.05f);
        reliability = Mathf.Clamp01(0.40f + r.weatherWetChance * 0.30f + r.laps * 0.004f);
    }

    private void Pay(long amount)
    {
        TeamData team = GameManager.Instance.State.PlayerTeam;
        team.money -= amount;
        moneyCounter.AnimateTo(team.money);
    }

    private void Save()
    {
        SaveManager.Instance.SaveGame(GameManager.Instance.State);
    }

    private static float Clamp(float v) { return Mathf.Clamp(v, 0f, 100f); }

    private static string Pct(float v) { return Mathf.RoundToInt(v * 100f) + "%"; }
}
