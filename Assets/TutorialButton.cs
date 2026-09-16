using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    public GameObject tutorialPanel;

    public void OpenTutorial()
    {
        if (tutorialPanel == null)
        {
            return;
        }

        tutorialPanel.SetActive(true);
        BuildElMandoobTutorial();
    }

    private void BuildElMandoobTutorial()
    {
        Transform existing = tutorialPanel.transform.Find("ElMandoobTutorialCard");
        if (existing != null)
        {
            existing.SetAsLastSibling();
            return;
        }

        GameObject card = new GameObject(
            "ElMandoobTutorialCard",
            typeof(RectTransform),
            typeof(Image));
        card.transform.SetParent(tutorialPanel.transform, false);
        card.transform.SetAsLastSibling();

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.08f, 0.08f);
        cardRect.anchorMax = new Vector2(0.92f, 0.92f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        Image background = card.GetComponent<Image>();
        background.color = new Color(0.025f, 0.11f, 0.095f, 0.985f);
        background.raycastTarget = true;

        TextMeshProUGUI title = CreateText(
            card.transform,
            "Title",
            new Vector2(0.08f, 0.82f),
            new Vector2(0.92f, 0.94f),
            34f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(title, "طريقة اللعب");

        TextMeshProUGUI body = CreateText(
            card.transform,
            "Instructions",
            new Vector2(0.10f, 0.22f),
            new Vector2(0.90f, 0.80f),
            27f,
            TextAlignmentOptions.TopRight);
        body.enableWordWrapping = true;

        ElMandoobBootstrap.ApplyArabicText(
            body,
            "١. اتحرك بالجوستيك على الموبايل، أو WASD والأسهم على الكمبيوتر.\n\n" +
            "٢. اتبع السهم لحد محل الاستلام واضغط «استلم الطلب».\n\n" +
            "٣. بعد الاستلام، اتبع السهم لعنوان الزبون واضغط «سلّم الطلب».\n\n" +
            "٤. الفلوس والسمعة بتتحسب بعد التسليم الناجح، وممكن تكسب بقشيش.\n\n" +
            "٥. خبطة العربية بتقلل تحملك وبتضيع مكافأة التوصيل الآمن.\n\n" +
            "٦. استخدم الكراج من القائمة الرئيسية لتطوير السرعة والتحمل.");

        GameObject closeObject = new GameObject(
            "CloseTutorial",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));
        closeObject.transform.SetParent(card.transform, false);

        RectTransform closeRect = closeObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.34f, 0.06f);
        closeRect.anchorMax = new Vector2(0.66f, 0.17f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        Image closeImage = closeObject.GetComponent<Image>();
        closeImage.color = new Color(0.18f, 0.42f, 0.30f, 1f);

        Button closeButton = closeObject.GetComponent<Button>();
        closeButton.onClick.AddListener(() => tutorialPanel.SetActive(false));

        TextMeshProUGUI closeLabel = CreateText(
            closeObject.transform,
            "CloseLabel",
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.92f),
            25f,
            TextAlignmentOptions.Center);
        ElMandoobBootstrap.ApplyArabicText(closeLabel, "تمام");
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontSizeMin = Mathf.Max(16f, fontSize - 8f);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
