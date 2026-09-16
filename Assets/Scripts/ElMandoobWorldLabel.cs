using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small world-space sign used to make the active ready-made map read as an Egyptian
/// delivery district without requiring hand-edited scene prefabs for every building.
/// </summary>
public class ElMandoobWorldLabel : MonoBehaviour
{
    private TextMeshProUGUI label;

    public static ElMandoobWorldLabel Attach(
        GameObject target,
        string text,
        string objectName,
        Color background,
        float verticalOffset = 2.4f)
    {
        if (target == null)
        {
            return null;
        }

        Transform existing = target.transform.Find(objectName);
        ElMandoobWorldLabel worldLabel;

        if (existing != null)
        {
            worldLabel = existing.GetComponent<ElMandoobWorldLabel>();
            if (worldLabel != null)
            {
                worldLabel.SetText(text);
                return worldLabel;
            }
        }

        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        root.transform.SetParent(target.transform, false);
        root.transform.localPosition = new Vector3(0f, verticalOffset, 0f);
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * 0.01f;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(300f, 72f);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        worldLabel = root.AddComponent<ElMandoobWorldLabel>();
        worldLabel.Build(background);
        worldLabel.SetText(text);
        return worldLabel;
    }

    private void Build(Color background)
    {
        GameObject plate = new GameObject("Plate", typeof(RectTransform), typeof(Image));
        plate.transform.SetParent(transform, false);

        RectTransform plateRect = plate.GetComponent<RectTransform>();
        plateRect.anchorMin = Vector2.zero;
        plateRect.anchorMax = Vector2.one;
        plateRect.offsetMin = Vector2.zero;
        plateRect.offsetMax = Vector2.zero;

        Image image = plate.GetComponent<Image>();
        image.color = background;
        image.raycastTarget = false;

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(plate.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.04f, 0.08f);
        textRect.anchorMax = new Vector2(0.96f, 0.92f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        label = textObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = 38f;
        label.fontSizeMin = 22f;
        label.fontSizeMax = 38f;
        label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    public void SetText(string value)
    {
        if (label == null)
        {
            label = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        ElMandoobBootstrap.ApplyArabicText(label, value);
    }
}
