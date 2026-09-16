using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds El Mandoob progression controls to the existing menu scene.
/// Account/story info and upgrades are intentionally separated so the bottom-left
/// area reads as a dedicated "تطوير" section instead of one crowded garage card.
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

        // Small account/story card above the upgrade section.
        GameObject accountPanel = CreatePanel(
            transform,
            "AccountPanel",
            new Vector2(0.015f, 0.245f),
            new Vector2(0.30f, 0.43f),
            new Color(0.025f, 0.03f, 0.035f, 0.92f));

        TextMeshProUGUI accountTitle = CreateText(
            accountPanel.transform,
            "AccountTitle",
            new Vector2(0.05f, 0.78f),
            new Vector2(0.95f, 0.96f),
            26f,
            TextAlignmentOptions.Right);
        ElMandoobBootstrap.ApplyArabicText(accountTitle, "حسابك");

        profileText = CreateText(
            accountPanel.transform,
            "Profile",
            new Vector2(0.05f, 0.48f),
            new Vector2(0.95f, 0.78f),
            18f,
            TextAlignmentOptions.Right);

        storyText = CreateText(
            accountPanel.transform,
            "Story",
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.47f),
            16f,
            TextAlignmentOptions.Right);

        // Standalone upgrade card in the bottom-left corner.
        GameObject upgradePanel = CreatePanel(
            transform,
            "UpgradePanel",
            new Vector2(0.015f, 0.025f),
            new Vector2(0.36f, 0.225f),
            new Color(0.055f, 0.14f, 0.10f, 0.97f));

        TextMeshProUGUI upgradeTitle = CreateText(
            upgradePanel.transform,
            "UpgradeTitle",
            new Vector2(0.05f, 0.78f),
            new Vector2(0.95f, 0.96f),
            30f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(upgradeTitle, "تطوير");

        TextMeshProUGUI upgradeHint = CreateText(
            upgradePanel.transform,
            "UpgradeHint",
            new Vector2(0.05f, 0.64f),
            new Vector2(0.95f, 0.79f),
            16f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(upgradeHint, "استخدم فلوس التوصيلات عشان تطوّر المندوب");

        speedButton = CreateButton(
            upgradePanel.transform,
            "SpeedUpgrade",
            new Vector2(0.05f, 0.36f),
            new Vector2(0.95f, 0.61f),
            out speedButtonText);
        speedButton.onClick.AddListener(BuySpeedUpgrade);

        enduranceButton = CreateButton(
            upgradePanel.transform,
            "EnduranceUpgrade",
            new Vector2(0.05f, 0.10f),
            new Vector2(0.95f, 0.34f),
            out enduranceButtonText);
        enduranceButton.onClick.AddListener(BuyEnduranceUpgrade);

        messageText = CreateText(
            upgradePanel.transform,
            "UpgradeMessage",
            new Vector2(0.05f, 0.01f),
            new Vector2(0.95f, 0.09f),
            14f,
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
            ShowMessage("محتاج " + (cost - data.money) + " جنيه كمان.");
            return;
        }

        data.money -= cost;
        data.speed++;
        SaveSystem.Save(data);
        Refresh();
        ShowMessage("تم تطوير السرعة.");
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
            ShowMessage("محتاج " + (cost - data.money) + " جنيه كمان.");
            return;
        }

        data.money -= cost;
        data.healths++;
        SaveSystem.Save(data);
        Refresh();
        ShowMessage("تم تطوير التحمل.");
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
            "الرصيد: " + data.money + " جنيه\n" +
            "السمعة: " + data.reputation + "\n" +
            "مفتوح لحد شيفت " + data.levelUnlocked);

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
        image.color = new Color(0.20f, 0.36f, 0.27f, 0.98f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.25f, 0.45f, 0.34f, 1f);
        colors.pressedColor = new Color(0.12f, 0.24f, 0.18f, 1f);
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
