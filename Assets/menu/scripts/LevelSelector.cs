using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public GameObject levelButton;
    public int level;

    public void SelectLevel()
    {
        if (levelButton == null || !int.TryParse(levelButton.name, out level))
        {
            Debug.LogWarning("El Mandoob: level button has no valid numeric name.");
            return;
        }

        GameData data = SaveSystem.Load();
        if (level > data.levelUnlocked)
        {
            Debug.Log("El Mandoob: shift " + level + " is still locked.");
            return;
        }

        PlayerPrefs.SetInt("SelectedLevel", level);
        PlayerPrefs.Save();
        SceneManager.LoadScene((level - 1) / 4 + 1);
    }
}
