using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Iteration7Setup
{
    private static Sprite uiSprite;
    private static Sprite knobSprite;

    private static readonly Color ColorBg = new Color(0.10f, 0.10f, 0.18f);
    private static readonly Color ColorPrimary = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ColorAccent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.20f);
    private static readonly Color ColorCard = new Color(0.16f, 0.16f, 0.26f);
    private static readonly Color ColorMuted = new Color(0.6f, 0.6f, 0.66f);

    [MenuItem("RacingGame/Iteration 7 - GP Weekend")]
    public static void Build()
    {
        EnsureFolder("Assets/RacingGame/Scenes");

        Scene wk = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildWeekendScene();
        string wkPath = "Assets/RacingGame/Scenes/Weekend.unity";
        EditorSceneManager.SaveScene(wk, wkPath);

        List<string> scenes = new List<string>();
        string[] prior = { "MainMenu", "Hub", "Staff", "RnD", "Car", "Base" };
        for (int i = 0; i < prior.Length; i++)
        {
            string p = "Assets/RacingGame/Scenes/" + prior[i] + ".unity";
            if (File.Exists(p)) scenes.Add(p);
        }
        scenes.Add(wkPath);
        AddScenesToBuild(scenes.ToArray());

        EditorSceneManager.OpenScene(wkPath);
        AssetDatabase.SaveAssets();
        Debug.Log("RacingGame Iteration 7 (GP Weekend) setup complete. Enter via Hub > Next Race, or open Weekend scene and Play.");
    }

    private static void BuildWeekendScene()
    {
        CreateCamera();
        CreateEventSystem();
        CreateManagers();

        Canvas canvas = CreateCanvas("Canvas");

        GameObject bg = CreateImage(canvas.transform, "Background", ColorBg);
        Stretch(bg.GetComponent<RectTransform>());

        GameObject safe = new GameObject("SafeArea");
        safe.transform.SetParent(canvas.transform, false);
        RectTransform safeRt = safe.AddComponent<RectTransform>();
        Stretch(safeRt);
        safe.AddComponent<SafeAreaFitter>();

        GameObject ctrlGo = new GameObject("WeekendController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        ctrlGo.AddComponent<RectTransform>();
        WeekendController ctrl = ctrlGo.AddComponent<WeekendController>();

        BuildTopBar(safe.transform, ctrl);
        BuildListScroll(safe.transform, ctrl);
        BuildPracticePanel(safe.transform, ctrl);
        BuildQualifyingPanel(safe.transform, ctrl);
        BuildRacePanel(safe.transform, ctrl);
        BuildResultsPanel(safe.transform, ctrl);
    }

    private static void BuildTopBar(Transform safe, WeekendController ctrl)
    {
        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(safe, false);
        Image hudBg = hud.AddComponent<Image>();
        hudBg.sprite = UISprite();
        hudBg.type = Image.Type.Sliced;
        hudBg.color = ColorPanel;
        SetRect(hud.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 150), Vector2.zero);

        Button back = CreateIconButton(hud.transform, "BackButton", "back", Color.white);
        SetRect(back.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(86, 86), new Vector2(20, -16));

        TMP_Text title = CreateText(hud.transform, "Title", "Weekend", 38, Color.white, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(560, 56), new Vector2(0, -20));
        title.fontStyle = FontStyles.Bold;

        TMP_Text lap = CreateText(hud.transform, "Lap", "", 26, ColorAccent, TextAlignmentOptions.Center);
        SetRect(lap.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(560, 32), new Vector2(0, -86));
        lap.fontStyle = FontStyles.Bold;

        ctrl.backButton = back;
        ctrl.titleText = title;
        ctrl.lapText = lap;
    }

    private static GameObject FullPanel(Transform safe, string name)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(safe, false);
        RectTransform rt = p.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, -150);
        return p;
    }

    private static GameObject BottomBar(Transform safe, string name)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(safe, false);
        RectTransform rt = p.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 200);
        rt.anchoredPosition = Vector2.zero;
        return p;
    }

    private static void BuildPracticePanel(Transform safe, WeekendController ctrl)
    {
        GameObject panel = FullPanel(safe, "PracticePanel");

        TMP_Text info = CreateText(panel.transform, "Info", "", 30, Color.white, TextAlignmentOptions.Top);
        SetRect(info.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 200), new Vector2(0, -30));

        TMP_Text cars = CreateText(panel.transform, "Cars", "", 28, ColorMuted, TextAlignmentOptions.Top);
        SetRect(cars.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 280), new Vector2(0, -300));

        TMP_Text runLabel;
        Button run = CreateWideButton(panel.transform, "RUN PRACTICE", 32, out runLabel);
        SetRect(run.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(900, 88), new Vector2(0, 130));

        TMP_Text _;
        Button toQ = CreateWideButton(panel.transform, "TO QUALIFYING", 32, out _);
        SetRect(toQ.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(900, 88), new Vector2(0, 24));

        ctrl.practicePanel = panel;
        ctrl.practiceInfoText = info;
        ctrl.practiceCarsText = cars;
        ctrl.runPracticeButton = run;
        ctrl.runPracticeLabel = runLabel;
        ctrl.toQualifyingButton = toQ;
    }

    private static void BuildQualifyingPanel(Transform safe, WeekendController ctrl)
    {
        GameObject panel = BottomBar(safe, "QualifyingPanel");

        TMP_Text _1;
        Button runQ = CreateWideButton(panel.transform, "RUN QUALIFYING", 32, out _1);
        SetRect(runQ.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(900, 84), new Vector2(0, 104));

        TMP_Text _2;
        Button toRace = CreateWideButton(panel.transform, "TO RACE", 32, out _2);
        SetRect(toRace.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(900, 84), new Vector2(0, 12));

        ctrl.qualifyingPanel = panel;
        ctrl.runQualifyingButton = runQ;
        ctrl.toRaceButton = toRace;
    }

    private static void BuildRacePanel(Transform safe, WeekendController ctrl)
    {
        GameObject panel = FullPanel(safe, "RacePanel");

        GameObject track = new GameObject("TrackArea");
        track.transform.SetParent(panel.transform, false);
        RectTransform trackRt = track.AddComponent<RectTransform>();
        trackRt.anchorMin = new Vector2(0, 1);
        trackRt.anchorMax = new Vector2(1, 1);
        trackRt.pivot = new Vector2(0.5f, 1);
        trackRt.sizeDelta = new Vector2(-80, 660);
        trackRt.anchoredPosition = new Vector2(0, -10);

        GameObject dot = new GameObject("CarDot");
        dot.transform.SetParent(track.transform, false);
        Image dotImg = dot.AddComponent<Image>();
        dotImg.sprite = KnobSprite();
        dotImg.color = Color.white;
        RectTransform dotRt = dot.GetComponent<RectTransform>();
        dotRt.anchorMin = new Vector2(0.5f, 0.5f);
        dotRt.anchorMax = new Vector2(0.5f, 0.5f);
        dotRt.pivot = new Vector2(0.5f, 0.5f);
        dotRt.sizeDelta = new Vector2(22, 22);
        dot.SetActive(false);

        Button speed = CreateWideButton(panel.transform, "1x", 30, out TMP_Text speedLabel);
        SetRect(speed.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(220, 64), new Vector2(40, -680));

        TMP_Text _;
        Button skip = CreateWideButton(panel.transform, "SKIP", 30, out _);
        SetRect(skip.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(260, 64), new Vector2(-40, -680));

        GameObject tower = new GameObject("TowerArea");
        tower.transform.SetParent(panel.transform, false);
        RectTransform towerRt = tower.AddComponent<RectTransform>();
        towerRt.anchorMin = new Vector2(0, 0);
        towerRt.anchorMax = new Vector2(1, 1);
        towerRt.offsetMin = new Vector2(40, 20);
        towerRt.offsetMax = new Vector2(-40, -756);

        GameObject rowTpl = BuildTimingRowTemplate(tower.transform);
        rowTpl.SetActive(false);

        ctrl.racePanel = panel;
        ctrl.trackArea = trackRt;
        ctrl.dotTemplate = dot;
        ctrl.towerArea = towerRt;
        ctrl.towerRowTemplate = rowTpl;
        ctrl.speedButton = speed;
        ctrl.speedLabel = speedLabel;
        ctrl.skipButton = skip;
    }

    private static GameObject BuildTimingRowTemplate(Transform parent)
    {
        GameObject go = new GameObject("TimingRow");
        go.transform.SetParent(parent, false);
        Image bg = go.AddComponent<Image>();
        bg.sprite = UISprite();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.15f, 0.15f, 0.24f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 40);
        rt.anchoredPosition = Vector2.zero;

        TimingRow tr = go.AddComponent<TimingRow>();

        GameObject sw = new GameObject("Swatch");
        sw.transform.SetParent(go.transform, false);
        Image swImg = sw.AddComponent<Image>();
        swImg.color = Color.white;
        SetRect(sw.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 16), new Vector2(16, 0));

        TMP_Text pos = CreateText(go.transform, "Pos", "0", 22, Color.white, TextAlignmentOptions.Left);
        SetRect(pos.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(56, 40), new Vector2(42, 0));
        pos.fontStyle = FontStyles.Bold;

        TMP_Text nm = CreateText(go.transform, "Name", "Name", 22, Color.white, TextAlignmentOptions.Left);
        SetRect(nm.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(420, 40), new Vector2(108, 0));

        TMP_Text gap = CreateText(go.transform, "Gap", "", 20, ColorMuted, TextAlignmentOptions.Right);
        SetRect(gap.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(200, 40), new Vector2(-16, 0));

        tr.bg = bg;
        tr.swatch = swImg;
        tr.posText = pos;
        tr.nameText = nm;
        tr.gapText = gap;
        return go;
    }

    private static void BuildResultsPanel(Transform safe, WeekendController ctrl)
    {
        GameObject panel = BottomBar(safe, "ResultsPanel");

        TMP_Text summary = CreateText(panel.transform, "Summary", "", 26, ColorAccent, TextAlignmentOptions.Center);
        SetRect(summary.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 70), new Vector2(0, -16));
        summary.fontStyle = FontStyles.Bold;

        TMP_Text _;
        Button finish = CreateWideButton(panel.transform, "FINISH", 32, out _);
        SetRect(finish.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(900, 88), new Vector2(0, 16));

        ctrl.resultsPanel = panel;
        ctrl.resultsSummaryText = summary;
        ctrl.finishButton = finish;
    }

    private static void BuildListScroll(Transform safe, WeekendController ctrl)
    {
        GameObject scroll = new GameObject("ListScroll");
        scroll.transform.SetParent(safe, false);
        RectTransform scrollRt = scroll.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(30, 220);
        scrollRt.offsetMax = new Vector2(-30, -160);
        ScrollRect sr = scroll.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 40f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scroll.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        vpRt.pivot = new Vector2(0.5f, 1f);
        viewport.AddComponent<RectMask2D>();
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0);

        GameObject listContent = new GameObject("ListContent");
        listContent.transform.SetParent(viewport.transform, false);
        RectTransform lcRt = listContent.AddComponent<RectTransform>();
        lcRt.anchorMin = new Vector2(0, 1);
        lcRt.anchorMax = new Vector2(1, 1);
        lcRt.pivot = new Vector2(0.5f, 1);
        lcRt.sizeDelta = Vector2.zero;
        lcRt.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup vlg = listContent.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10;
        vlg.padding = new RectOffset(0, 0, 0, 8);
        ContentSizeFitter csf = listContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.viewport = vpRt;
        sr.content = lcRt;

        GameObject row = BuildPersonRowTemplate(listContent.transform);
        row.SetActive(false);

        ctrl.listScroll = scroll;
        ctrl.listContent = lcRt;
        ctrl.rowTemplate = row;
    }

    private static GameObject BuildPersonRowTemplate(Transform parent)
    {
        GameObject go = new GameObject("Row");
        go.transform.SetParent(parent, false);
        Image bg = go.AddComponent<Image>();
        bg.sprite = UISprite();
        bg.type = Image.Type.Sliced;
        bg.color = ColorCard;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 104;
        le.minHeight = 104;
        Button rowBtn = go.AddComponent<Button>();
        rowBtn.targetGraphic = bg;
        SetWhiteColors(rowBtn);
        go.AddComponent<ButtonPunch>();
        PersonRow pr = go.AddComponent<PersonRow>();

        GameObject tag = new GameObject("Tag");
        tag.transform.SetParent(go.transform, false);
        Image tagBg = tag.AddComponent<Image>();
        tagBg.sprite = UISprite();
        tagBg.type = Image.Type.Sliced;
        tagBg.color = ColorPrimary;
        SetRect(tag.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(130, 32), new Vector2(20, -12));
        TMP_Text tagText = CreateText(tag.transform, "TagText", "TAG", 18, Color.white, TextAlignmentOptions.Center);
        RectTransform tgrt = tagText.rectTransform;
        tgrt.anchorMin = Vector2.zero;
        tgrt.anchorMax = Vector2.one;
        tgrt.offsetMin = Vector2.zero;
        tgrt.offsetMax = Vector2.zero;
        tagText.fontStyle = FontStyles.Bold;

        TMP_Text nameText = CreateText(go.transform, "Name", "Name", 32, Color.white, TextAlignmentOptions.Left);
        SetRect(nameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(620, 38), new Vector2(20, -46));
        nameText.fontStyle = FontStyles.Bold;

        TMP_Text subtitle = CreateText(go.transform, "Subtitle", "Subtitle", 22, ColorMuted, TextAlignmentOptions.Left);
        SetRect(subtitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(640, 28), new Vector2(20, -82));

        TMP_Text rating = CreateText(go.transform, "Rating", "0", 42, ColorPrimary, TextAlignmentOptions.Right);
        SetRect(rating.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(120, 52), new Vector2(-24, -14));
        rating.fontStyle = FontStyles.Bold;

        TMP_Text actionLabel;
        Button actionBtn = CreatePillButton(go.transform, "Action", 26, out actionLabel);
        SetRect(actionBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(260, 56), new Vector2(-24, 14));

        pr.nameText = nameText;
        pr.subtitleText = subtitle;
        pr.tagText = tagText;
        pr.tagBg = tagBg;
        pr.ratingText = rating;
        pr.rowButton = rowBtn;
        pr.actionButton = actionBtn;
        pr.actionLabel = actionLabel;
        return go;
    }

    private static Button CreateWideButton(Transform parent, string text, float fontSize, out TMP_Text label)
    {
        return CreatePillButton(parent, text, fontSize, out label);
    }

    private static Button CreateIconButton(Transform parent, string name, string icon, Color iconColor)
    {
        Button btn = CreateButton(parent, name, new Vector2(86, 86), new Color(0.18f, 0.18f, 0.28f));
        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(btn.transform, false);
        ico.AddComponent<Image>();
        IconImage ii = ico.AddComponent<IconImage>();
        ii.iconName = icon;
        ii.iconColor = iconColor;
        RectTransform irt = ico.GetComponent<RectTransform>();
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(22, 22);
        irt.offsetMax = new Vector2(-22, -22);
        return btn;
    }

    private static Button CreateButton(Transform parent, string name, Vector2 size, Color bg)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.sprite = UISprite();
        img.type = Image.Type.Sliced;
        img.color = bg;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        SetWhiteColors(btn);
        go.AddComponent<ButtonPunch>();
        return btn;
    }

    private static Button CreatePillButton(Transform parent, string name, float fontSize, out TMP_Text label)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.sprite = UISprite();
        img.type = Image.Type.Sliced;
        img.color = ColorPrimary;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(280, 60);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.45f, 0.45f, 0.5f);
        btn.colors = cb;
        go.AddComponent<ButtonPunch>();

        label = CreateText(go.transform, "Label", name, fontSize, Color.white, TextAlignmentOptions.Center);
        RectTransform lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        label.fontStyle = FontStyles.Bold;
        return btn;
    }

    private static void SetWhiteColors(Button btn)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.selectedColor = Color.white;
        cb.disabledColor = Color.white;
        btn.colors = cb;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 80);
        return t;
    }

    private static GameObject CreateImage(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(100, 100);
        return go;
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private static void CreateCamera()
    {
        if (Object.FindObjectOfType<Camera>() != null) return;
        GameObject go = new GameObject("Main Camera");
        Camera cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColorBg;
        cam.orthographic = true;
        go.tag = "MainCamera";
        go.AddComponent<AudioListener>();
    }

    private static void CreateManagers()
    {
        GameObject go = new GameObject("Managers");
        go.AddComponent<GameManager>();
        go.AddComponent<SoundManager>();
        go.AddComponent<HapticManager>();
        go.AddComponent<TransitionManager>();
        go.AddComponent<SaveManager>();
    }

    private static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Sprite UISprite()
    {
        if (uiSprite == null) uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return uiSprite;
    }

    private static Sprite KnobSprite()
    {
        if (knobSprite == null) knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        return knobSprite;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static void AddScenesToBuild(string[] paths)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < paths.Length; i++)
            scenes.Add(new EditorBuildSettingsScene(paths[i], true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
