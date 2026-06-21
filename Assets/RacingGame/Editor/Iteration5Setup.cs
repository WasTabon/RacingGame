using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Iteration5Setup
{
    private static Sprite uiSprite;
    private static Sprite knobSprite;

    private static readonly Color ColorBg = new Color(0.10f, 0.10f, 0.18f);
    private static readonly Color ColorPrimary = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ColorAccent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.20f);
    private static readonly Color ColorCard = new Color(0.16f, 0.16f, 0.26f);
    private static readonly Color ColorMuted = new Color(0.6f, 0.6f, 0.66f);

    [MenuItem("RacingGame/Iteration 5 - Car")]
    public static void Build()
    {
        EnsureFolder("Assets/RacingGame/Scenes");

        Scene car = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildCarScene();
        string carPath = "Assets/RacingGame/Scenes/Car.unity";
        EditorSceneManager.SaveScene(car, carPath);

        List<string> scenes = new List<string>();
        string[] prior = { "MainMenu", "Hub", "Staff", "RnD" };
        for (int i = 0; i < prior.Length; i++)
        {
            string p = "Assets/RacingGame/Scenes/" + prior[i] + ".unity";
            if (File.Exists(p)) scenes.Add(p);
        }
        scenes.Add(carPath);
        AddScenesToBuild(scenes.ToArray());

        EditorSceneManager.OpenScene(carPath);
        AssetDatabase.SaveAssets();
        Debug.Log("RacingGame Iteration 5 (Car) setup complete. Open Car scene and Play, or enter via Hub > CAR.");
    }

    private static void BuildCarScene()
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

        GameObject ctrlGo = new GameObject("CarController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        ctrlGo.AddComponent<RectTransform>();
        CarController ctrl = ctrlGo.AddComponent<CarController>();

        BuildTopBar(safe.transform, ctrl);
        BuildSubTabs(safe.transform, ctrl);
        BuildBuild(safe.transform, ctrl);
        BuildSetup(safe.transform, ctrl);
    }

    private static void BuildTopBar(Transform safe, CarController ctrl)
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

        TMP_Text title = CreateText(hud.transform, "Title", "CAR", 46, Color.white, TextAlignmentOptions.Center);
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

    private static void BuildSubTabs(Transform safe, CarController ctrl)
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

        string[] labels = { "BUILD", "SETUP" };
        ctrl.subTabs = new StaffSubTab[2];
        ctrl.subTabClickables = new Button[2];
        for (int i = 0; i < 2; i++)
        {
            Button btn;
            StaffSubTab st = BuildSubTab(container.transform, labels[i], 22, out btn);
            ctrl.subTabs[i] = st;
            ctrl.subTabClickables[i] = btn;
        }
    }

    private static StaffSubTab BuildSubTab(Transform parent, string label, float fontSize, out Button btn)
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

        TMP_Text lbl = CreateText(go.transform, "Label", label, fontSize, ColorMuted, TextAlignmentOptions.Center);
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
        SetRect(ind.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(80, 5), new Vector2(0, 6));
        indImg.enabled = false;

        st.label = lbl;
        st.indicator = indImg;
        return st;
    }

    private static void BuildBuild(Transform safe, CarController ctrl)
    {
        GameObject scroll = new GameObject("BuildScroll");
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

        ctrl.buildContainer = scroll;
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
        SetRect(tag.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, 34), new Vector2(20, -14));
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
        SetRect(actionBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(290, 60), new Vector2(-24, 16));

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

    private static void BuildSetup(Transform safe, CarController ctrl)
    {
        GameObject setup = new GameObject("SetupPanel");
        setup.transform.SetParent(safe, false);
        RectTransform setupRt = setup.AddComponent<RectTransform>();
        setupRt.anchorMin = new Vector2(0, 0);
        setupRt.anchorMax = new Vector2(1, 1);
        setupRt.offsetMin = new Vector2(30, 30);
        setupRt.offsetMax = new Vector2(-30, -250);

        GameObject selBar = new GameObject("CarSelect");
        selBar.transform.SetParent(setup.transform, false);
        Image selBg = selBar.AddComponent<Image>();
        selBg.sprite = UISprite();
        selBg.type = Image.Type.Sliced;
        selBg.color = new Color(0.13f, 0.13f, 0.22f);
        SetRect(selBar.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 80), new Vector2(0, -4));

        GameObject selContainer = new GameObject("Container");
        selContainer.transform.SetParent(selBar.transform, false);
        RectTransform selCrt = selContainer.AddComponent<RectTransform>();
        selCrt.anchorMin = Vector2.zero;
        selCrt.anchorMax = Vector2.one;
        selCrt.offsetMin = Vector2.zero;
        selCrt.offsetMax = Vector2.zero;
        HorizontalLayoutGroup selH = selContainer.AddComponent<HorizontalLayoutGroup>();
        selH.childControlWidth = true;
        selH.childControlHeight = true;
        selH.childForceExpandWidth = true;
        selH.childForceExpandHeight = true;
        selH.childAlignment = TextAnchor.MiddleCenter;
        selH.spacing = 0;

        ctrl.carSelTabs = new StaffSubTab[2];
        ctrl.carSelClickables = new Button[2];
        string[] carLabels = { "Car 1", "Car 2" };
        for (int i = 0; i < 2; i++)
        {
            Button cb;
            StaffSubTab cst = BuildSubTab(selContainer.transform, carLabels[i], 24, out cb);
            ctrl.carSelTabs[i] = cst;
            ctrl.carSelClickables[i] = cb;
        }

        TMP_Text track = CreateText(setup.transform, "TrackInfo", "track", 24, ColorMuted, TextAlignmentOptions.Center);
        SetRect(track.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(940, 36), new Vector2(0, -100));

        ctrl.downforceSlider = BuildSliderRow(setup.transform, "Downforce", -160, out ctrl.downforceValue);
        ctrl.balanceSlider = BuildSliderRow(setup.transform, "Balance", -290, out ctrl.balanceValue);
        ctrl.reliabilitySlider = BuildSliderRow(setup.transform, "Reliability", -420, out ctrl.reliabilityValue);

        TMP_Text rLabel = CreateText(setup.transform, "RatingLabel", "SETUP RATING", 26, ColorMuted, TextAlignmentOptions.Center);
        SetRect(rLabel.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(600, 34), new Vector2(0, -560));

        TMP_Text rNum = CreateText(setup.transform, "RatingNum", "0", 64, ColorPrimary, TextAlignmentOptions.Center);
        SetRect(rNum.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(300, 76), new Vector2(0, -598));
        rNum.fontStyle = FontStyles.Bold;

        Image rFill = CreateBar(setup.transform, new Vector2(900, 26), new Vector2(0, -700), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), ColorPrimary);

        TMP_Text autoLabel;
        Button auto = CreatePillButton(setup.transform, "AUTO SETUP", 30, out autoLabel);
        SetRect(auto.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(900, 84), new Vector2(0, 20));

        ctrl.setupContainer = setup;
        ctrl.setupRatingText = rNum;
        ctrl.setupRatingFill = rFill;
        ctrl.autoSetupButton = auto;
        ctrl.trackInfoText = track;
    }

    private static Slider BuildSliderRow(Transform parent, string label, float y, out TMP_Text valueText)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        SetRect(rowRt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(960, 110), new Vector2(0, y));

        TMP_Text lbl = CreateText(row.transform, "Label", label, 30, Color.white, TextAlignmentOptions.Left);
        SetRect(lbl.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(400, 40), new Vector2(10, -6));

        valueText = CreateText(row.transform, "Value", "50%", 30, ColorPrimary, TextAlignmentOptions.Right);
        SetRect(valueText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(160, 40), new Vector2(-10, -6));
        valueText.fontStyle = FontStyles.Bold;

        Slider slider = CreateSlider(row.transform, new Vector2(940, 50));
        SetRect(slider.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(940, 50), new Vector2(0, 8));
        return slider;
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
        slider.value = 0.5f;
        return slider;
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
