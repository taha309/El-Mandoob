using System;
using System.Collections.Generic;
using TMPro;
using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Egyptian Arabic UI support for El Mandoob.
/// The source project stores many labels directly in scenes/prefabs, so known legacy
/// labels are translated when a scene loads while all new systems use ApplyArabicText.
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

/// <summary>
/// Installs El Mandoob Arabic presentation automatically without scene references.
/// </summary>
public class ElMandoobBootstrap : MonoBehaviour
{
    private static ElMandoobBootstrap instance;
    private static TMP_FontAsset runtimeArabicFont;
    private static bool warnedAboutFont;

    // Windows normally includes Tahoma and Arial with Arabic glyph coverage.
    // Try these directly in a deterministic order. Unity 2021's legacy Font.HasCharacter
    // can incorrectly report false for Arabic presentation forms, so candidates are no
    // longer rejected through that API before TextMeshPro gets a chance to build them.
    private static readonly string[] PreferredArabicFonts =
    {
        "Tahoma",
        "Arial",
        "Segoe UI",
        "Traditional Arabic",
        "Simplified Arabic",
        "Arabic Typesetting",
        "Noto Sans Arabic",
        "Noto Naskh Arabic",
        "Arial Unicode MS"
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
        TMP_FontAsset arabicFont = GetRuntimeArabicFont();
        if (arabicFont != null)
        {
            textObject.font = arabicFont;
            textObject.fontSharedMaterial = arabicFont.material;
        }

        // RTLSupport already shapes and reorders the string.
        textObject.isRightToLeftText = false;
        textObject.text = shapedText;
    }

    public static TMP_FontAsset GetRuntimeArabicFont()
    {
        if (runtimeArabicFont != null)
        {
            return runtimeArabicFont;
        }

        foreach (string fontName in PreferredArabicFonts)
        {
            TMP_FontAsset candidate = TryCreateArabicFont(fontName);
            if (candidate != null)
            {
                runtimeArabicFont = candidate;
                Debug.Log("EL MANDOOB ARABIC FONT ACTIVE: " + fontName);
                return runtimeArabicFont;
            }
        }

        // Last chance: ask Unity to pick from the whole preferred family list itself.
        try
        {
            Font sourceFont = Font.CreateDynamicFontFromOSFont(PreferredArabicFonts, 36);
            TMP_FontAsset fallback = CreateTmpFontAsset(sourceFont, "SystemFallback");
            if (fallback != null)
            {
                runtimeArabicFont = fallback;
                Debug.Log("EL MANDOOB ARABIC FONT ACTIVE: system fallback");
                return runtimeArabicFont;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("El Mandoob: system Arabic font fallback failed. " + exception.Message);
        }

        if (!warnedAboutFont)
        {
            Debug.LogWarning(
                "El Mandoob: could not create an Arabic-capable runtime TMP font. " +
                "Arabic text will require a bundled TMP font asset before release.");
            warnedAboutFont = true;
        }

        return null;
    }

    private static TMP_FontAsset TryCreateArabicFont(string fontName)
    {
        try
        {
            Font sourceFont = Font.CreateDynamicFontFromOSFont(fontName, 36);
            return CreateTmpFontAsset(sourceFont, fontName.Replace(' ', '_'));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("El Mandoob: Arabic font candidate failed (" + fontName + "). " + exception.Message);
            return null;
        }
    }

    private static TMP_FontAsset CreateTmpFontAsset(Font sourceFont, string label)
    {
        if (sourceFont == null)
        {
            return null;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (fontAsset == null)
        {
            return null;
        }

        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fontAsset.name = "ElMandoob_RuntimeArabic_" + label;
        return fontAsset;
    }
}
