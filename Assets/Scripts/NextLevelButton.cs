using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelButton : MonoBehaviour
{
    public void nextLevelButtonClick()
    {
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
