using System;
using System.Collections.Generic;
using TMPro;
using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Egyptian Arabic UI support for El Mandoob.
/// The upstream project uses TMP font assets that do not contain Arabic glyphs.
/// Arabic labels are therefore shaped with RTLTMPro and rendered through a
/// dynamic Windows UI font overlay while the original TMP label stays hidden.
/// </summary>
public static class ElMandoobArabic
{
    private static readonly Dictionary<string, string> LegacyTranslations =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "delivery boy", "المندوب" },
            { "Deliver your orders", "وصّل طلباتك" },
            { "Play", "ابدأ الشغل" },
            { "Start", "ابدأ" },
            { "Start game", "ابدأ الشغل" },
            { "Tutorial", "طريقة اللعب" },
            { "Settings", "الإعدادات" },
            { "Quit", "خروج" },
            { "Exit", "خروج" },
            { "Back", "رجوع" },
            { "Back to menu", "ارجع للقائمة" },
            { "Continue", "كمّل" },
            { "Resume", "كمّل" },
            { "Restart", "ابدأ من جديد" },
            { "Retry", "حاول تاني" },
            { "Main Menu", "القائمة الرئيسية" },
            { "Levels", "الشيفتات" },
            { "Level", "الشيفت" },
            { "Level Select", "اختار الشيفت" },
            { "Select Level", "اختار الشيفت" },
            { "Deliver", "سلّم الطلب" },
            { "Receive", "استلم الطلب" },
            { "Next level", "الشيفت اللي بعده" },
            { "TASK FAILED", "الشيفت فشل" },
            { "TASK COMPLETED", "الشيفت خلص" },
            { "PAUSED", "واقف مؤقتًا" },
            { "Pause", "إيقاف مؤقت" },
            { "Audio", "الصوت" },
            { "Music", "الموسيقى" },
            { "Volume", "مستوى الصوت" },
            { "Sound", "الصوت" },
            { "Task", "المطلوب" },
            { "Completed", "تم" },
            { "Confirm", "تأكيد" },
            { "Cancel", "إلغاء" },
            { "Close", "اقفل" },
            { "Game Over", "الشيفت خلص" },
            { "Move with the joystick", "اتحرك بالجوستيك" },
            { "Avoid cars", "خلي بالك من العربيات" },
            { "Read your tasks", "اقرأ المطلوب قبل ما تبدأ" },
            { "Are you sure you want to quit?", "متأكد إنك عايز تخرج؟" },
            { "Yes", "أيوه" },
            { "No", "لأ" },
            { "New Text", "" }
        };

    public static string Shape(string arabicText)
    {
        if (string.IsNullOrEmpty(arabicText))
        {
            return arabicText;
        }

        FastStringBuilder output = new FastStringBuilder(
            Mathf.Max(RTLSupport.DefaultBufferSize, arabicText.Length * 4));

        RTLSupport.FixRTL(
            arabicText,
            output,
            farsi: false,
            fixTextTags: true,
            preserveNumbers: true);

        return output.ToString();
    }

    public static bool TryTranslateLegacyText(string source, out string shapedArabic)
    {
        shapedArabic = null;
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        string cleanSource = source.Trim();
        if (LegacyTranslations.TryGetValue(cleanSource, out string arabic))
        {
            shapedArabic = Shape(arabic);
            return true;
        }

        const string earnedPrefix = "YOU EARNED ";
        if (cleanSource.StartsWith(earnedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string value = cleanSource.Substring(earnedPrefix.Length).Trim();
            shapedArabic = Shape("كسبت " + value);
            return true;
        }

        return false;
    }
}

public class ElMandoobBootstrap : MonoBehaviour
{
    private const string OverlayName = "ElMandoobArabicOverlay";

    private static ElMandoobBootstrap instance;
    private static Font runtimeArabicFont;
    private static bool warnedAboutFont;

    private static readonly string[] PreferredArabicFonts =
    {
        "Tahoma",
        "Arial",
        "Segoe UI",
        "Traditional Arabic",
        "Simplified Arabic",
        "Arabic Typesetting",
        "Noto Sans Arabic",
        "Noto Naskh Arabic"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (instance != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("ElMandoobBootstrap");
        instance = bootstrapObject.AddComponent<ElMandoobBootstrap>();
        DontDestroyOnLoad(bootstrapObject);
        instance.ApplyArabicToLoadedScene(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyArabicToLoadedScene(scene);
    }

    private void ApplyArabicToLoadedScene(Scene scene)
    {
        TMP_Text[] textObjects = Resources.FindObjectsOfTypeAll<TMP_Text>();

        foreach (TMP_Text textObject in textObjects)
        {
            if (textObject == null || !textObject.gameObject.scene.IsValid() ||
                textObject.gameObject.scene != scene)
            {
                continue;
            }

            if (!ElMandoobArabic.TryTranslateLegacyText(textObject.text, out string translated))
            {
                continue;
            }

            ApplyPreparedArabicText(textObject, translated);
        }
    }

    public static void ApplyArabicText(TMP_Text textObject, string arabicText)
    {
        if (textObject == null)
        {
            return;
        }

        ApplyPreparedArabicText(textObject, ElMandoobArabic.Shape(arabicText));
    }

    private static void ApplyPreparedArabicText(TMP_Text textObject, string shapedText)
    {
        if (textObject == null)
        {
            return;
        }

        Font font = GetRuntimeArabicFont();
        if (font == null)
        {
            // Keep the original TMP object as a last-resort fallback.
            textObject.text = shapedText;
            return;
        }

        UnityEngine.UI.Text overlay = GetOrCreateOverlay(textObject, font);
        if (overlay == null)
        {
            textObject.text = shapedText;
            return;
        }

        Color sourceColor = textObject.color;
        overlay.color = new Color(sourceColor.r, sourceColor.g, sourceColor.b, Mathf.Max(0.01f, sourceColor.a));
        overlay.text = shapedText;
        overlay.font = font;
        overlay.fontSize = Mathf.Clamp(Mathf.RoundToInt(textObject.fontSize), 10, 96);
        overlay.fontStyle = FontStyle.Normal;
        overlay.alignment = ConvertAlignment(textObject.alignment);
        overlay.horizontalOverflow = HorizontalWrapMode.Wrap;
        overlay.verticalOverflow = VerticalWrapMode.Truncate;
        overlay.resizeTextForBestFit = textObject.enableAutoSizing;
        overlay.resizeTextMinSize = Mathf.Clamp(Mathf.RoundToInt(textObject.fontSizeMin), 8, 72);
        overlay.resizeTextMaxSize = Mathf.Clamp(Mathf.RoundToInt(textObject.fontSizeMax), overlay.resizeTextMinSize, 120);
        overlay.raycastTarget = false;

        // Hide only the TMP glyphs. The child UI.Text does not inherit TMP vertex alpha.
        textObject.text = string.Empty;
    }

    private static UnityEngine.UI.Text GetOrCreateOverlay(TMP_Text source, Font font)
    {
        Transform existing = source.transform.Find(OverlayName);
        if (existing != null)
        {
            UnityEngine.UI.Text existingText = existing.GetComponent<UnityEngine.UI.Text>();
            if (existingText != null)
            {
                return existingText;
            }
        }

        GameObject overlayObject = new GameObject(
            OverlayName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(UnityEngine.UI.Text));
        overlayObject.transform.SetParent(source.transform, false);

        RectTransform rect = overlayObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        UnityEngine.UI.Text overlay = overlayObject.GetComponent<UnityEngine.UI.Text>();
        overlay.font = font;
        overlay.supportRichText = false;
        overlay.raycastTarget = false;
        return overlay;
    }

    private static TextAnchor ConvertAlignment(TextAlignmentOptions alignment)
    {
        string value = alignment.ToString();
        bool top = value.IndexOf("Top", StringComparison.OrdinalIgnoreCase) >= 0;
        bool bottom = value.IndexOf("Bottom", StringComparison.OrdinalIgnoreCase) >= 0;
        bool left = value.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0;
        bool right = value.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0;

        if (top)
        {
            if (left) return TextAnchor.UpperLeft;
            if (right) return TextAnchor.UpperRight;
            return TextAnchor.UpperCenter;
        }

        if (bottom)
        {
            if (left) return TextAnchor.LowerLeft;
            if (right) return TextAnchor.LowerRight;
            return TextAnchor.LowerCenter;
        }

        if (left) return TextAnchor.MiddleLeft;
        if (right) return TextAnchor.MiddleRight;
        return TextAnchor.MiddleCenter;
    }

    public static Font GetRuntimeArabicFont()
    {
        if (runtimeArabicFont != null)
        {
            return runtimeArabicFont;
        }

        foreach (string fontName in PreferredArabicFonts)
        {
            try
            {
                Font candidate = Font.CreateDynamicFontFromOSFont(fontName, 36);
                if (candidate == null)
                {
                    continue;
                }

                runtimeArabicFont = candidate;
                runtimeArabicFont.name = "ElMandoob_Arabic_" + fontName.Replace(' ', '_');
                Debug.Log("EL MANDOOB ARABIC UI FONT ACTIVE: " + fontName);
                return runtimeArabicFont;
            }
            catch (Exception)
            {
                // Try the next Windows font family.
            }
        }

        try
        {
            Font fallback = Font.CreateDynamicFontFromOSFont(PreferredArabicFonts, 36);
            if (fallback != null)
            {
                runtimeArabicFont = fallback;
                Debug.Log("EL MANDOOB ARABIC UI FONT ACTIVE: system fallback");
                return runtimeArabicFont;
            }
        }
        catch (Exception)
        {
            // Report one useful warning below rather than one warning per candidate.
        }

        if (!warnedAboutFont)
        {
            Debug.LogWarning(
                "El Mandoob: no Arabic-capable Windows font could be created at runtime.");
            warnedAboutFont = true;
        }

        return null;
    }
}
