using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds El Mandoob progression and garage controls to the existing menu scene
/// without requiring fragile hand edits to the original menu prefab hierarchy.
/// </summary>
public class ElMandoobMenuPanel : MonoBehaviour
{
    private GameData data;
    private TextMeshProUGUI profileText;
    private TextMeshProUGUI storyText;
    private TextMeshProUGUI messageText;
    private Button speedButton;
    private Button enduranceButton;
    private TextMeshProUGUI speedButtonText;
    private TextMeshProUGUI enduranceButtonText;
    private float nextRefreshAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryCreate(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreate(scene);
    }

    private static void TryCreate(Scene scene)
    {
        if (!scene.IsValid() || !string.Equals(scene.name, "menu", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (FindObjectOfType<ElMandoobMenuPanel>() != null)
        {
            return;
        }

        GameObject root = new GameObject("ElMandoobMenuPanel", typeof(RectTransform));
        root.AddComponent<ElMandoobMenuPanel>().Build();
    }

    private void Update()
    {
        // Refresh the profile from disk while the menu is visible. This makes the
        // balance/reputation update immediately after returning from a completed delivery.
        if (Time.unscaledTime < nextRefreshAt || profileText == null)
        {
            return;
        }

        nextRefreshAt = Time.unscaledTime + 0.75f;
        Refresh();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && profileText != null)
        {
            Refresh();
        }
    }

    private void Build()
    {
        Time.timeScale = 1f;
        data = SaveSystem.Load();

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // Keep the garage entirely on the left so it no longer covers the courier
        // or the original center menu buttons.
        GameObject panel = CreatePanel(
            transform,
            "ProfileGarage",
            new Vector2(0.015f, 0.035f),
            new Vector2(0.34f, 0.40f),
            new Color(0.025f, 0.03f, 0.035f, 0.92f));

        TextMeshProUGUI title = CreateText(
            panel.transform,
            "GarageTitle",
            new Vector2(0.05f, 0.84f),
            new Vector2(0.95f, 0.96f),
            27f,
            TextAlignmentOptions.Right);
        ElMandoobBootstrap.ApplyArabicText(title, "المندوب - حسابك والكراج");

        profileText = CreateText(
            panel.transform,
            "Profile",
            new Vector2(0.05f, 0.69f),
            new Vector2(0.95f, 0.84f),
            19f,
            TextAlignmentOptions.Right);

        storyText = CreateText(
            panel.transform,
            "Story",
            new Vector2(0.05f, 0.49f),
            new Vector2(0.95f, 0.69f),
            17f,
            TextAlignmentOptions.Right);

        speedButton = CreateButton(
            panel.transform,
            "SpeedUpgrade",
            new Vector2(0.05f, 0.30f),
            new Vector2(0.95f, 0.46f),
            out speedButtonText);
        speedButton.onClick.AddListener(BuySpeedUpgrade);

        enduranceButton = CreateButton(
            panel.transform,
            "EnduranceUpgrade",
            new Vector2(0.05f, 0.12f),
            new Vector2(0.95f, 0.28f),
            out enduranceButtonText);
        enduranceButton.onClick.AddListener(BuyEnduranceUpgrade);

        messageText = CreateText(
            panel.transform,
            "GarageMessage",
            new Vector2(0.05f, 0.015f),
            new Vector2(0.95f, 0.11f),
            16f,
            TextAlignmentOptions.Center);

        Refresh();
    }

    private void BuySpeedUpgrade()
    {
        data = SaveSystem.Load();
        int cost = GetSpeedCost();
        if (data.speed >= 8)
        {
            ShowMessage("السرعة وصلت لأقصى تطوير.");
            return;
        }

        if (data.money < cost)
        {
            ShowMessage("محتاج " + (cost - data.money) + " جنيه كمان للتطوير ده.");
            return;
        }

        data.money -= cost;
        data.speed++;
        SaveSystem.Save(data);
        Refresh();
        ShowMessage("تمام. المندوب بقى أسرع في الشارع.");
    }

    private void BuyEnduranceUpgrade()
    {
        data = SaveSystem.Load();
        int cost = GetEnduranceCost();
        if (data.healths >= 3)
        {
            ShowMessage("التحمل وصل لأقصى تطوير.");
            return;
        }

        if (data.money < cost)
        {
            ShowMessage("محتاج " + (cost - data.money) + " جنيه كمان للتطوير ده.");
            return;
        }

        data.money -= cost;
        data.healths++;
        SaveSystem.Save(data);
        Refresh();
        ShowMessage("جهزت نفسك للشيفتات الأصعب.");
    }

    private void Refresh()
    {
        GameData latest = SaveSystem.Load();
        if (latest != null)
        {
            data = latest;
        }

        if (data == null)
        {
            return;
        }

        ElMandoobBootstrap.ApplyArabicText(
            profileText,
            "الرصيد: " + data.money + " جنيه | السمعة: " + data.reputation +
            " | شيفت " + data.levelUnlocked);

        ElMandoobBootstrap.ApplyArabicText(
            storyText,
            "الحكاية: " + ElMandoobContent.GetStoryChapterName(data.storyStage) +
            "\n" + ElMandoobContent.GetNextStoryHint(data));

        int speedCost = GetSpeedCost();
        string speedLabel = data.speed >= 8
            ? "السرعة " + data.speed + "/8 - آخر تطوير"
            : "طوّر السرعة لـ " + (data.speed + 1) + " - " + speedCost + " جنيه";
        ElMandoobBootstrap.ApplyArabicText(speedButtonText, speedLabel);

        int enduranceCost = GetEnduranceCost();
        string enduranceLabel = data.healths >= 3
            ? "التحمل " + data.healths + "/3 - آخر تطوير"
            : "طوّر التحمل لـ " + (data.healths + 1) + " - " + enduranceCost + " جنيه";
        ElMandoobBootstrap.ApplyArabicText(enduranceButtonText, enduranceLabel);

        speedButton.interactable = data.speed < 8;
        enduranceButton.interactable = data.healths < 3;

        ApplyShiftLocks();
    }

    private void ApplyShiftLocks()
    {
        LevelSelector[] selectors = Resources.FindObjectsOfTypeAll<LevelSelector>();
        foreach (LevelSelector selector in selectors)
        {
            if (selector == null || !selector.gameObject.scene.IsValid() ||
                selector.gameObject.scene != gameObject.scene || selector.levelButton == null)
            {
                continue;
            }

            if (!int.TryParse(selector.levelButton.name, out int shiftNumber))
            {
                continue;
            }

            Button button = selector.levelButton.GetComponent<Button>();
            if (button == null)
            {
                button = selector.GetComponent<Button>();
            }

            bool unlocked = shiftNumber <= data.levelUnlocked;
            if (button != null)
            {
                button.interactable = unlocked;
            }

            TMP_Text label = selector.levelButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                ElMandoobBootstrap.ApplyArabicText(
                    label,
                    unlocked ? "شيفت " + shiftNumber : "شيفت " + shiftNumber + " - مقفول");
            }
        }
    }

    private int GetSpeedCost()
    {
        return 150 + Mathf.Max(0, data.speed - 5) * 100;
    }

    private int GetEnduranceCost()
    {
        return 220 + Mathf.Max(0, data.healths - 1) * 140;
    }

    private void ShowMessage(string message)
    {
        ElMandoobBootstrap.ApplyArabicText(messageText, message);
    }

    private GameObject CreatePanel(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panel;
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        out TextMeshProUGUI label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.33f, 0.25f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.21f, 0.43f, 0.32f, 1f);
        colors.pressedColor = new Color(0.11f, 0.24f, 0.18f, 1f);
        colors.disabledColor = new Color(0.12f, 0.12f, 0.13f, 0.65f);
        button.colors = colors;

        label = CreateText(
            buttonObject.transform,
            objectName + "Label",
            new Vector2(0.03f, 0.08f),
            new Vector2(0.97f, 0.92f),
            18f,
            TextAlignmentOptions.Center);

        return button;
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontSizeMin = Mathf.Max(12f, fontSize - 6f);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
