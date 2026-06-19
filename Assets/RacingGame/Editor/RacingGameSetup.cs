using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RacingGameSetup
{
    private static Sprite uiSprite;
    private static Sprite knobSprite;

    private static readonly Color ColorBg = new Color(0.10f, 0.10f, 0.18f);
    private static readonly Color ColorPrimary = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ColorAccent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.20f);
    private static readonly Color ColorCard = new Color(0.16f, 0.16f, 0.26f);
    private static readonly Color ColorClose = new Color(0.9f, 0.3f, 0.3f);

    [MenuItem("RacingGame/Iteration 1 - Foundation & Main Menu")]
    public static void Build()
    {
        EnsureFolder("Assets/RacingGame/Scenes");

        Scene hub = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildHubPlaceholder();
        string hubPath = "Assets/RacingGame/Scenes/Hub.unity";
        EditorSceneManager.SaveScene(hub, hubPath);

        Scene menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildMainMenuScene();
        string menuPath = "Assets/RacingGame/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(menu, menuPath);

        AddScenesToBuild(new string[] { menuPath, hubPath });
        EditorSceneManager.OpenScene(menuPath);
        AssetDatabase.SaveAssets();
        Debug.Log("RacingGame Iteration 1 setup complete. Open MainMenu scene and press Play.");
    }

    private static void BuildHubPlaceholder()
    {
        CreateCamera();
        CreateEventSystem();
        Canvas canvas = CreateCanvas("Canvas");
        GameObject bg = CreateImage(canvas.transform, "Background", ColorBg);
        Stretch(bg.GetComponent<RectTransform>());
        TextMeshProUGUI t = CreateText(canvas.transform, "Placeholder", "HQ HUB\nBuilt in Iteration 2", 60, Color.white, TextAlignmentOptions.Center);
        Stretch(t.rectTransform);
    }

    private static void BuildMainMenuScene()
    {
        CreateCamera();
        CreateEventSystem();
        CreateManagers();

        Canvas canvas = CreateCanvas("Canvas");

        GameObject bg = CreateImage(canvas.transform, "Background", ColorBg);
        Stretch(bg.GetComponent<RectTransform>());

        GameObject accent = CreateImage(canvas.transform, "AccentBar", ColorPrimary);
        RectTransform art = accent.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0, 1);
        art.anchorMax = new Vector2(1, 1);
        art.pivot = new Vector2(0.5f, 1f);
        art.sizeDelta = new Vector2(0, 12);
        art.anchoredPosition = Vector2.zero;

        GameObject safe = new GameObject("SafeArea");
        safe.transform.SetParent(canvas.transform, false);
        RectTransform safeRt = safe.AddComponent<RectTransform>();
        Stretch(safeRt);
        safe.AddComponent<SafeAreaFitter>();

        TextMeshProUGUI logo = CreateText(safe.transform, "Logo", "RACING\nGAME", 130, Color.white, TextAlignmentOptions.Center);
        RectTransform logoRt = logo.rectTransform;
        logoRt.anchorMin = new Vector2(0.5f, 1f);
        logoRt.anchorMax = new Vector2(0.5f, 1f);
        logoRt.pivot = new Vector2(0.5f, 1f);
        logoRt.sizeDelta = new Vector2(900, 360);
        logoRt.anchoredPosition = new Vector2(0, -180);
        logo.fontStyle = FontStyles.Bold;

        TextMeshProUGUI subtitle = CreateText(safe.transform, "Subtitle", "MOTORSPORT MANAGER", 38, ColorPrimary, TextAlignmentOptions.Center);
        RectTransform subRt = subtitle.rectTransform;
        subRt.anchorMin = new Vector2(0.5f, 1f);
        subRt.anchorMax = new Vector2(0.5f, 1f);
        subRt.pivot = new Vector2(0.5f, 1f);
        subRt.sizeDelta = new Vector2(900, 60);
        subRt.anchoredPosition = new Vector2(0, -540);

        Button newCareerBtn = CreateMenuButton(safe.transform, "NewCareerButton", "NEW CAREER", "flag", new Vector2(0, 60));
        Button continueBtn = CreateMenuButton(safe.transform, "ContinueButton", "CONTINUE", "continue", new Vector2(0, -110));
        Button settingsBtn = CreateMenuButton(safe.transform, "SettingsButton", "SETTINGS", "settings", new Vector2(0, -280));
        CanvasGroup continueGroup = continueBtn.gameObject.AddComponent<CanvasGroup>();

        TextMeshProUGUI version = CreateText(safe.transform, "Version", "v0.1 - Iteration 1", 28, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.Center);
        RectTransform vrt = version.rectTransform;
        vrt.anchorMin = new Vector2(0.5f, 0f);
        vrt.anchorMax = new Vector2(0.5f, 0f);
        vrt.pivot = new Vector2(0.5f, 0f);
        vrt.sizeDelta = new Vector2(600, 40);
        vrt.anchoredPosition = new Vector2(0, 40);

        DifficultyPopup diffPopup = BuildDifficultyPopup(canvas.transform);
        SettingsPopup setPopup = BuildSettingsPopup(canvas.transform);

        GameObject uiGo = new GameObject("MainMenuUI");
        uiGo.transform.SetParent(canvas.transform, false);
        uiGo.AddComponent<RectTransform>();
        MainMenuUI menuUI = uiGo.AddComponent<MainMenuUI>();
        menuUI.newCareerButton = newCareerBtn;
        menuUI.continueButton = continueBtn;
        menuUI.settingsButton = settingsBtn;
        menuUI.difficultyPopup = diffPopup;
        menuUI.settingsPopup = setPopup;
        menuUI.continueGroup = continueGroup;
    }

    private static DifficultyPopup BuildDifficultyPopup(Transform canvas)
    {
        GameObject root = new GameObject("DifficultyPopup");
        root.transform.SetParent(canvas, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        Stretch(rootRt);
        DifficultyPopup popup = root.AddComponent<DifficultyPopup>();

        GameObject backdrop = CreateImage(root.transform, "Backdrop", new Color(0, 0, 0, 0));
        Stretch(backdrop.GetComponent<RectTransform>());
        Image backdropImg = backdrop.GetComponent<Image>();
        backdropImg.raycastTarget = true;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.pivot = new Vector2(0.5f, 0.5f);
        contentRt.sizeDelta = new Vector2(920, 1180);
        Image panelImg = content.AddComponent<Image>();
        panelImg.sprite = UISprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = ColorPanel;

        TextMeshProUGUI title = CreateText(content.transform, "Title", "SELECT DIFFICULTY", 54, Color.white, TextAlignmentOptions.Center);
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(820, 90);
        titleRt.anchoredPosition = new Vector2(0, -50);
        title.fontStyle = FontStyles.Bold;

        GameObject grid = new GameObject("Grid");
        grid.transform.SetParent(content.transform, false);
        RectTransform gridRt = grid.AddComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0.5f, 0.5f);
        gridRt.anchorMax = new Vector2(0.5f, 0.5f);
        gridRt.pivot = new Vector2(0.5f, 0.5f);
        gridRt.sizeDelta = new Vector2(820, 900);
        gridRt.anchoredPosition = new Vector2(0, -40);
        GridLayoutGroup g = grid.AddComponent<GridLayoutGroup>();
        g.cellSize = new Vector2(390, 420);
        g.spacing = new Vector2(30, 30);
        g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        g.constraintCount = 2;
        g.childAlignment = TextAnchor.MiddleCenter;

        Button easy = CreateDifficultyCard(grid.transform, "EASY", "$150M", "easy", new Color(0.3f, 0.8f, 0.4f));
        Button normal = CreateDifficultyCard(grid.transform, "NORMAL", "$80M", "normal", ColorPrimary);
        Button hard = CreateDifficultyCard(grid.transform, "HARD", "$40M", "hard", ColorAccent);
        Button extreme = CreateDifficultyCard(grid.transform, "EXTREME", "$15M", "extreme", ColorClose);

        Button close = CreateIconButton(content.transform, "CloseButton", "close", ColorClose, true);
        RectTransform closeRt = close.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 1);
        closeRt.anchorMax = new Vector2(1, 1);
        closeRt.pivot = new Vector2(1, 1);
        closeRt.anchoredPosition = new Vector2(-20, -20);

        popup.content = contentRt;
        popup.backdrop = backdropImg;
        popup.easyButton = easy;
        popup.normalButton = normal;
        popup.hardButton = hard;
        popup.extremeButton = extreme;
        popup.closeButton = close;

        root.SetActive(false);
        return popup;
    }

    private static SettingsPopup BuildSettingsPopup(Transform canvas)
    {
        GameObject root = new GameObject("SettingsPopup");
        root.transform.SetParent(canvas, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        Stretch(rootRt);
        SettingsPopup popup = root.AddComponent<SettingsPopup>();

        GameObject backdrop = CreateImage(root.transform, "Backdrop", new Color(0, 0, 0, 0));
        Stretch(backdrop.GetComponent<RectTransform>());
        Image backdropImg = backdrop.GetComponent<Image>();
        backdropImg.raycastTarget = true;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.pivot = new Vector2(0.5f, 0.5f);
        contentRt.sizeDelta = new Vector2(880, 700);
        Image panelImg = content.AddComponent<Image>();
        panelImg.sprite = UISprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = ColorPanel;

        TextMeshProUGUI title = CreateText(content.transform, "Title", "SETTINGS", 54, Color.white, TextAlignmentOptions.Center);
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(820, 90);
        titleRt.anchoredPosition = new Vector2(0, -50);
        title.fontStyle = FontStyles.Bold;

        Slider sfxSlider;
        Button sfxMute;
        Image sfxMuteIcon;
        CreateSettingsRow(content.transform, "SOUND", "sfx", new Vector2(0, -40), out sfxSlider, out sfxMute, out sfxMuteIcon);

        Slider musicSlider;
        Button musicMute;
        Image musicMuteIcon;
        CreateSettingsRow(content.transform, "MUSIC", "music", new Vector2(0, -240), out musicSlider, out musicMute, out musicMuteIcon);

        Button close = CreateIconButton(content.transform, "CloseButton", "close", ColorClose, true);
        RectTransform closeRt = close.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 1);
        closeRt.anchorMax = new Vector2(1, 1);
        closeRt.pivot = new Vector2(1, 1);
        closeRt.anchoredPosition = new Vector2(-20, -20);

        popup.content = contentRt;
        popup.backdrop = backdropImg;
        popup.sfxSlider = sfxSlider;
        popup.musicSlider = musicSlider;
        popup.sfxMuteButton = sfxMute;
        popup.musicMuteButton = musicMute;
        popup.sfxMuteIcon = sfxMuteIcon;
        popup.musicMuteIcon = musicMuteIcon;
        popup.closeButton = close;

        root.SetActive(false);
        return popup;
    }

    private static void CreateSettingsRow(Transform parent, string label, string icon, Vector2 anchoredPos, out Slider slider, out Button muteBtn, out Image muteIcon)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.pivot = new Vector2(0.5f, 0.5f);
        rowRt.sizeDelta = new Vector2(780, 150);
        rowRt.anchoredPosition = anchoredPos;

        TextMeshProUGUI lbl = CreateText(row.transform, "Label", label, 38, Color.white, TextAlignmentOptions.Left);
        RectTransform lblRt = lbl.rectTransform;
        lblRt.anchorMin = new Vector2(0, 1);
        lblRt.anchorMax = new Vector2(0, 1);
        lblRt.pivot = new Vector2(0, 1);
        lblRt.sizeDelta = new Vector2(400, 50);
        lblRt.anchoredPosition = new Vector2(20, -10);

        slider = CreateSlider(row.transform, new Vector2(560, 50));
        RectTransform srt = slider.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0);
        srt.anchorMax = new Vector2(0, 0);
        srt.pivot = new Vector2(0, 0);
        srt.anchoredPosition = new Vector2(20, 30);

        muteBtn = CreateIconButton(row.transform, "MuteButton", icon, ColorPrimary, false);
        RectTransform mrt = muteBtn.GetComponent<RectTransform>();
        mrt.anchorMin = new Vector2(1, 0);
        mrt.anchorMax = new Vector2(1, 0);
        mrt.pivot = new Vector2(1, 0);
        mrt.sizeDelta = new Vector2(90, 90);
        mrt.anchoredPosition = new Vector2(-10, 25);
        muteIcon = muteBtn.transform.Find("Icon").GetComponent<Image>();
        muteIcon.sprite = IconFactory.Get(icon, Color.white);
        muteIcon.color = ColorPrimary;
        muteIcon.preserveAspect = true;
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, string icon, Vector2 anchoredPos)
    {
        Button btn = CreateButton(parent, name, new Vector2(680, 130), ColorPrimary);
        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(btn.transform, false);
        ico.AddComponent<Image>();
        IconImage ii = ico.AddComponent<IconImage>();
        ii.iconName = icon;
        ii.iconColor = Color.white;
        RectTransform irt = ico.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0, 0.5f);
        irt.anchorMax = new Vector2(0, 0.5f);
        irt.pivot = new Vector2(0, 0.5f);
        irt.sizeDelta = new Vector2(80, 80);
        irt.anchoredPosition = new Vector2(40, 0);

        TextMeshProUGUI t = CreateText(btn.transform, "Label", label, 48, Color.white, TextAlignmentOptions.Center);
        RectTransform trt = t.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(140, 0);
        trt.offsetMax = new Vector2(-40, 0);
        t.fontStyle = FontStyles.Bold;
        return btn;
    }

    private static Button CreateDifficultyCard(Transform parent, string title, string subtitle, string icon, Color accent)
    {
        Button btn = CreateButton(parent, title + "Card", new Vector2(390, 420), ColorCard);

        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(btn.transform, false);
        ico.AddComponent<Image>();
        IconImage ii = ico.AddComponent<IconImage>();
        ii.iconName = icon;
        ii.iconColor = accent;
        RectTransform irt = ico.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.5f, 0.5f);
        irt.anchorMax = new Vector2(0.5f, 0.5f);
        irt.pivot = new Vector2(0.5f, 0.5f);
        irt.sizeDelta = new Vector2(150, 150);
        irt.anchoredPosition = new Vector2(0, 90);

        TextMeshProUGUI t = CreateText(btn.transform, "Title", title, 46, Color.white, TextAlignmentOptions.Center);
        RectTransform trt = t.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(360, 60);
        trt.anchoredPosition = new Vector2(0, -50);
        t.fontStyle = FontStyles.Bold;

        TextMeshProUGUI s = CreateText(btn.transform, "Budget", subtitle, 40, accent, TextAlignmentOptions.Center);
        RectTransform srt = s.rectTransform;
        srt.anchorMin = new Vector2(0.5f, 0.5f);
        srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(360, 50);
        srt.anchoredPosition = new Vector2(0, -130);
        s.fontStyle = FontStyles.Bold;

        return btn;
    }

    private static Button CreateIconButton(Transform parent, string name, string icon, Color iconColor, bool useIconComponent)
    {
        Button btn = CreateButton(parent, name, new Vector2(90, 90), new Color(0.18f, 0.18f, 0.28f));
        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(btn.transform, false);
        ico.AddComponent<Image>();
        if (useIconComponent)
        {
            IconImage ii = ico.AddComponent<IconImage>();
            ii.iconName = icon;
            ii.iconColor = iconColor;
        }
        RectTransform irt = ico.GetComponent<RectTransform>();
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(22, 22);
        irt.offsetMax = new Vector2(-22, -22);
        return btn;
    }

    private static Slider CreateSlider(Transform parent, Vector2 size)
    {
        GameObject go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        Slider slider = go.AddComponent<Slider>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = UISprite();
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.08f, 0.08f, 0.14f);
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0, 0.5f);
        bgrt.anchorMax = new Vector2(1, 0.5f);
        bgrt.pivot = new Vector2(0.5f, 0.5f);
        bgrt.sizeDelta = new Vector2(0, 18);
        bgrt.anchoredPosition = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fart = fillArea.AddComponent<RectTransform>();
        fart.anchorMin = new Vector2(0, 0.5f);
        fart.anchorMax = new Vector2(1, 0.5f);
        fart.pivot = new Vector2(0.5f, 0.5f);
        fart.sizeDelta = new Vector2(-30, 18);
        fart.anchoredPosition = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.sprite = UISprite();
        fillImg.type = Image.Type.Sliced;
        fillImg.color = ColorPrimary;
        RectTransform fillrt = fill.GetComponent<RectTransform>();
        fillrt.anchorMin = new Vector2(0, 0);
        fillrt.anchorMax = new Vector2(0, 1);
        fillrt.pivot = new Vector2(0.5f, 0.5f);
        fillrt.sizeDelta = new Vector2(30, 0);

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        RectTransform hart = handleArea.AddComponent<RectTransform>();
        hart.anchorMin = new Vector2(0, 0);
        hart.anchorMax = new Vector2(1, 1);
        hart.pivot = new Vector2(0.5f, 0.5f);
        hart.sizeDelta = new Vector2(-30, 0);
        hart.anchoredPosition = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.sprite = KnobSprite();
        handleImg.color = Color.white;
        RectTransform hrt = handle.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(48, 48);

        slider.fillRect = fillrt;
        slider.handleRect = hrt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
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
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.selectedColor = Color.white;
        cb.disabledColor = Color.white;
        btn.colors = cb;
        go.AddComponent<ButtonPunch>();
        return btn;
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
