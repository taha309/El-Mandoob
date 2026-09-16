using UnityEngine;
using UnityEngine.SceneManagement;

public class taskExit : MonoBehaviour
{
    public void exitTask()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
