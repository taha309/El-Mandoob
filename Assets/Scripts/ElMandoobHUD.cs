using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime-created HUD for El Mandoob.
/// It deliberately does not require scene wiring so the original Delivery Boy scenes
/// stay usable while the game is transformed.
/// </summary>
public class ElMandoobHUD : MonoBehaviour
{
    private TextMeshProUGUI statsText;
    private TextMeshProUGUI chapterText;
    private TextMeshProUGUI orderText;
    private TextMeshProUGUI notificationText;
    private GameObject notificationPanel;
    private Coroutine notificationRoutine;
    private int currentLevel;

    public static ElMandoobHUD Create(GameData data, int level)
    {
        GameObject root = new GameObject("ElMandoobHUD", typeof(RectTransform));
        ElMandoobHUD hud = root.AddComponent<ElMandoobHUD>();
        hud.currentLevel = Mathf.Clamp(level, 1, 8);
        hud.Build(data);
        return hud;
    }

    private void Build(GameData data)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject topPanel = CreatePanel(
            "TopPanel",
            new Vector2(0.57f, 0.79f),
            new Vector2(0.985f, 0.985f),
            new Color(0.04f, 0.04f, 0.05f, 0.82f));

        chapterText = CreateText(
            topPanel.transform,
            "Chapter",
            new Vector2(0.04f, 0.76f),
            new Vector2(0.96f, 0.96f),
            25f,
            TextAlignmentOptions.Right);

        statsText = CreateText(
            topPanel.transform,
            "Stats",
            new Vector2(0.04f, 0.57f),
            new Vector2(0.96f, 0.76f),
            23f,
            TextAlignmentOptions.Right);

        orderText = CreateText(
            topPanel.transform,
            "CurrentOrder",
            new Vector2(0.04f, 0.05f),
            new Vector2(0.96f, 0.55f),
            25f,
            TextAlignmentOptions.TopRight);
        orderText.enableWordWrapping = true;

        notificationPanel = CreatePanel(
            "NotificationPanel",
            new Vector2(0.20f, 0.055f),
            new Vector2(0.80f, 0.20f),
            new Color(0.03f, 0.03f, 0.04f, 0.90f));

        notificationText = CreateText(
            notificationPanel.transform,
            "Notification",
            new Vector2(0.04f, 0.12f),
            new Vector2(0.96f, 0.88f),
            28f,
            TextAlignmentOptions.Center);
        notificationText.enableWordWrapping = true;
        notificationPanel.SetActive(false);

        RefreshStats(data);
        SetWaitingForOrder();
    }

    public void RefreshStats(GameData data)
    {
        if (data == null)
        {
            return;
        }

        ElMandoobBootstrap.ApplyArabicText(
            chapterText,
            "شيفت " + currentLevel + " - " + ElMandoobContent.GetShiftArea(currentLevel) +
            " | " + ElMandoobContent.GetStoryChapterName(data.storyStage));

        ElMandoobBootstrap.ApplyArabicText(
            statsText,
            "الرصيد: " + data.money + " جنيه   |   السمعة: " + data.reputation +
            "   |   التوصيلات: " + data.completedDeliveries);
    }

    public void SetWaitingForOrder()
    {
        ElMandoobBootstrap.ApplyArabicText(orderText, "مستني طلب جديد...");
    }

    public void SetOrder(ElMandoobOrder order, bool carrying)
    {
        if (order == null)
        {
            SetWaitingForOrder();
            return;
        }

        string header = order.storyOrder ? "طلب خاص" : "طلب جديد";
        string destinationLine = carrying
            ? "الزبون: " + order.customerName + " - " + order.areaName
            : "الاستلام من: " + order.businessName;

        string instruction = carrying
            ? "العنوان: " + order.address
            : "روح للمحل واستلم الطلب";

        string payout = "الأجرة: " + order.basePay + " جنيه";
        if (order.tip > 0)
        {
            payout += " + بقشيش " + order.tip;
        }

        ElMandoobBootstrap.ApplyArabicText(
            orderText,
            header + "\n" + destinationLine + "\n" +
            "الطلب: " + order.itemDescription + "\n" + instruction + "\n" + payout);
    }

    public void ShowMessage(string message, float seconds = 4.5f)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
        }

        notificationRoutine = StartCoroutine(ShowMessageRoutine(message, seconds));
    }

    public void ShowStoryFinished()
    {
        ShowMessage(
            "خلصت حكاية أول حي. لسه تقدر تكمل شيفتات وتجمع فلوس وسمعة، والحكايات الجاية هتتفتح مع مناطق جديدة.",
            7f);
    }

    private IEnumerator ShowMessageRoutine(string message, float seconds)
    {
        ElMandoobBootstrap.ApplyArabicText(notificationText, message);
        notificationPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(seconds);
        notificationPanel.SetActive(false);
        notificationRoutine = null;
    }

    private GameObject CreatePanel(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);

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
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(16f, fontSize - 8f);
        text.fontSizeMax = fontSize;

        return text;
    }
}
