using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Iteration3Setup
{
    private static Sprite uiSprite;

    private static readonly Color ColorBg = new Color(0.10f, 0.10f, 0.18f);
    private static readonly Color ColorPrimary = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ColorAccent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.20f);
    private static readonly Color ColorCard = new Color(0.16f, 0.16f, 0.26f);
    private static readonly Color ColorClose = new Color(0.9f, 0.3f, 0.3f);
    private static readonly Color ColorMuted = new Color(0.6f, 0.6f, 0.66f);

    [MenuItem("RacingGame/Iteration 3 - Staff Management")]
    public static void Build()
    {
        EnsureFolder("Assets/RacingGame/Scenes");

        Scene staff = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildStaffScene();
        string staffPath = "Assets/RacingGame/Scenes/Staff.unity";
        EditorSceneManager.SaveScene(staff, staffPath);

        List<string> scenes = new List<string>();
        string menuPath = "Assets/RacingGame/Scenes/MainMenu.unity";
        string hubPath = "Assets/RacingGame/Scenes/Hub.unity";
        if (File.Exists(menuPath)) scenes.Add(menuPath);
        if (File.Exists(hubPath)) scenes.Add(hubPath);
        scenes.Add(staffPath);
        AddScenesToBuild(scenes.ToArray());

        EditorSceneManager.OpenScene(staffPath);
        AssetDatabase.SaveAssets();
        Debug.Log("RacingGame Iteration 3 (Staff Management) setup complete. Open Staff scene and Play, or enter via Hub > STAFF.");
    }

    private static void BuildStaffScene()
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

        GameObject ctrlGo = new GameObject("StaffController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        ctrlGo.AddComponent<RectTransform>();
        StaffController ctrl = ctrlGo.AddComponent<StaffController>();

        BuildTopBar(safe.transform, ctrl);
        BuildSubTabs(safe.transform, ctrl);
        BuildList(safe.transform, ctrl);

        PersonDetailPopup popup = BuildDetailPopup(canvas.transform);
        ctrl.detailPopup = popup;
    }

    private static void BuildTopBar(Transform safe, StaffController ctrl)
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

        TMP_Text title = CreateText(hud.transform, "Title", "STAFF", 46, Color.white, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(360, 60), new Vector2(0, -34));
        title.fontStyle = FontStyles.Bold;

        GameObject chip = new GameObject("MoneyChip");
        chip.transform.SetParent(hud.transform, false);
        Image chipBg = chip.AddComponent<Image>();
        chipBg.sprite = UISprite();
        chipBg.type = Image.Type.Sliced;
        chipBg.color = new Color(0.18f, 0.18f, 0.28f);
        SetRect(chip.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(300, 86), new Vector2(-20, -16));

        GameObject coin = new GameObject("Icon");
        coin.transform.SetParent(chip.transform, false);
        coin.AddComponent<Image>();
        IconImage ci = coin.AddComponent<IconImage>();
        ci.iconName = "money";
        ci.iconColor = ColorAccent;
        SetRect(coin.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(52, 52), new Vector2(14, 0));

        TMP_Text money = CreateText(chip.transform, "Value", "$0", 34, Color.white, TextAlignmentOptions.Left);
        RectTransform mrt = money.rectTransform;
        mrt.anchorMin = new Vector2(0, 0);
        mrt.anchorMax = new Vector2(1, 1);
        mrt.offsetMin = new Vector2(74, 0);
        mrt.offsetMax = new Vector2(-12, 0);
        money.fontStyle = FontStyles.Bold;

        ResourceCounter rc = chip.AddComponent<ResourceCounter>();
        rc.label = money;
        rc.moneyFormat = true;

        ctrl.backButton = back;
        ctrl.moneyCounter = rc;
    }

    private static void BuildSubTabs(Transform safe, StaffController ctrl)
    {
        GameObject bar = new GameObject("SubTabs");
        bar.transform.SetParent(safe, false);
        Image barBg = bar.AddComponent<Image>();
        barBg.sprite = UISprite();
        barBg.type = Image.Type.Sliced;
        barBg.color = new Color(0.13f, 0.13f, 0.22f);
        SetRect(bar.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 92), new Vector2(0, -150));

        GameObject container = new GameObject("Container");
        container.transform.SetParent(bar.transform, false);
        RectTransform crt = container.AddComponent<RectTransform>();
        crt.anchorMin = Vector2.zero;
        crt.anchorMax = Vector2.one;
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;
        HorizontalLayoutGroup h = container.AddComponent<HorizontalLayoutGroup>();
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.spacing = 0;

        string[] labels = { "DRIVERS", "ENGINEERS", "LEADERS", "MARKET", "ACADEMY" };
        ctrl.subTabs = new StaffSubTab[5];
        ctrl.subTabClickables = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            Button btn;
            StaffSubTab st = BuildSubTab(container.transform, labels[i], out btn);
            ctrl.subTabs[i] = st;
            ctrl.subTabClickables[i] = btn;
        }
    }

    private static StaffSubTab BuildSubTab(Transform parent, string label, out Button btn)
    {
        GameObject go = new GameObject("Tab_" + label);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = true;
        btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        SetWhiteColors(btn);
        go.AddComponent<ButtonPunch>();

        StaffSubTab st = go.AddComponent<StaffSubTab>();

        TMP_Text lbl = CreateText(go.transform, "Label", label, 21, ColorMuted, TextAlignmentOptions.Center);
        RectTransform lrt = lbl.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        lbl.fontStyle = FontStyles.Bold;

        GameObject ind = new GameObject("Indicator");
        ind.transform.SetParent(go.transform, false);
        Image indImg = ind.AddComponent<Image>();
        indImg.color = ColorAccent;
        SetRect(ind.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(70, 5), new Vector2(0, 6));
        indImg.enabled = false;

        st.label = lbl;
        st.indicator = indImg;
        return st;
    }

    private static void BuildList(Transform safe, StaffController ctrl)
    {
        GameObject scroll = new GameObject("Scroll");
        scroll.transform.SetParent(safe, false);
        RectTransform scrollRt = scroll.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(30, 30);
        scrollRt.offsetMax = new Vector2(-30, -250);
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
        vlg.spacing = 12;
        vlg.padding = new RectOffset(0, 0, 0, 8);
        ContentSizeFitter csf = listContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.viewport = vpRt;
        sr.content = lcRt;

        GameObject row = BuildPersonRowTemplate(listContent.transform);
        row.SetActive(false);

        GameObject emptyGo = new GameObject("EmptyLabel");
        emptyGo.transform.SetParent(scroll.transform, false);
        TextMeshProUGUI empty = emptyGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) empty.font = TMP_Settings.defaultFontAsset;
        empty.text = "";
        empty.fontSize = 30;
        empty.color = ColorMuted;
        empty.alignment = TextAlignmentOptions.Center;
        RectTransform ert = empty.rectTransform;
        ert.anchorMin = new Vector2(0.5f, 1);
        ert.anchorMax = new Vector2(0.5f, 1);
        ert.pivot = new Vector2(0.5f, 1);
        ert.sizeDelta = new Vector2(600, 60);
        ert.anchoredPosition = new Vector2(0, -40);
        empty.gameObject.SetActive(false);

        ctrl.listContent = lcRt;
        ctrl.rowTemplate = row;
        ctrl.emptyLabel = empty;
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
        le.preferredHeight = 120;
        le.minHeight = 120;
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
        SetRect(tag.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, 34), new Vector2(20, -14));
        TMP_Text tagText = CreateText(tag.transform, "TagText", "TAG", 18, Color.white, TextAlignmentOptions.Center);
        RectTransform tgrt = tagText.rectTransform;
        tgrt.anchorMin = Vector2.zero;
        tgrt.anchorMax = Vector2.one;
        tgrt.offsetMin = Vector2.zero;
        tgrt.offsetMax = Vector2.zero;
        tagText.fontStyle = FontStyles.Bold;

        TMP_Text nameText = CreateText(go.transform, "Name", "Name", 34, Color.white, TextAlignmentOptions.Left);
        SetRect(nameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(620, 40), new Vector2(20, -52));
        nameText.fontStyle = FontStyles.Bold;

        TMP_Text subtitle = CreateText(go.transform, "Subtitle", "Subtitle", 22, ColorMuted, TextAlignmentOptions.Left);
        SetRect(subtitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(640, 28), new Vector2(20, -92));

        TMP_Text rating = CreateText(go.transform, "Rating", "0", 44, ColorPrimary, TextAlignmentOptions.Right);
        SetRect(rating.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(130, 56), new Vector2(-24, -14));
        rating.fontStyle = FontStyles.Bold;

        TMP_Text actionLabel;
        Button actionBtn = CreatePillButton(go.transform, "Action", 26, out actionLabel);
        SetRect(actionBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(250, 60), new Vector2(-24, 16));

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

    private static PersonDetailPopup BuildDetailPopup(Transform canvas)
    {
        GameObject root = new GameObject("PersonDetailPopup");
        root.transform.SetParent(canvas, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        Stretch(rootRt);
        PersonDetailPopup popup = root.AddComponent<PersonDetailPopup>();

        GameObject backdrop = CreateImage(root.transform, "Backdrop", new Color(0, 0, 0, 0));
        Stretch(backdrop.GetComponent<RectTransform>());
        Image backdropImg = backdrop.GetComponent<Image>();
        backdropImg.raycastTarget = true;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);
        RectTransform cRt = content.AddComponent<RectTransform>();
        SetRect(cRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(940, 980), Vector2.zero);
        Image cImg = content.AddComponent<Image>();
        cImg.sprite = UISprite();
        cImg.type = Image.Type.Sliced;
        cImg.color = ColorPanel;

        Button close = CreateIconButton(content.transform, "CloseButton", "close", ColorClose);
        SetRect(close.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(84, 84), new Vector2(-18, -18));

        TMP_Text nameText = CreateText(content.transform, "Name", "Name", 44, Color.white, TextAlignmentOptions.Left);
        SetRect(nameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(640, 56), new Vector2(40, -36));
        nameText.fontStyle = FontStyles.Bold;

        TMP_Text rating = CreateText(content.transform, "Rating", "0", 52, ColorPrimary, TextAlignmentOptions.Right);
        SetRect(rating.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(150, 64), new Vector2(-118, -30));
        rating.fontStyle = FontStyles.Bold;

        TMP_Text info = CreateText(content.transform, "Info", "Info", 24, ColorMuted, TextAlignmentOptions.Left);
        SetRect(info.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(860, 32), new Vector2(40, -98));

        GameObject stats = new GameObject("Stats");
        stats.transform.SetParent(content.transform, false);
        RectTransform stRt = stats.AddComponent<RectTransform>();
        SetRect(stRt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(860, 440), new Vector2(0, -150));
        VerticalLayoutGroup svlg = stats.AddComponent<VerticalLayoutGroup>();
        svlg.childControlWidth = true;
        svlg.childControlHeight = true;
        svlg.childForceExpandWidth = true;
        svlg.childForceExpandHeight = false;
        svlg.spacing = 8;
        svlg.padding = new RectOffset(0, 0, 0, 0);

        TMP_Text[] statLabels = new TMP_Text[8];
        Image[] statFills = new Image[8];
        TMP_Text[] statValues = new TMP_Text[8];
        for (int i = 0; i < 8; i++)
            BuildStatRow(stats.transform, i, statLabels, statFills, statValues);

        GameObject actions = new GameObject("Actions");
        actions.transform.SetParent(content.transform, false);
        RectTransform aRt = actions.AddComponent<RectTransform>();
        aRt.anchorMin = new Vector2(0, 0);
        aRt.anchorMax = new Vector2(1, 0);
        aRt.pivot = new Vector2(0.5f, 0);
        aRt.sizeDelta = new Vector2(-80, 76);
        aRt.anchoredPosition = new Vector2(0, 34);
        HorizontalLayoutGroup ahlg = actions.AddComponent<HorizontalLayoutGroup>();
        ahlg.childControlWidth = true;
        ahlg.childControlHeight = true;
        ahlg.childForceExpandWidth = true;
        ahlg.childForceExpandHeight = true;
        ahlg.childAlignment = TextAnchor.MiddleCenter;
        ahlg.spacing = 14;

        Button[] actionButtons = new Button[3];
        TMP_Text[] actionLabels = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            TMP_Text lbl;
            Button b = CreatePillButton(actions.transform, "A" + i, 28, out lbl);
            actionButtons[i] = b;
            actionLabels[i] = lbl;
        }

        popup.content = cRt;
        popup.backdrop = backdropImg;
        popup.closeButton = close;
        popup.nameText = nameText;
        popup.infoText = info;
        popup.ratingText = rating;
        popup.statLabels = statLabels;
        popup.statFills = statFills;
        popup.statValues = statValues;
        popup.actionButtons = actionButtons;
        popup.actionLabels = actionLabels;

        root.SetActive(false);
        return popup;
    }

    private static void BuildStatRow(Transform parent, int index, TMP_Text[] labels, Image[] fills, TMP_Text[] values)
    {
        GameObject row = new GameObject("StatRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 44;
        le.minHeight = 44;

        TMP_Text label = CreateText(row.transform, "Label", "STAT", 24, Color.white, TextAlignmentOptions.Left);
        SetRect(label.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(230, 40), new Vector2(0, 0));

        Image fill = CreateBar(row.transform, new Vector2(480, 22), new Vector2(240, 0), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), ColorPrimary);

        TMP_Text val = CreateText(row.transform, "Value", "0", 24, Color.white, TextAlignmentOptions.Right);
        SetRect(val.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(70, 40), new Vector2(0, 0));

        labels[index] = label;
        fills[index] = fill;
        values[index] = val;
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
        rt.sizeDelta = new Vector2(240, 60);
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
