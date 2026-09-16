using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds a standalone El Mandoob upgrade button to the original menu.
/// The main menu stays clean; pressing "تطوير" opens a focused upgrade modal.
/// </summary>
public class ElMandoobMenuPanel : MonoBehaviour
{
    private GameData data;

    private GameObject upgradeModal;
    private RectTransform upgradeWindow;
    private CanvasGroup upgradeCanvasGroup;

    private TextMeshProUGUI profileText;
    private TextMeshProUGUI speedStatusText;
    private TextMeshProUGUI enduranceStatusText;
    private TextMeshProUGUI messageText;

    private Button speedButton;
    private Button enduranceButton;
    private Image speedButtonImage;
    private Image enduranceButtonImage;
    private TextMeshProUGUI speedButtonText;
    private TextMeshProUGUI enduranceButtonText;

    private Coroutine modalAnimation;

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
        if (upgradeModal != null && upgradeModal.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUpgradePanel();
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

        // One standalone button. Nothing else permanently occupies the left side.
        Button openUpgradeButton = CreateMenuButton(
            transform,
            "OpenUpgradeButton",
            new Vector2(0.025f, 0.045f),
            new Vector2(0.20f, 0.135f),
            out TextMeshProUGUI openUpgradeText);
        openUpgradeButton.onClick.AddListener(OpenUpgradePanel);
        ElMandoobBootstrap.ApplyArabicText(openUpgradeText, "تطوير");

        BuildUpgradeModal();
        upgradeModal.SetActive(false);

        // Shift locks belong to progression and should be applied even if the player
        // never opens the upgrade screen.
        RefreshData();
        ApplyShiftLocks();
    }

    private void BuildUpgradeModal()
    {
        upgradeModal = new GameObject(
            "UpgradeModal",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(CanvasGroup));
        upgradeModal.transform.SetParent(transform, false);

        RectTransform modalRect = upgradeModal.GetComponent<RectTransform>();
        modalRect.anchorMin = Vector2.zero;
        modalRect.anchorMax = Vector2.one;
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        Image backdrop = upgradeModal.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.68f);
        backdrop.raycastTarget = true;

        Button backdropButton = upgradeModal.GetComponent<Button>();
        backdropButton.transition = Selectable.Transition.None;
        backdropButton.targetGraphic = backdrop;
        backdropButton.onClick.AddListener(CloseUpgradePanel);

        upgradeCanvasGroup = upgradeModal.GetComponent<CanvasGroup>();
        upgradeCanvasGroup.alpha = 1f;

        GameObject windowObject = CreatePanel(
            upgradeModal.transform,
            "UpgradeWindow",
            new Vector2(0.29f, 0.14f),
            new Vector2(0.71f, 0.86f),
            new Color(0.035f, 0.052f, 0.043f, 0.995f));

        Image windowImage = windowObject.GetComponent<Image>();
        windowImage.raycastTarget = true;
        upgradeWindow = windowObject.GetComponent<RectTransform>();

        // Accent strip gives the modal a deliberate game-screen hierarchy without
        // importing new art assets.
        CreatePanel(
            windowObject.transform,
            "HeaderAccent",
            new Vector2(0f, 0.965f),
            new Vector2(1f, 1f),
            new Color(0.95f, 0.58f, 0.18f, 1f));

        TextMeshProUGUI title = CreateText(
            windowObject.transform,
            "UpgradeTitle",
            new Vector2(0.07f, 0.84f),
            new Vector2(0.93f, 0.95f),
            40f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(title, "تطوير المندوب");

        TextMeshProUGUI hint = CreateText(
            windowObject.transform,
            "UpgradeHint",
            new Vector2(0.08f, 0.76f),
            new Vector2(0.92f, 0.84f),
            19f,
            TextAlignmentOptions.Center);
        hint.color = new Color(0.82f, 0.86f, 0.82f, 1f);
        ElMandoobBootstrap.ApplyArabicText(hint, "كل تطوير دائم وبيأثر على كل الشيفتات");

        GameObject walletPanel = CreatePanel(
            windowObject.transform,
            "WalletPanel",
            new Vector2(0.10f, 0.65f),
            new Vector2(0.90f, 0.75f),
            new Color(0.07f, 0.11f, 0.09f, 1f));

        profileText = CreateText(
            walletPanel.transform,
            "Profile",
            new Vector2(0.04f, 0.08f),
            new Vector2(0.96f, 0.92f),
            23f,
            TextAlignmentOptions.Center);

        // SPEED CARD
        GameObject speedCard = CreatePanel(
            windowObject.transform,
            "SpeedCard",
            new Vector2(0.08f, 0.42f),
            new Vector2(0.92f, 0.62f),
            new Color(0.055f, 0.09f, 0.075f, 1f));

        TextMeshProUGUI speedTitle = CreateText(
            speedCard.transform,
            "SpeedTitle",
            new Vector2(0.05f, 0.62f),
            new Vector2(0.42f, 0.93f),
            25f,
            TextAlignmentOptions.Right);
        ElMandoobBootstrap.ApplyArabicText(speedTitle, "السرعة");

        speedStatusText = CreateText(
            speedCard.transform,
            "SpeedStatus",
            new Vector2(0.05f, 0.15f),
            new Vector2(0.54f, 0.61f),
            18f,
            TextAlignmentOptions.Right);

        speedButton = CreateUpgradeButton(
            speedCard.transform,
            "SpeedUpgrade",
            new Vector2(0.60f, 0.18f),
            new Vector2(0.94f, 0.82f),
            out speedButtonText,
            out speedButtonImage);
        speedButton.onClick.AddListener(BuySpeedUpgrade);

        // ENDURANCE CARD
        GameObject enduranceCard = CreatePanel(
            windowObject.transform,
            "EnduranceCard",
            new Vector2(0.08f, 0.20f),
            new Vector2(0.92f, 0.40f),
            new Color(0.055f, 0.09f, 0.075f, 1f));

        TextMeshProUGUI enduranceTitle = CreateText(
            enduranceCard.transform,
            "EnduranceTitle",
            new Vector2(0.05f, 0.62f),
            new Vector2(0.42f, 0.93f),
            25f,
            TextAlignmentOptions.Right);
        ElMandoobBootstrap.ApplyArabicText(enduranceTitle, "التحمل");

        enduranceStatusText = CreateText(
            enduranceCard.transform,
            "EnduranceStatus",
            new Vector2(0.05f, 0.15f),
            new Vector2(0.54f, 0.61f),
            18f,
            TextAlignmentOptions.Right);

        enduranceButton = CreateUpgradeButton(
            enduranceCard.transform,
            "EnduranceUpgrade",
            new Vector2(0.60f, 0.18f),
            new Vector2(0.94f, 0.82f),
            out enduranceButtonText,
            out enduranceButtonImage);
        enduranceButton.onClick.AddListener(BuyEnduranceUpgrade);

        messageText = CreateText(
            windowObject.transform,
            "UpgradeMessage",
            new Vector2(0.10f, 0.125f),
            new Vector2(0.90f, 0.19f),
            18f,
            TextAlignmentOptions.Center);
        messageText.color = new Color(1f, 0.80f, 0.38f, 1f);

        Button closeButton = CreateUpgradeButton(
            windowObject.transform,
            "CloseUpgrade",
            new Vector2(0.34f, 0.035f),
            new Vector2(0.66f, 0.115f),
            out TextMeshProUGUI closeText,
            out Image closeImage);
        closeImage.color = new Color(0.16f, 0.18f, 0.17f, 1f);
        closeButton.onClick.AddListener(CloseUpgradePanel);
        ElMandoobBootstrap.ApplyArabicText(closeText, "رجوع");
    }

    private void OpenUpgradePanel()
    {
        if (upgradeModal == null)
        {
            return;
        }

        Refresh();
        ClearMessage();
        upgradeModal.SetActive(true);

        if (modalAnimation != null)
        {
            StopCoroutine(modalAnimation);
        }
        modalAnimation = StartCoroutine(AnimateModal(true));
    }

    private void CloseUpgradePanel()
    {
        if (upgradeModal == null || !upgradeModal.activeSelf)
        {
            return;
        }

        if (modalAnimation != null)
        {
            StopCoroutine(modalAnimation);
        }
        modalAnimation = StartCoroutine(AnimateModal(false));
    }

    private IEnumerator AnimateModal(bool opening)
    {
        const float duration = 0.16f;
        float elapsed = 0f;

        float startAlpha = upgradeCanvasGroup != null ? upgradeCanvasGroup.alpha : (opening ? 0f : 1f);
        float endAlpha = opening ? 1f : 0f;

        Vector3 startScale = opening ? new Vector3(0.94f, 0.94f, 1f) : Vector3.one;
        Vector3 endScale = opening ? Vector3.one : new Vector3(0.96f, 0.96f, 1f);

        if (upgradeWindow != null && opening)
        {
            upgradeWindow.localScale = startScale;
        }
        if (upgradeCanvasGroup != null && opening)
        {
            upgradeCanvasGroup.alpha = 0f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = t * t * (3f - 2f * t);

            if (upgradeCanvasGroup != null)
            {
                upgradeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smooth);
            }
            if (upgradeWindow != null)
            {
                upgradeWindow.localScale = Vector3.Lerp(startScale, endScale, smooth);
            }

            yield return null;
        }

        if (upgradeCanvasGroup != null)
        {
            upgradeCanvasGroup.alpha = endAlpha;
        }
        if (upgradeWindow != null)
        {
            upgradeWindow.localScale = endScale;
        }

        if (!opening)
        {
            upgradeModal.SetActive(false);
            if (upgradeCanvasGroup != null)
            {
                upgradeCanvasGroup.alpha = 1f;
            }
            if (upgradeWindow != null)
            {
                upgradeWindow.localScale = Vector3.one;
            }
        }

        modalAnimation = null;
    }

    private void BuySpeedUpgrade()
    {
        RefreshData();
        int cost = GetSpeedCost();

        if (data.speed >= 8)
        {
            ShowMessage("السرعة وصلت لأقصى تطوير.");
            return;
        }

        if (data.money < cost)
        {
            ShowMessage("ناقصك " + (cost - data.money) + " جنيه للتطوير ده.");
            return;
        }

        data.money -= cost;
        data.speed++;
        SaveSystem.Save(data);
        Refresh();
        ShowMessage("تم تطوير السرعة. المندوب بقى أسرع.");
    }

    private void BuyEnduranceUpgrade()
    {
        RefreshData();
        int cost = GetEnduranceCost();

        if (data.healths >= 3)
        {
            ShowMessage("التحمل وصل لأقصى تطوير.");
            return;
        }

        if (data.money < cost)
        {
            ShowMessage("ناقصك " + (cost - data.money) + " جنيه للتطوير ده.");
            return;
        }

        data.money -= cost;
        data.healths++;
        SaveSystem.Save(data);
        Refresh();
        ShowMessage("تم تطوير التحمل. هتستحمل خبطة زيادة.");
    }

    private void RefreshData()
    {
        GameData latest = SaveSystem.Load();
        if (latest != null)
        {
            data = latest;
        }

        if (data == null)
        {
            data = new GameData();
        }
    }

    private void Refresh()
    {
        RefreshData();

        if (profileText == null)
        {
            return;
        }

        ElMandoobBootstrap.ApplyArabicText(
            profileText,
            "الرصيد: " + data.money + " جنيه     |     السمعة: " + data.reputation);

        int speedCost = GetSpeedCost();
        bool speedMax = data.speed >= 8;
        bool canAffordSpeed = data.money >= speedCost;

        ElMandoobBootstrap.ApplyArabicText(
            speedStatusText,
            speedMax
                ? "المستوى الحالي: 8 / 8\nوصلت لأقصى سرعة"
                : "المستوى الحالي: " + data.speed + " / 8\nالمستوى الجاي: " + (data.speed + 1) + " / 8");

        ElMandoobBootstrap.ApplyArabicText(
            speedButtonText,
            speedMax ? "آخر تطوير" : "تطوير\n" + speedCost + " جنيه");

        int enduranceCost = GetEnduranceCost();
        bool enduranceMax = data.healths >= 3;
        bool canAffordEndurance = data.money >= enduranceCost;

        ElMandoobBootstrap.ApplyArabicText(
            enduranceStatusText,
            enduranceMax
                ? "المستوى الحالي: 3 / 3\nوصلت لأقصى تحمل"
                : "المستوى الحالي: " + data.healths + " / 3\nالمستوى الجاي: " + (data.healths + 1) + " / 3");

        ElMandoobBootstrap.ApplyArabicText(
            enduranceButtonText,
            enduranceMax ? "آخر تطوير" : "تطوير\n" + enduranceCost + " جنيه");

        // Maxed upgrades are disabled. Unaffordable upgrades remain clickable so the
        // player gets a useful "ناقصك X جنيه" message instead of silent failure.
        speedButton.interactable = !speedMax;
        enduranceButton.interactable = !enduranceMax;

        ApplyUpgradeButtonVisual(speedButtonImage, speedMax, canAffordSpeed);
        ApplyUpgradeButtonVisual(enduranceButtonImage, enduranceMax, canAffordEndurance);

        ApplyShiftLocks();
    }

    private void ApplyUpgradeButtonVisual(Image image, bool maxed, bool affordable)
    {
        if (image == null)
        {
            return;
        }

        if (maxed)
        {
            image.color = new Color(0.12f, 0.13f, 0.13f, 1f);
        }
        else if (affordable)
        {
            image.color = new Color(0.16f, 0.42f, 0.27f, 1f);
        }
        else
        {
            image.color = new Color(0.34f, 0.20f, 0.13f, 1f);
        }
    }

    private void ApplyShiftLocks()
    {
        if (data == null)
        {
            return;
        }

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

    private void ClearMessage()
    {
        if (messageText == null)
        {
            return;
        }

        UnityEngine.UI.Text overlay = messageText.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (overlay != null)
        {
            overlay.text = string.Empty;
        }
        messageText.text = string.Empty;
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
        out TextMeshProUGUI label,
        out Image image)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.42f, 0.27f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.80f, 0.80f, 0.80f, 1f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.80f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        label = CreateText(
            buttonObject.transform,
            objectName + "Label",
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.92f),
            21f,
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
        text.fontSizeMin = Mathf.Max(12f, fontSize - 7f);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
