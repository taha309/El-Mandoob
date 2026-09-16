using TMPro;
using UnityEngine;

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
        Transform existing = tutorialPanel.transform.Find("ElMandoobTutorialText");
        if (existing != null)
        {
            return;
        }

        TMP_Text[] oldTexts = tutorialPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text oldText in oldTexts)
        {
            // Keep button labels such as Close/Back visible; the runtime translator
            // will Egyptianize them. Old instructional paragraphs are hidden.
            string value = oldText.text != null ? oldText.text.Trim() : string.Empty;
            bool looksLikeButton = value.Equals("Close", System.StringComparison.OrdinalIgnoreCase) ||
                                   value.Equals("Back", System.StringComparison.OrdinalIgnoreCase) ||
                                   value.Equals("Exit", System.StringComparison.OrdinalIgnoreCase);
            if (!looksLikeButton)
            {
                oldText.gameObject.SetActive(false);
            }
        }

        GameObject textObject = new GameObject(
            "ElMandoobTutorialText",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(tutorialPanel.transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.12f, 0.14f);
        rect.anchorMax = new Vector2(0.88f, 0.88f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 30f;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 30f;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.TopRight;
        text.color = Color.white;
        text.raycastTarget = false;

        ElMandoobBootstrap.ApplyArabicText(
            text,
            "طريقة اللعب\n\n" +
            "١. اتحرك بالجوستيك على الموبايل، أو WASD / الأسهم على الكمبيوتر.\n" +
            "٢. اتبع السهم وروح للمحل اللي عليه الطلب.\n" +
            "٣. لما توصل اضغط «استلم الطلب».\n" +
            "٤. كارت الطلب هيقولك اسم الزبون والمنطقة والعنوان.\n" +
            "٥. روح للعنوان واضغط «سلّم الطلب».\n" +
            "٦. ابعد عن العربيات. كل خبطة بتقلل تحملك.\n" +
            "٧. كل توصيلة بتديك جنيهات وسمعة، وممكن يطلعلك بقشيش.\n" +
            "٨. من القائمة الرئيسية استخدم الكراج عشان تطور السرعة والتحمل.\n\n" +
            "مع الوقت هتقابل زباين بيتكرروا، وبعض الطلبات هتفتح حكاية الحي.");
    }
}
