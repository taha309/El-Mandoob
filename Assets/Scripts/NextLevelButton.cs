using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelButton : MonoBehaviour
{
    private void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        TMP_Text label = GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            ElMandoobBootstrap.ApplyArabicText(
                label,
                currentLevel >= 8 ? "القائمة الرئيسية" : "الشيفت اللي بعده");
        }
    }

    public void nextLevelButtonClick()
    {
        Time.timeScale = 1f;

        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        if (currentLevel >= 8)
        {
            SceneManager.LoadScene(0);
            return;
        }

        GameData data = SaveSystem.Load();
        int nextLevel = Mathf.Min(currentLevel + 1, data.levelUnlocked);
        nextLevel = Mathf.Clamp(nextLevel, 1, 8);

        PlayerPrefs.SetInt("SelectedLevel", nextLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene((nextLevel - 1) / 4 + 1);
    }
}
