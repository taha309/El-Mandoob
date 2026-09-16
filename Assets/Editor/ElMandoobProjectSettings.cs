#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class ElMandoobProjectSettings
{
    static ElMandoobProjectSettings()
    {
        EditorApplication.delayCall += Apply;
    }

    private static void Apply()
    {
        const string productName = "El Mandoob - المندوب";

        if (PlayerSettings.productName != productName)
        {
            PlayerSettings.productName = productName;
        }

        // Only replace the upstream team's company label. If the project owner later
        // sets a real studio/company name, this script deliberately leaves it alone.
        if (PlayerSettings.companyName == "1xBest" ||
            PlayerSettings.companyName == "DefaultCompany")
        {
            PlayerSettings.companyName = "El Mandoob";
        }

        if (PlayerSettings.bundleVersion == "1.0.2" ||
            string.IsNullOrEmpty(PlayerSettings.bundleVersion))
        {
            PlayerSettings.bundleVersion = "0.1.0";
        }
    }
}
#endif
