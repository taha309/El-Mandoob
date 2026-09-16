using System;
using System.Collections.Generic;
using System.Linq;
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

    // These are tried directly rather than relying on an exact-name match from
    // GetOSInstalledFontNames. Unity can report Windows font family names differently
    // across machines even when the font itself is installed.
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

    private const string ArabicProbe = "المندوب شريف ندى عم حسن جنيه طلب توصيل";

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

        // First try known Windows/Arabic font families directly. This avoids the
        // exact-name lookup that caused the first test machine to fall back to
        // LiberationSans SDF and render Arabic as square boxes.
        foreach (string fontName in PreferredArabicFonts)
        {
            TMP_FontAsset candidate = TryCreateArabicFont(fontName);
            if (candidate != null)
            {
                runtimeArabicFont = candidate;
                return runtimeArabicFont;
            }
        }

        // Then try any installed font whose family name strongly suggests Arabic support.
        string[] installedFonts = Font.GetOSInstalledFontNames();
        if (installedFonts != null)
        {
            foreach (string installed in installedFonts)
            {
                if (string.IsNullOrWhiteSpace(installed))
                {
                    continue;
                }

                string lower = installed.ToLowerInvariant();
                bool likelyArabic = lower.Contains("arab") || lower.Contains("naskh") ||
                                    lower.Contains("nask") || lower.Contains("kufi") ||
                                    lower.Contains("tahoma") || lower.Contains("arial") ||
                                    lower.Contains("segoe");

                if (!likelyArabic)
                {
                    continue;
                }

                TMP_FontAsset candidate = TryCreateArabicFont(installed);
                if (candidate != null)
                {
                    runtimeArabicFont = candidate;
                    return runtimeArabicFont;
                }
            }
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
            Font sourceFont = Font.CreateDynamicFontFromOSFont(fontName, 32);
            if (sourceFont == null)
            {
                return null;
            }

            string shapedProbe = ElMandoobArabic.Shape(ArabicProbe);
            sourceFont.RequestCharactersInTexture(shapedProbe, 32, FontStyle.Normal);

            bool hasArabic = shapedProbe
                .Where(character => !char.IsWhiteSpace(character) && !char.IsDigit(character))
                .Any(character => sourceFont.HasCharacter(character));

            if (!hasArabic)
            {
                return null;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            if (fontAsset == null)
            {
                return null;
            }

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.name = "ElMandoob_RuntimeArabic_" + fontName.Replace(' ', '_');

            Debug.Log("El Mandoob Arabic font: " + fontName);
            return fontAsset;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("El Mandoob: Arabic font candidate failed (" + fontName + "). " + exception.Message);
            return null;
        }
    }
}
