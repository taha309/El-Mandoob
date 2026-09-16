using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds a standalone El Mandoob upgrade button to the existing menu.
/// The main menu stays clean; pressing "تطوير" opens a dedicated upgrade popup.
/// </summary>
public class ElMandoobMenuPanel : MonoBehaviour
{
    private GameData data;

    private GameObject upgradeModal;
    private TextMeshProUGUI profileText;
    private TextMeshProUGUI messageText;
    private Button speedButton;
    private Button enduranceButton;
    private TextMeshProUGUI speedButtonText;
    private TextMeshProUGUI enduranceButtonText;

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

        // One clean standalone button in the bottom-left of the main menu.
        Button openUpgradeButton = CreateMenuButton(
            transform,
            "OpenUpgradeButton",
            new Vector2(0.025f, 0.045f),
            new Vector2(0.20f, 0.13f),
            out TextMeshProUGUI openUpgradeText);
        openUpgradeButton.onClick.AddListener(OpenUpgradePanel);
        ElMandoobBootstrap.ApplyArabicText(openUpgradeText, "تطوير");

        BuildUpgradeModal();
        upgradeModal.SetActive(false);
    }

    private void BuildUpgradeModal()
    {
        // Full-screen raycast blocker so the original menu cannot be clicked through the popup.
        upgradeModal = new GameObject("UpgradeModal", typeof(RectTransform), typeof(Image));
        upgradeModal.transform.SetParent(transform, false);

        RectTransform modalRect = upgradeModal.GetComponent<RectTransform>();
        modalRect.anchorMin = Vector2.zero;
        modalRect.anchorMax = Vector2.one;
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        Image backdrop = upgradeModal.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.58f);
        backdrop.raycastTarget = true;

        GameObject panel = CreatePanel(
            upgradeModal.transform,
            "UpgradeWindow",
            new Vector2(0.30f, 0.18f),
            new Vector2(0.70f, 0.82f),
            new Color(0.035f, 0.055f, 0.045f, 0.98f));

        // Unlike the backdrop, the window itself should also catch raycasts so clicks
        // never reach the old menu behind it.
        panel.GetComponent<Image>().raycastTarget = true;

        TextMeshProUGUI title = CreateText(
            panel.transform,
            "UpgradeTitle",
            new Vector2(0.07f, 0.84f),
            new Vector2(0.93f, 0.96f),
            38f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(title, "تطوير المندوب");

        TextMeshProUGUI hint = CreateText(
            panel.transform,
            "UpgradeHint",
            new Vector2(0.08f, 0.75f),
            new Vector2(0.92f, 0.84f),
            20f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(hint, "استخدم فلوس التوصيلات عشان تطوّر نفسك للشيفتات الأصعب");

        profileText = CreateText(
            panel.transform,
            "Profile",
            new Vector2(0.10f, 0.63f),
            new Vector2(0.90f, 0.74f),
            22f,
            TextAlignmentOptions.Center);

        speedButton = CreateUpgradeButton(
            panel.transform,
            "SpeedUpgrade",
            new Vector2(0.10f, 0.43f),
            new Vector2(0.90f, 0.59f),
            out speedButtonText);
        speedButton.onClick.AddListener(BuySpeedUpgrade);

        enduranceButton = CreateUpgradeButton(
            panel.transform,
            "EnduranceUpgrade",
            new Vector2(0.10f, 0.25f),
            new Vector2(0.90f, 0.41f),
            out enduranceButtonText);
        enduranceButton.onClick.AddListener(BuyEnduranceUpgrade);

        messageText = CreateText(
            panel.transform,
            "UpgradeMessage",
            new Vector2(0.10f, 0.16f),
            new Vector2(0.90f, 0.24f),
            18f,
            TextAlignmentOptions.Center);

        Button closeButton = CreateUpgradeButton(
            panel.transform,
            "CloseUpgrade",
            new Vector2(0.31f, 0.045f),
            new Vector2(0.69f, 0.14f),
            out TextMeshProUGUI closeText);
        closeButton.onClick.AddListener(CloseUpgradePanel);
        ElMandoobBootstrap.ApplyArabicText(closeText, "رجوع");
    }

    private void OpenUpgradePanel()
    {
        if (upgradeModal == null)
        {
            return;
        }

        messageText.text = string.Empty;
        Refresh();
        upgradeModal.SetActive(true);
    }

    private void CloseUpgradePanel()
    {
        if (upgradeModal != null)
        {
            upgradeModal.SetActive(false);
        }
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
        ShowMessage("تم تطوير السرعة. هتحس بالفرق في الشارع.");
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
        ShowMessage("تم تطوير التحمل. بقيت مستعد للشيفتات الأصعب.");
    }

    private void Refresh()
    {
        GameData latest = SaveSystem.Load();
        if (latest != null)
        {
            data = latest;
        }

        if (data == null || profileText == null)
        {
            return;
        }

        ElMandoobBootstrap.ApplyArabicText(
            profileText,
            "الرصيد: " + data.money + " جنيه     |     السمعة: " + data.reputation);

        int speedCost = GetSpeedCost();
        string speedLabel = data.speed >= 8
            ? "السرعة " + data.speed + "/8  -  آخر تطوير"
            : "السرعة " + data.speed + "/8  ←  طوّر لـ " + (data.speed + 1) + "  |  " + speedCost + " جنيه";
        ElMandoobBootstrap.ApplyArabicText(speedButtonText, speedLabel);

        int enduranceCost = GetEnduranceCost();
        string enduranceLabel = data.healths >= 3
            ? "التحمل " + data.healths + "/3  -  آخر تطوير"
            : "التحمل " + data.healths + "/3  ←  طوّر لـ " + (data.healths + 1) + "  |  " + enduranceCost + " جنيه";
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
        if (messageText != null)
        {
            ElMandoobBootstrap.ApplyArabicText(messageText, message);
        }
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

    private Button CreateMenuButton(
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
        Button button = buttonObject.GetComponent<Button>();

        if (!CopyExistingMenuButtonStyle(button, image))
        {
            image.color = new Color(0.96f, 0.60f, 0.22f, 1f);
            ColorBlock fallbackColors = button.colors;
            fallbackColors.highlightedColor = new Color(1f, 0.70f, 0.30f, 1f);
            fallbackColors.pressedColor = new Color(0.78f, 0.43f, 0.12f, 1f);
            button.colors = fallbackColors;
        }

        label = CreateText(
            buttonObject.transform,
            objectName + "Label",
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.92f),
            30f,
            TextAlignmentOptions.Center);

        return button;
    }

    private bool CopyExistingMenuButtonStyle(Button targetButton, Image targetImage)
    {
        Button[] sceneButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button candidate in sceneButtons)
        {
            if (candidate == null || candidate == targetButton ||
                !candidate.gameObject.scene.IsValid() || candidate.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            Image candidateImage = candidate.GetComponent<Image>();
            if (candidateImage == null || candidateImage.sprite == null)
            {
                continue;
            }

            targetImage.sprite = candidateImage.sprite;
            targetImage.type = candidateImage.type;
            targetImage.color = candidateImage.color;
            targetImage.preserveAspect = candidateImage.preserveAspect;
            targetButton.colors = candidate.colors;
            targetButton.transition = candidate.transition;
            return true;
        }

        return false;
    }

    private Button CreateUpgradeButton(
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
        image.color = new Color(0.16f, 0.33f, 0.25f, 0.98f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.23f, 0.46f, 0.34f, 1f);
        colors.pressedColor = new Color(0.10f, 0.23f, 0.17f, 1f);
        colors.disabledColor = new Color(0.11f, 0.12f, 0.12f, 0.70f);
        button.colors = colors;

        label = CreateText(
            buttonObject.transform,
            objectName + "Label",
            new Vector2(0.04f, 0.08f),
            new Vector2(0.96f, 0.92f),
            22f,
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
        text.fontSizeMin = Mathf.Max(13f, fontSize - 7f);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
