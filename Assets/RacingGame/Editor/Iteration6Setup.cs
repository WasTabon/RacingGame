using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Iteration6Setup
{
    private static Sprite uiSprite;

    private static readonly Color ColorBg = new Color(0.10f, 0.10f, 0.18f);
    private static readonly Color ColorPrimary = new Color(0.29f, 0.56f, 0.89f);
    private static readonly Color ColorAccent = new Color(0.96f, 0.65f, 0.14f);
    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.20f);
    private static readonly Color ColorCard = new Color(0.16f, 0.16f, 0.26f);
    private static readonly Color ColorMuted = new Color(0.6f, 0.6f, 0.66f);

    [MenuItem("RacingGame/Iteration 6 - Facilities")]
    public static void Build()
    {
        EnsureFolder("Assets/RacingGame/Scenes");

        Scene baseScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildBaseScene();
        string basePath = "Assets/RacingGame/Scenes/Base.unity";
        EditorSceneManager.SaveScene(baseScene, basePath);

        List<string> scenes = new List<string>();
        string[] prior = { "MainMenu", "Hub", "Staff", "RnD", "Car" };
        for (int i = 0; i < prior.Length; i++)
        {
            string p = "Assets/RacingGame/Scenes/" + prior[i] + ".unity";
            if (File.Exists(p)) scenes.Add(p);
        }
        scenes.Add(basePath);
        AddScenesToBuild(scenes.ToArray());

        EditorSceneManager.OpenScene(basePath);
        AssetDatabase.SaveAssets();
        Debug.Log("RacingGame Iteration 6 (Facilities) setup complete. Open Base scene and Play, or enter via Hub > BASE.");
    }

    private static void BuildBaseScene()
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

        GameObject ctrlGo = new GameObject("BaseController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        ctrlGo.AddComponent<RectTransform>();
        BaseController ctrl = ctrlGo.AddComponent<BaseController>();

        BuildTopBar(safe.transform, ctrl);
        BuildList(safe.transform, ctrl);
    }

    private static void BuildTopBar(Transform safe, BaseController ctrl)
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

        TMP_Text title = CreateText(hud.transform, "Title", "FACILITIES", 42, Color.white, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(420, 60), new Vector2(0, -34));
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

    private static void BuildList(Transform safe, BaseController ctrl)
    {
        GameObject scroll = new GameObject("Scroll");
        scroll.transform.SetParent(safe, false);
        RectTransform scrollRt = scroll.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(30, 30);
        scrollRt.offsetMax = new Vector2(-30, -170);
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
