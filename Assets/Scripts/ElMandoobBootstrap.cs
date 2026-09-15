using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Egyptian Arabic UI support for El Mandoob.
///
/// The original project stores most labels directly inside Unity scenes and prefabs.
/// Rather than rewriting large YAML scene files, this bootstrap translates known legacy
/// labels when scenes load. New gameplay text should call ElMandoobArabic.Shape directly.
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
            { "Tutorial", "طريقة اللعب" },
            { "Settings", "الإعدادات" },
            { "Quit", "خروج" },
            { "Exit", "خروج" },
            { "Back", "رجوع" },
            { "Continue", "كمّل" },
            { "Resume", "كمّل" },
            { "Restart", "ابدأ من جديد" },
            { "Main Menu", "القائمة الرئيسية" },
            { "Levels", "الشيفتات" },
            { "Level Select", "اختار الشيفت" },
            { "Deliver", "سلّم الطلب" },
            { "Receive", "استلم الطلب" },
            { "Next level", "الشيفت اللي بعده" },
            { "TASK FAILED", "الشيفت فشل" },
            { "PAUSED", "واقف مؤقتًا" },
            { "Pause", "إيقاف مؤقت" },
            { "Audio", "الصوت" },
            { "Music", "الموسيقى" },
            { "Volume", "مستوى الصوت" },
            { "Task", "المطلوب" },
            { "Completed", "تم" }
        };

    /// <summary>
    /// Shapes Arabic text for normal TextMeshPro components.
    /// RTLTMPro handles Arabic letter joining and text direction; we keep western digits
    /// for timers and gameplay values so strings such as 01:30 remain unambiguous.
    /// </summary>
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
/// Installs the El Mandoob Arabic presentation layer automatically.
/// No scene reference is required, which keeps the original scenes intact while the
/// project is being converted milestone by milestone.
/// </summary>
public class ElMandoobBootstrap : MonoBehaviour
{
    private static ElMandoobBootstrap instance;
    private static TMP_FontAsset runtimeArabicFont;
    private static bool warnedAboutFont;

    private static readonly string[] PreferredArabicFonts =
    {
        "Noto Sans Arabic",
        "Noto Naskh Arabic",
        "Tahoma",
        "Arial"
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
        TMP_FontAsset arabicFont = GetRuntimeArabicFont();
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

            if (arabicFont != null)
            {
                textObject.font = arabicFont;
            }

            // Text has already been shaped by RTLTMPro, so do not reverse it again.
            textObject.isRightToLeftText = false;
            textObject.text = translated;
        }
    }

    private static TMP_FontAsset GetRuntimeArabicFont()
    {
        if (runtimeArabicFont != null)
        {
            return runtimeArabicFont;
        }

        string[] installedFonts = Font.GetOSInstalledFontNames();
        string selectedFont = PreferredArabicFonts.FirstOrDefault(preferred =>
            installedFonts.Any(installed =>
                string.Equals(installed, preferred, StringComparison.OrdinalIgnoreCase)));

        if (string.IsNullOrEmpty(selectedFont))
        {
            if (!warnedAboutFont)
            {
                Debug.LogWarning(
                    "El Mandoob: no preferred Arabic system font was found. " +
                    "Arabic text shaping is active, but a bundled Arabic TMP font asset " +
                    "must be added before release.");
                warnedAboutFont = true;
            }

            return null;
        }

        Font sourceFont = Font.CreateDynamicFontFromOSFont(selectedFont, 32);
        if (sourceFont == null)
        {
            return null;
        }

        runtimeArabicFont = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (runtimeArabicFont != null)
        {
            runtimeArabicFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            runtimeArabicFont.name = "ElMandoob_RuntimeArabic";
        }

        return runtimeArabicFont;
    }
}
