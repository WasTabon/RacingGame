using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Iteration2Setup
{
    private static Sprite uiSprite;
    private static Sprite knobSprite;

    private static readonly Color ColorBg = new Color(0.10f, 0.10f, 0.18f);
    private static readonly Color ColorPrimary = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ColorAccent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.20f);
    private static readonly Color ColorCard = new Color(0.16f, 0.16f, 0.26f);
    private static readonly Color ColorClose = new Color(0.9f, 0.3f, 0.3f);
    private static readonly Color ColorMuted = new Color(0.6f, 0.6f, 0.66f);

    [MenuItem("RacingGame/Iteration 2 - HQ Hub")]
    public static void Build()
    {
        EnsureFolder("Assets/RacingGame/Scenes");

        Scene hub = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildHubScene();
        string hubPath = "Assets/RacingGame/Scenes/Hub.unity";
        EditorSceneManager.SaveScene(hub, hubPath);

        string menuPath = "Assets/RacingGame/Scenes/MainMenu.unity";
        if (File.Exists(menuPath))
            AddScenesToBuild(new string[] { menuPath, hubPath });
        else
            AddScenesToBuild(new string[] { hubPath });

        EditorSceneManager.OpenScene(hubPath);
        AssetDatabase.SaveAssets();
        Debug.Log("RacingGame Iteration 2 (HQ Hub) setup complete. Open Hub scene and press Play, or run from MainMenu.");
    }

    private static void BuildHubScene()
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

        GameObject hubGo = new GameObject("HubController");
        hubGo.transform.SetParent(canvas.transform, false);
        hubGo.AddComponent<RectTransform>();
        HubController hub = hubGo.AddComponent<HubController>();

        GameObject content = new GameObject("Content");
        content.transform.SetParent(safe.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 0);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.offsetMin = new Vector2(0, 160);
        contentRt.offsetMax = new Vector2(0, -160);

        hub.tabPanels = new CanvasGroup[5];
        hub.tabPanels[0] = BuildTeamPanel(content.transform, hub);
        hub.tabPanels[1] = BuildPlaceholderPanel(content.transform, "staff", "STAFF", "Coming in Iteration 3");
        hub.tabPanels[2] = BuildPlaceholderPanel(content.transform, "research", "R&D", "Coming in Iteration 4");
        hub.tabPanels[3] = BuildPlaceholderPanel(content.transform, "car", "CAR", "Coming in Iteration 5");
        hub.tabPanels[4] = BuildPlaceholderPanel(content.transform, "facility", "BASE", "Coming in Iteration 6");

        BuildTopBar(safe.transform, hub);
        BuildBottomNav(safe.transform, hub);

        SettingsPopup settings = BuildSettingsPopup(canvas.transform);
        CalendarPopup calendar = BuildCalendarPopup(canvas.transform);
        hub.settingsPopup = settings;
        hub.calendarPopup = calendar;
    }

    private static void BuildTopBar(Transform safe, HubController hub)
    {
        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(safe, false);
        Image hudBg = hud.AddComponent<Image>();
        hudBg.sprite = UISprite();
        hudBg.type = Image.Type.Sliced;
        hudBg.color = ColorPanel;
        SetRect(hud.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 160), Vector2.zero);

        ResourceCounter money = CreateChip(hud.transform, "MoneyChip", "money", ColorAccent, new Vector2(20, -16), new Vector2(300, 86), true);
        Button financeBtn = money.gameObject.AddComponent<Button>();
        financeBtn.targetGraphic = money.GetComponent<Image>();
        ColorBlock fcb = financeBtn.colors;
        fcb.normalColor = Color.white;
        fcb.highlightedColor = Color.white;
        fcb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        fcb.selectedColor = Color.white;
        fcb.disabledColor = Color.white;
        financeBtn.colors = fcb;
        money.gameObject.AddComponent<ButtonPunch>();
        hub.financeButton = financeBtn;
        ResourceCounter rep = CreateChip(hud.transform, "RepChip", "reputation", new Color(0.96f, 0.78f, 0.25f), new Vector2(336, -16), new Vector2(210, 86), false);
        Button standingsBtn = rep.gameObject.AddComponent<Button>();
        standingsBtn.targetGraphic = rep.GetComponent<Image>();
        ColorBlock scb = standingsBtn.colors;
        scb.normalColor = Color.white;
        scb.highlightedColor = Color.white;
        scb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        scb.selectedColor = Color.white;
        scb.disabledColor = Color.white;
        standingsBtn.colors = scb;
        rep.gameObject.AddComponent<ButtonPunch>();
        hub.standingsButton = standingsBtn;

        Button settingsBtn = CreateIconButton(hud.transform, "SettingsButton", "settings", Color.white, true);
        SetRect(settingsBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(86, 86), new Vector2(-20, -16));

        TMP_Text season = CreateText(hud.transform, "SeasonWeek", "Season", 26, ColorMuted, TextAlignmentOptions.Center);
        SetRect(season.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(900, 36), new Vector2(0, -112));

        hub.moneyCounter = money;
        hub.repCounter = rep;
        hub.seasonWeekText = season;
        hub.settingsButton = settingsBtn;
    }

    private static ResourceCounter CreateChip(Transform parent, string name, string icon, Color iconColor, Vector2 pos, Vector2 size, bool moneyFormat)
    {
        GameObject chip = new GameObject(name);
        chip.transform.SetParent(parent, false);
        Image bg = chip.AddComponent<Image>();
        bg.sprite = UISprite();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.18f, 0.18f, 0.28f);
        SetRect(chip.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), size, pos);

        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(chip.transform, false);
        ico.AddComponent<Image>();
        IconImage ii = ico.AddComponent<IconImage>();
        ii.iconName = icon;
        ii.iconColor = iconColor;
        SetRect(ico.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(52, 52), new Vector2(14, 0));

        TMP_Text txt = CreateText(chip.transform, "Value", "$0", 34, Color.white, TextAlignmentOptions.Left);
        RectTransform trt = txt.rectTransform;
        trt.anchorMin = new Vector2(0, 0);
        trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = new Vector2(74, 0);
        trt.offsetMax = new Vector2(-12, 0);
        txt.fontStyle = FontStyles.Bold;

        ResourceCounter rc = chip.AddComponent<ResourceCounter>();
        rc.label = txt;
        rc.moneyFormat = moneyFormat;
        return rc;
    }

    private static CanvasGroup BuildTeamPanel(Transform parent, HubController hub)
    {
        GameObject panel = new GameObject("Panel_Team");
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        Stretch(rt);
        CanvasGroup cg = panel.AddComponent<CanvasGroup>();

        TMP_Text teamName = CreateText(panel.transform, "TeamName", "Team", 52, Color.white, TextAlignmentOptions.Center);
        SetRect(teamName.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 62), new Vector2(0, -20));
        teamName.fontStyle = FontStyles.Bold;

        TMP_Text philo = CreateText(panel.transform, "Philosophy", "Philosophy", 30, ColorAccent, TextAlignmentOptions.Center);
        SetRect(philo.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 40), new Vector2(0, -86));

        TMP_Text driversLbl = CreateText(panel.transform, "DriversLabel", "DRIVERS", 30, ColorMuted, TextAlignmentOptions.Left);
        SetRect(driversLbl.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(980, 36), new Vector2(0, -150));

        hub.driverCards = new DriverCard[2];
        hub.driverCards[0] = BuildDriverCard(panel.transform, -196);
        hub.driverCards[1] = BuildDriverCard(panel.transform, -446);

        TMP_Text carsLbl = CreateText(panel.transform, "CarsLabel", "CARS", 30, ColorMuted, TextAlignmentOptions.Left);
        SetRect(carsLbl.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(980, 36), new Vector2(0, -700));

        hub.carRows = new CarRow[2];
        hub.carRows[0] = BuildCarRow(panel.transform, -746);
        hub.carRows[1] = BuildCarRow(panel.transform, -834);

        BuildNextRaceCard(panel.transform, -936, hub);

        hub.teamNameText = teamName;
        hub.philosophyText = philo;

        panel.SetActive(true);
        return cg;
    }

    private static DriverCard BuildDriverCard(Transform parent, float y)
    {
        GameObject card = new GameObject("DriverCard");
        card.transform.SetParent(parent, false);
        Image bg = card.AddComponent<Image>();
        bg.sprite = UISprite();
        bg.type = Image.Type.Sliced;
        bg.color = ColorCard;
        SetRect(card.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(980, 230), new Vector2(0, y));
        DriverCard dc = card.AddComponent<DriverCard>();

        TMP_Text nameText = CreateText(card.transform, "Name", "Driver", 40, Color.white, TextAlignmentOptions.Left);
        SetRect(nameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(600, 52), new Vector2(28, -18));
        nameText.fontStyle = FontStyles.Bold;

        TMP_Text info = CreateText(card.transform, "Info", "Age", 26, ColorMuted, TextAlignmentOptions.Left);
        SetRect(info.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(600, 32), new Vector2(30, -74));

        TMP_Text ovr = CreateText(card.transform, "Overall", "0", 56, ColorPrimary, TextAlignmentOptions.Right);
        SetRect(ovr.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(180, 64), new Vector2(-28, -14));
        ovr.fontStyle = FontStyles.Bold;

        TMP_Text ovrLbl = CreateText(card.transform, "OvrLabel", "OVR", 22, ColorMuted, TextAlignmentOptions.Right);
        SetRect(ovrLbl.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(160, 28), new Vector2(-34, -80));

        string[] labels = { "SPD", "QUAL", "CONS" };
        dc.statFills = new Image[3];
        dc.statValues = new TMP_Text[3];
        float startY = -118f;
        float rowH = 38f;
        for (int i = 0; i < 3; i++)
        {
            float ry = startY - i * rowH;
            TMP_Text lbl = CreateText(card.transform, "Lbl" + i, labels[i], 24, ColorMuted, TextAlignmentOptions.Left);
            SetRect(lbl.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(110, 34), new Vector2(28, ry));
            Image fill = CreateBar(card.transform, new Vector2(640, 24), new Vector2(150, ry - 3), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), ColorPrimary);
            TMP_Text val = CreateText(card.transform, "Val" + i, "0", 24, Color.white, TextAlignmentOptions.Right);
            SetRect(val.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(90, 34), new Vector2(-28, ry));
            dc.statFills[i] = fill;
            dc.statValues[i] = val;
        }

        dc.nameText = nameText;
        dc.infoText = info;
        dc.overallText = ovr;
        return dc;
    }

    private static CarRow BuildCarRow(Transform parent, float y)
    {
        GameObject row = new GameObject("CarRow");
        row.transform.SetParent(parent, false);
        Image bg = row.AddComponent<Image>();
        bg.sprite = UISprite();
        bg.type = Image.Type.Sliced;
        bg.color = ColorCard;
        SetRect(row.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(980, 80), new Vector2(0, y));
        CarRow cr = row.AddComponent<CarRow>();

        TMP_Text nameText = CreateText(row.transform, "Name", "Car", 30, Color.white, TextAlignmentOptions.Left);
        SetRect(nameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(400, 34), new Vector2(24, -10));

        TMP_Text val = CreateText(row.transform, "Val", "0", 30, ColorPrimary, TextAlignmentOptions.Right);
        SetRect(val.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(120, 34), new Vector2(-24, -10));
        val.fontStyle = FontStyles.Bold;

        Image fill = CreateBar(row.transform, new Vector2(932, 20), new Vector2(24, 14), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), ColorPrimary);

        cr.nameText = nameText;
        cr.perfFill = fill;
        cr.perfText = val;
        return cr;
    }

    private static void BuildNextRaceCard(Transform parent, float y, HubController hub)
    {
        Button btn = CreateButton(parent, "NextRaceCard", new Vector2(980, 150), ColorCard);
        SetRect(btn.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(980, 150), new Vector2(0, y));

        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(btn.transform, false);
        ico.AddComponent<Image>();
        IconImage ii = ico.AddComponent<IconImage>();
        ii.iconName = "flag";
        ii.iconColor = ColorAccent;
        SetRect(ico.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(90, 90), new Vector2(40, 0));

        TMP_Text round = CreateText(btn.transform, "Round", "ROUND", 26, ColorAccent, TextAlignmentOptions.Left);
        SetRect(round.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(500, 30), new Vector2(160, -22));
        round.fontStyle = FontStyles.Bold;

        TMP_Text track = CreateText(btn.transform, "Track", "Track", 44, Color.white, TextAlignmentOptions.Left);
        SetRect(track.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(620, 52), new Vector2(160, -52));
        track.fontStyle = FontStyles.Bold;

        TMP_Text info = CreateText(btn.transform, "Info", "Info", 24, ColorMuted, TextAlignmentOptions.Left);
        SetRect(info.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(700, 30), new Vector2(160, -108));

        GameObject chev = new GameObject("Chevron");
        chev.transform.SetParent(btn.transform, false);
        chev.AddComponent<Image>();
        IconImage ci = chev.AddComponent<IconImage>();
        ci.iconName = "chevron";
        ci.iconColor = ColorMuted;
        SetRect(chev.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(50, 50), new Vector2(-30, 0));

        hub.nextRaceButton = btn;
        hub.nextRaceRoundText = round;
        hub.nextRaceTrackText = track;
        hub.nextRaceInfoText = info;
    }

    private static CanvasGroup BuildPlaceholderPanel(Transform parent, string icon, string title, string subtitle)
    {
        GameObject panel = new GameObject("Panel_" + title);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        Stretch(rt);
        CanvasGroup cg = panel.AddComponent<CanvasGroup>();

        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(panel.transform, false);
        ico.AddComponent<Image>();
        IconImage ii = ico.AddComponent<IconImage>();
        ii.iconName = icon;
        ii.iconColor = new Color(0.3f, 0.3f, 0.4f);
        SetRect(ico.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(180, 180), new Vector2(0, 120));

        TMP_Text t = CreateText(panel.transform, "Title", title, 60, Color.white, TextAlignmentOptions.Center);
        SetRect(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 70), new Vector2(0, -30));
        t.fontStyle = FontStyles.Bold;

        TMP_Text s = CreateText(panel.transform, "Sub", subtitle, 32, ColorMuted, TextAlignmentOptions.Center);
        SetRect(s.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 44), new Vector2(0, -110));

        panel.SetActive(false);
        return cg;
    }

    private static void BuildBottomNav(Transform safe, HubController hub)
    {
        GameObject nav = new GameObject("BottomNav");
        nav.transform.SetParent(safe, false);
        Image navBg = nav.AddComponent<Image>();
        navBg.sprite = UISprite();
        navBg.type = Image.Type.Sliced;
        navBg.color = ColorPanel;
        SetRect(nav.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 160), Vector2.zero);

        GameObject container = new GameObject("NavContainer");
        container.transform.SetParent(nav.transform, false);
        RectTransform crt = container.AddComponent<RectTransform>();
        crt.anchorMin = Vector2.zero;
        crt.anchorMax = Vector2.one;
        crt.offsetMin = new Vector2(10, 10);
        crt.offsetMax = new Vector2(-10, -10);
        HorizontalLayoutGroup h = container.AddComponent<HorizontalLayoutGroup>();
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.spacing = 0;

        string[] icons = { "home", "staff", "research", "car", "facility" };
        string[] labels = { "HQ", "STAFF", "R&D", "CAR", "BASE" };
        hub.navButtons = new HubNavButton[5];
        hub.navClickables = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            Button btn;
            HubNavButton nb = BuildNavButton(container.transform, icons[i], labels[i], out btn);
            hub.navButtons[i] = nb;
            hub.navClickables[i] = btn;
        }
    }

    private static HubNavButton BuildNavButton(Transform parent, string icon, string label, out Button btn)
    {
        GameObject go = new GameObject("Nav_" + label);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = true;
        btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = Color.white;
        cb.selectedColor = Color.white;
        cb.disabledColor = Color.white;
        btn.colors = cb;
        go.AddComponent<ButtonPunch>();

        HubNavButton nb = go.AddComponent<HubNavButton>();
        nb.iconName = icon;

        GameObject ind = new GameObject("Indicator");
        ind.transform.SetParent(go.transform, false);
        Image indImg = ind.AddComponent<Image>();
        indImg.color = ColorPrimary;
        SetRect(ind.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(46, 6), new Vector2(0, -4));
        indImg.enabled = false;

        GameObject ico = new GameObject("Icon");
        ico.transform.SetParent(go.transform, false);
        Image icoImg = ico.AddComponent<Image>();
        SetRect(ico.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(58, 58), new Vector2(0, 10));

        TMP_Text lbl = CreateText(go.transform, "Label", label, 22, new Color(0.5f, 0.5f, 0.58f), TextAlignmentOptions.Center);
        SetRect(lbl.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(180, 28), new Vector2(0, 14));

        nb.icon = icoImg;
        nb.label = lbl;
        nb.indicator = indImg;
        return nb;
    }

    private static CalendarPopup BuildCalendarPopup(Transform canvas)
    {
        GameObject root = new GameObject("CalendarPopup");
        root.transform.SetParent(canvas, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        Stretch(rootRt);
        CalendarPopup popup = root.AddComponent<CalendarPopup>();

        GameObject backdrop = CreateImage(root.transform, "Backdrop", new Color(0, 0, 0, 0));
        Stretch(backdrop.GetComponent<RectTransform>());
        Image backdropImg = backdrop.GetComponent<Image>();
        backdropImg.raycastTarget = true;

        GameObject contentPanel = new GameObject("Content");
        contentPanel.transform.SetParent(root.transform, false);
        RectTransform cpRt = contentPanel.AddComponent<RectTransform>();
        SetRect(cpRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(920, 1320), Vector2.zero);
        Image panelImg = contentPanel.AddComponent<Image>();
        panelImg.sprite = UISprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = ColorPanel;

        TMP_Text title = CreateText(contentPanel.transform, "Title", "SEASON CALENDAR", 50, Color.white, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(820, 80), new Vector2(0, -46));
        title.fontStyle = FontStyles.Bold;

        Button close = CreateIconButton(contentPanel.transform, "CloseButton", "close", ColorClose, true);
        SetRect(close.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(90, 90), new Vector2(-20, -20));

        GameObject scroll = new GameObject("Scroll");
        scroll.transform.SetParent(contentPanel.transform, false);
        RectTransform scrollRt = scroll.AddComponent<RectTransform>();
        SetRect(scrollRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(840, 1080), new Vector2(0, -40));
        ScrollRect sr = scroll.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 35f;

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
        lcRt.sizeDelta = new Vector2(0, 0);
        lcRt.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup vlg = listContent.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 12;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        ContentSizeFitter csf = listContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.viewport = vpRt;
        sr.content = lcRt;

        GameObject row = BuildCalendarRow(listContent.transform);
        row.SetActive(false);

        popup.content = cpRt;
        popup.backdrop = backdropImg;
        popup.closeButton = close;
        popup.listContent = lcRt;
        popup.rowTemplate = row;

        root.SetActive(false);
        return popup;
    }

    private static GameObject BuildCalendarRow(Transform parent)
    {
        GameObject row = new GameObject("Row");
        row.transform.SetParent(parent, false);
        Image bg = row.AddComponent<Image>();
        bg.sprite = UISprite();
        bg.type = Image.Type.Sliced;
        bg.color = ColorCard;
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 92;
        le.minHeight = 92;
        CalendarRow cr = row.AddComponent<CalendarRow>();

        TMP_Text round = CreateText(row.transform, "Round", "R1", 34, ColorAccent, TextAlignmentOptions.Left);
        SetRect(round.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 50), new Vector2(22, 0));
        round.fontStyle = FontStyles.Bold;

        TMP_Text track = CreateText(row.transform, "Track", "Track", 30, Color.white, TextAlignmentOptions.Left);
        SetRect(track.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(420, 36), new Vector2(130, -14));

        TMP_Text country = CreateText(row.transform, "Country", "Country", 22, ColorMuted, TextAlignmentOptions.Left);
        SetRect(country.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(420, 28), new Vector2(130, 14));

        TMP_Text info = CreateText(row.transform, "Info", "Info", 22, ColorMuted, TextAlignmentOptions.Right);
        SetRect(info.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(340, 30), new Vector2(-22, 0));

        cr.roundText = round;
        cr.trackText = track;
        cr.countryText = country;
        cr.infoText = info;
        return row;
    }

    private static Image CreateBar(Transform parent, Vector2 size, Vector2 pos, Vector2 aMin, Vector2 aMax, Vector2 pivot, Color fillColor)
    {
        GameObject bg = new GameObject("Bar");
        bg.transform.SetParent(parent, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = UISprite();
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.08f, 0.08f, 0.14f);
        SetRect(bg.GetComponent<RectTransform>(), aMin, aMax, pivot, size, pos);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(bg.transform, false);
        Image fImg = fill.AddComponent<Image>();
        fImg.sprite = UISprite();
        fImg.type = Image.Type.Filled;
        fImg.fillMethod = Image.FillMethod.Horizontal;
        fImg.fillOrigin = 0;
        fImg.fillAmount = 1f;
        fImg.color = fillColor;
        RectTransform frt = fill.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(3, 3);
        frt.offsetMax = new Vector2(-3, -3);
        return fImg;
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
        SetRect(contentRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(880, 700), Vector2.zero);
        Image panelImg = content.AddComponent<Image>();
        panelImg.sprite = UISprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = ColorPanel;

        TMP_Text title = CreateText(content.transform, "Title", "SETTINGS", 54, Color.white, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(820, 90), new Vector2(0, -50));
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
        SetRect(close.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(90, 90), new Vector2(-20, -20));

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
        SetRect(rowRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(780, 150), anchoredPos);

        TMP_Text lbl = CreateText(row.transform, "Label", label, 38, Color.white, TextAlignmentOptions.Left);
        SetRect(lbl.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(400, 50), new Vector2(20, -10));

        slider = CreateSlider(row.transform, new Vector2(560, 50));
        SetRect(slider.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(560, 50), new Vector2(20, 30));

        muteBtn = CreateIconButton(row.transform, "MuteButton", icon, ColorPrimary, false);
        SetRect(muteBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(90, 90), new Vector2(-10, 25));
        muteIcon = muteBtn.transform.Find("Icon").GetComponent<Image>();
        muteIcon.sprite = IconFactory.Get(icon, Color.white);
        muteIcon.color = ColorPrimary;
        muteIcon.preserveAspect = true;
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
