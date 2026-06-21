using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceEntrant
{
    public string driverId;
    public string teamId;
    public string driverName;
    public Color color;
    public bool isPlayer;

    public float paceFactor;
    public float qualiPace;

    public float distance;
    public bool dnf;
    public bool finished;

    public int position;
    public int points;
    public float gapSeconds;
    public bool lapped;
    public int lapsDown;

    public RectTransform dot;
    public TimingRow row;
    public float towerY;
}

public class WeekendController : MonoBehaviour
{
    // ---- tuning knobs ----
    private const float BaseRaceSeconds = 30f;
    private const float DisplayLapSeconds = 92f;
    private const float DnfBasePerSecond = 0.004f;
    private const float NoiseAmp = 0.06f;
    private const float DotLerp = 10f;
    private const float TowerLerp = 12f;
    private const long PrizeTop = 8000000L;
    private const long PrizeBottom = 500000L;
    private const int ResearchBase = 25;
    private const int SalaryRaces = 22;
    private const long UpkeepPerLevel = 150000L;

    public Button backButton;
    public TMP_Text titleText;
    public TMP_Text lapText;

    public GameObject practicePanel;
    public TMP_Text practiceInfoText;
    public TMP_Text practiceCarsText;
    public Button runPracticeButton;
    public TMP_Text runPracticeLabel;
    public Button toQualifyingButton;

    public GameObject qualifyingPanel;
    public Button runQualifyingButton;
    public Button toRaceButton;

    public GameObject racePanel;
    public RectTransform trackArea;
    public GameObject dotTemplate;
    public RectTransform towerArea;
    public GameObject towerRowTemplate;
    public Button speedButton;
    public TMP_Text speedLabel;
    public Button skipButton;

    public GameObject resultsPanel;
    public TMP_Text resultsSummaryText;
    public Button finishButton;

    public GameObject listScroll;
    public RectTransform listContent;
    public GameObject rowTemplate;

    private static readonly int[] Points = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };

    private RaceData race;
    private readonly List<RaceEntrant> entrants = new List<RaceEntrant>();
    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private int phase = -1;
    private bool racing;
    private bool qualiDone;
    private float timeScale = 1f;
    private float rowH = 40f;
    private long lastNet;
    private int lastRP;

    private void Start()
    {
        if (GameManager.Instance.State == null)
            GameManager.Instance.StartNewCareer(Difficulty.Normal);

        race = GameManager.Instance.State.season.NextRace;

        backButton.onClick.AddListener(GoBack);
        runPracticeButton.onClick.AddListener(RunPractice);
        toQualifyingButton.onClick.AddListener(() => SetPhase(1));
        runQualifyingButton.onClick.AddListener(RunQualifying);
        toRaceButton.onClick.AddListener(() => SetPhase(2));
        speedButton.onClick.AddListener(CycleSpeed);
        skipButton.onClick.AddListener(SkipRace);
        finishButton.onClick.AddListener(FinishWeekend);

        BuildEntrants();
        SetPhase(0);
        TransitionManager.Instance.FadeIn();
    }

    private void GoBack()
    {
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Hub");
    }

    private void BuildEntrants()
    {
        entrants.Clear();
        if (race == null) return;
        GameState st = GameManager.Instance.State;

        for (int t = 0; t < st.teams.Count; t++)
        {
            TeamData team = st.teams[t];
            bool isPlayer = team.id == "team_0";
            for (int s = 0; s < team.raceDriverIds.Length && s < team.cars.Count; s++)
            {
                DriverData d = st.GetDriver(team.raceDriverIds[s]);
                if (d == null) continue;
                CarData car = team.cars[s];

                RaceEntrant e = new RaceEntrant();
                e.driverId = d.id;
                e.teamId = team.id;
                e.driverName = d.FullName;
                e.isPlayer = isPlayer;
                e.color = TeamColor(t, isPlayer);

                float setupTerm = SetupTerm(car);
                float strength = 0.5f * car.OverallPerformance + 0.35f * d.OverallSkill + 0.15f * setupTerm;
                e.paceFactor = 0.88f + strength * 0.0024f;
                e.qualiPace = 0.45f * car.OverallPerformance + 0.40f * d.qualifying + 0.15f * setupTerm + Random.Range(-3f, 3f);

                entrants.Add(e);
            }
        }
    }

    private float SetupTerm(CarData car)
    {
        float d, b, r;
        Optimum(race, out d, out b, out r);
        float dev = (Mathf.Abs(car.downforceSetup - d) + Mathf.Abs(car.balanceSetup - b) + Mathf.Abs(car.reliabilitySetup - r)) / 3f;
        return Mathf.Clamp(100f * (1f - dev), 0f, 100f);
    }

    private void Optimum(RaceData r, out float downforce, out float balance, out float reliability)
    {
        if (r == null) { downforce = 0.5f; balance = 0.5f; reliability = 0.5f; return; }
        downforce = Mathf.Clamp01(0.75f - (r.trackLengthKm - 5f) * 0.07f + r.weatherWetChance * 0.25f);
        balance = Mathf.Clamp01(0.45f + (r.trackLengthKm - 5f) * 0.05f);
        reliability = Mathf.Clamp01(0.40f + r.weatherWetChance * 0.30f + r.laps * 0.004f);
    }

    private Color TeamColor(int index, bool isPlayer)
    {
        if (isPlayer) return new Color(0.96f, 0.65f, 0.14f);
        float h = (index * 0.137f) % 1f;
        return Color.HSVToRGB(h, 0.55f, 0.95f);
    }

    private void SetPhase(int p)
    {
        phase = p;
        racing = false;

        practicePanel.SetActive(p == 0);
        qualifyingPanel.SetActive(p == 1);
        racePanel.SetActive(p == 2);
        resultsPanel.SetActive(p == 3);
        listScroll.SetActive(p == 1 || p == 3);

        if (p != 0) SoundManager.Instance.PlayClick();

        if (race == null)
        {
            titleText.text = "No Race";
            practiceInfoText.text = "Season complete — no upcoming race.";
            practiceCarsText.text = "";
            runPracticeButton.interactable = false;
            toQualifyingButton.interactable = false;
            return;
        }

        titleText.text = "R" + race.round + "  " + race.trackName;

        if (p == 0) SetupPractice();
        else if (p == 1) SetupQualifying();
        else if (p == 2) StartRace();
        else if (p == 3) { }
    }

    // ---------- Practice ----------
    private void SetupPractice()
    {
        practiceInfoText.text = race.trackName + "  •  " + race.country + "\n"
            + race.laps + " laps  •  " + race.trackLengthKm.ToString("0.0") + " km  •  Wet "
            + Mathf.RoundToInt(race.weatherWetChance * 100f) + "%";

        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;
        string txt = "";
        for (int s = 0; s < team.raceDriverIds.Length && s < team.cars.Count; s++)
        {
            DriverData d = st.GetDriver(team.raceDriverIds[s]);
            CarData car = team.cars[s];
            if (d == null) continue;
            int setup = Mathf.RoundToInt(SetupTerm(car));
            txt += d.FullName + "   Car " + Mathf.RoundToInt(car.OverallPerformance) + "   Setup " + setup + "\n";
        }
        practiceCarsText.text = txt;
        runPracticeLabel.text = "RUN PRACTICE";
        runPracticeButton.interactable = true;
        toQualifyingButton.interactable = false;
    }

    private void RunPractice()
    {
        runPracticeLabel.text = "PRACTICE COMPLETE";
        runPracticeButton.interactable = false;
        toQualifyingButton.interactable = true;
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Light();
    }

    // ---------- Qualifying ----------
    private void SetupQualifying()
    {
        toRaceButton.interactable = qualiDone;
        if (qualiDone) PopulateGrid();
        else ClearList();
    }

    private void RunQualifying()
    {
        entrants.Sort((a, b) => b.qualiPace.CompareTo(a.qualiPace));
        qualiDone = true;
        toRaceButton.interactable = true;
        PopulateGrid();
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Medium();
    }

    private void PopulateGrid()
    {
        ClearList();
        float best = entrants.Count > 0 ? entrants[0].qualiPace : 0f;
        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            float gap = (best - e.qualiPace) * 0.12f;
            string sub = TeamName(e.teamId) + "   " + (i == 0 ? "POLE" : "+" + gap.ToString("0.000") + "s");
            Color tag = e.isPlayer ? new Color(0.96f, 0.65f, 0.14f) : new Color(0.30f, 0.50f, 0.75f);
            NewRow().Bind(e.driverName, sub, "P" + (i + 1), tag, i + 1, null, null, null, false);
        }
    }

    // ---------- Race ----------
    private void StartRace()
    {
        ClearDots();
        BuildTrack();
        BuildTower();

        for (int i = 0; i < entrants.Count; i++)
        {
            entrants[i].distance = (entrants.Count - i) * 0.0008f;
            entrants[i].dnf = false;
            entrants[i].finished = false;
        }

        timeScale = 1f;
        speedLabel.text = "1x";
        racing = true;
        UpdatePositions(true);
    }

    private void Update()
    {
        if (!racing || phase != 2 || race == null) return;

        float dt = Time.deltaTime * timeScale;
        float speedBase = race.laps / BaseRaceSeconds;

        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            if (e.dnf || e.finished) continue;

            if (Random.value < DnfBasePerSecond * dt) { e.dnf = true; continue; }

            float inst = e.paceFactor + Random.Range(-NoiseAmp, NoiseAmp);
            e.distance += speedBase * inst * dt;
            if (e.distance >= race.laps) { e.distance = race.laps; e.finished = true; }
        }

        UpdatePositions(false);

        RaceEntrant leader = Leader();
        int leaderLap = leader != null ? Mathf.Clamp(Mathf.FloorToInt(leader.distance) + 1, 1, race.laps) : 1;
        lapText.text = "LAP " + leaderLap + "/" + race.laps;

        if (leader != null && leader.finished) FinishRace();
    }

    private RaceEntrant Leader()
    {
        RaceEntrant best = null;
        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            if (e.dnf) continue;
            if (best == null || e.distance > best.distance) best = e;
        }
        return best;
    }

    private void UpdatePositions(bool snap)
    {
        entrants.Sort((a, b) =>
        {
            if (a.dnf != b.dnf) return a.dnf ? 1 : -1;
            return b.distance.CompareTo(a.distance);
        });

        float leaderDist = 0f;
        for (int i = 0; i < entrants.Count; i++)
            if (!entrants[i].dnf) { leaderDist = entrants[i].distance; break; }

        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            e.position = i + 1;

            if (e.dot != null)
            {
                Vector2 target = LoopPos(e.distance - Mathf.Floor(e.distance));
                e.dot.anchoredPosition = snap ? target : Vector2.Lerp(e.dot.anchoredPosition, target, Time.deltaTime * DotLerp);
            }

            if (e.row != null)
            {
                float ty = -i * rowH;
                e.towerY = snap ? ty : Mathf.Lerp(e.towerY, ty, Time.deltaTime * TowerLerp);
                RectTransform rt = e.row.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, e.towerY);

                string gap;
                if (e.dnf) gap = "DNF";
                else if (i == 0) gap = "LEADER";
                else
                {
                    float loops = leaderDist - e.distance;
                    if (loops >= 1f) gap = "+" + Mathf.FloorToInt(loops) + "L";
                    else gap = "+" + (loops * DisplayLapSeconds).ToString("0.0") + "s";
                }
                e.row.Set(e.position, e.driverName, e.color, gap, e.isPlayer);
            }
        }
    }

    private void CycleSpeed()
    {
        if (timeScale < 1.5f) timeScale = 2f;
        else if (timeScale < 3f) timeScale = 4f;
        else timeScale = 1f;
        speedLabel.text = (int)timeScale + "x";
        SoundManager.Instance.PlayClick();
    }

    private void SkipRace()
    {
        if (!racing) return;
        float speedBase = race.laps / BaseRaceSeconds;
        int guard = 0;
        while (guard++ < 5000)
        {
            RaceEntrant leader = Leader();
            if (leader != null && leader.distance >= race.laps) break;
            for (int i = 0; i < entrants.Count; i++)
            {
                RaceEntrant e = entrants[i];
                if (e.dnf || e.finished) continue;
                if (Random.value < DnfBasePerSecond * 0.1f) { e.dnf = true; continue; }
                e.distance += speedBase * e.paceFactor * 0.1f;
                if (e.distance >= race.laps) { e.distance = race.laps; e.finished = true; }
            }
        }
        FinishRace();
    }

    private void FinishRace()
    {
        racing = false;

        entrants.Sort((a, b) =>
        {
            if (a.dnf != b.dnf) return a.dnf ? 1 : -1;
            return b.distance.CompareTo(a.distance);
        });

        float leaderDist = entrants.Count > 0 ? entrants[0].distance : 0f;
        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            e.position = i + 1;
            e.points = (!e.dnf && i < Points.Length) ? Points[i] : 0;
            float loops = leaderDist - e.distance;
            e.lapped = loops >= 1f;
            e.lapsDown = Mathf.FloorToInt(loops);
            e.gapSeconds = loops * DisplayLapSeconds;
        }

        WriteResults();
        ApplySettlement();
        SaveManager.Instance.SaveGame(GameManager.Instance.State);
        SoundManager.Instance.PlaySuccess();
        HapticManager.Instance.Success();
        SetPhase(3);
        PopulateResults();
    }

    private void WriteResults()
    {
        GameState st = GameManager.Instance.State;
        race.results.Clear();
        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            RaceResultEntry r = new RaceResultEntry();
            r.driverId = e.driverId;
            r.teamId = e.teamId;
            r.position = e.position;
            r.points = e.points;
            r.gapSeconds = e.gapSeconds;
            r.dnf = e.dnf;
            race.results.Add(r);

            StandingEntry ds = FindStanding(st.season.driverStandings, e.driverId);
            ds.points += e.points;
            if (e.position == 1 && !e.dnf) ds.wins += 1;

            StandingEntry cs = FindStanding(st.season.constructorStandings, e.teamId);
            cs.points += e.points;
            if (e.position == 1 && !e.dnf) cs.wins += 1;
        }
        race.completed = true;
    }

    private StandingEntry FindStanding(List<StandingEntry> list, string id)
    {
        for (int i = 0; i < list.Count; i++) if (list[i].id == id) return list[i];
        StandingEntry s = new StandingEntry();
        s.id = id;
        list.Add(s);
        return s;
    }

    private void PopulateResults()
    {
        ClearList();
        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            string gap;
            if (e.dnf) gap = "DNF";
            else if (e.position == 1) gap = "Winner";
            else if (e.lapped) gap = "+" + e.lapsDown + " lap";
            else gap = "+" + e.gapSeconds.ToString("0.0") + "s";
            string sub = TeamName(e.teamId) + "   " + gap + "   " + e.points + " pts";
            Color tag = e.isPlayer ? new Color(0.96f, 0.65f, 0.14f) : PosColor(e.position);
            NewRow().Bind(e.driverName, sub, "P" + e.position, tag, e.position, null, null, null, false);
        }

        BuildSummary();
    }

    private void ApplySettlement()
    {
        GameState st = GameManager.Instance.State;
        TeamData team = st.PlayerTeam;

        long prize = 0;
        int rp = ResearchBase;
        int count = Mathf.Max(entrants.Count, 2);
        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];
            if (!e.isPlayer) continue;
            prize += PrizeForPosition(e.position, count);
            rp += Mathf.Max(0, 11 - e.position);
        }

        long income = prize + SponsorIncome(team);
        long expenses = PerRaceSalaries(team) + UpkeepPerRace(team);
        long net = income - expenses;

        team.money += net;
        team.researchPoints += rp;
        lastNet = net;
        lastRP = rp;
    }

    private long PrizeForPosition(int pos, int count)
    {
        float frac = count > 1 ? (pos - 1) / (float)(count - 1) : 0f;
        return (long)Mathf.Lerp(PrizeTop, PrizeBottom, frac);
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

    private void BuildSummary()
    {
        GameState st = GameManager.Instance.State;
        int best = 99;
        int pts = 0;
        for (int i = 0; i < entrants.Count; i++)
        {
            if (entrants[i].isPlayer)
            {
                pts += entrants[i].points;
                if (entrants[i].position < best) best = entrants[i].position;
            }
        }

        st.season.constructorStandings.Sort((a, b) => b.points.CompareTo(a.points));
        int champPos = 1;
        for (int i = 0; i < st.season.constructorStandings.Count; i++)
            if (st.season.constructorStandings[i].id == "team_0") { champPos = i + 1; break; }

        string netStr = (lastNet >= 0 ? "+" : "-") + ResourceCounter.FormatMoney(System.Math.Abs(lastNet));
        resultsSummaryText.text = "Best P" + best + "   •   " + pts + " pts   •   Champ P" + champPos
            + "\nPayout " + netStr + "   •   +" + lastRP + " RP";
    }

    private Color PosColor(int pos)
    {
        if (pos == 1) return new Color(0.95f, 0.78f, 0.25f);
        if (pos == 2) return new Color(0.70f, 0.74f, 0.80f);
        if (pos == 3) return new Color(0.80f, 0.52f, 0.30f);
        return new Color(0.40f, 0.42f, 0.50f);
    }

    private void FinishWeekend()
    {
        GameState st = GameManager.Instance.State;
        st.currentWeek += 2;
        RaceData next = st.season.NextRace;
        if (next != null) st.season.currentRound = next.round;
        SaveManager.Instance.SaveGame(st);
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("Hub");
    }

    // ---------- track + tower spawn ----------
    private void BuildTrack()
    {
        for (int i = trackArea.childCount - 1; i >= 0; i--)
        {
            Transform c = trackArea.GetChild(i);
            if (c.gameObject != dotTemplate) Destroy(c.gameObject);
        }

        int markers = 64;
        for (int i = 0; i < markers; i++)
        {
            GameObject m = new GameObject("TrackDot");
            m.transform.SetParent(trackArea, false);
            Image img = m.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.18f);
            RectTransform rt = m.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(8, 8);
            rt.anchoredPosition = LoopPos((float)i / markers);
        }

        GameObject sf = new GameObject("StartLine");
        sf.transform.SetParent(trackArea, false);
        Image sfImg = sf.AddComponent<Image>();
        sfImg.color = Color.white;
        RectTransform sfrt = sf.GetComponent<RectTransform>();
        sfrt.anchorMin = new Vector2(0.5f, 0.5f);
        sfrt.anchorMax = new Vector2(0.5f, 0.5f);
        sfrt.pivot = new Vector2(0.5f, 0.5f);
        sfrt.sizeDelta = new Vector2(8, 22);
        sfrt.anchoredPosition = LoopPos(0f);
    }

    private void ClearDots()
    {
        for (int i = 0; i < entrants.Count; i++) entrants[i].dot = null;
    }

    private Vector2 LoopPos(float t)
    {
        Rect r = trackArea.rect;
        float rx = r.width * 0.5f - 40f;
        float ry = r.height * 0.5f - 40f;
        float ang = t * Mathf.PI * 2f - Mathf.PI * 0.5f;
        return new Vector2(rx * Mathf.Cos(ang), ry * Mathf.Sin(ang));
    }

    private void BuildTower()
    {
        for (int i = 0; i < spawnedRows.Count; i++) Destroy(spawnedRows[i]);
        spawnedRows.Clear();

        int count = Mathf.Max(entrants.Count, 1);
        rowH = towerArea.rect.height / count;

        for (int i = 0; i < entrants.Count; i++)
        {
            RaceEntrant e = entrants[i];

            GameObject dot = Instantiate(dotTemplate, trackArea);
            dot.SetActive(true);
            Image dImg = dot.GetComponent<Image>();
            dImg.color = e.color;
            RectTransform drt = dot.GetComponent<RectTransform>();
            drt.sizeDelta = e.isPlayer ? new Vector2(30, 30) : new Vector2(22, 22);
            e.dot = drt;
            dot.transform.SetAsLastSibling();

            GameObject rowGo = Instantiate(towerRowTemplate, towerArea);
            rowGo.SetActive(true);
            RectTransform rrt = rowGo.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(rrt.sizeDelta.x, rowH);
            spawnedRows.Add(rowGo);
            e.row = rowGo.GetComponent<TimingRow>();
            e.towerY = -i * rowH;
        }
    }

    // ---------- shared list ----------
    private void ClearList()
    {
        for (int i = listContent.childCount - 1; i >= 0; i--)
        {
            Transform c = listContent.GetChild(i);
            if (c.gameObject != rowTemplate) Destroy(c.gameObject);
        }
    }

    private PersonRow NewRow()
    {
        GameObject go = Instantiate(rowTemplate, listContent);
        go.SetActive(true);
        return go.GetComponent<PersonRow>();
    }

    private string TeamName(string teamId)
    {
        GameState st = GameManager.Instance.State;
        for (int i = 0; i < st.teams.Count; i++) if (st.teams[i].id == teamId) return st.teams[i].teamName;
        return teamId;
    }
}
