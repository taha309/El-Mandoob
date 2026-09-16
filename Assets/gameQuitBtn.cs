using UnityEngine;
using UnityEngine.SceneManagement;

public class gameQuitBtn : MonoBehaviour
{
    public void saveAndQuit()
    {
        // El Mandoob saves progression on every completed delivery/shift.
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
