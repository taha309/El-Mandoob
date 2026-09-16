using UnityEngine;

public class taskReadHandler : MonoBehaviour
{
    public GameObject taskCanvas;
    public GameObject pauseBtn;
    public GameObject joystick;
    public HealthBar healthBar;
    public GameObject itemDelivered;
    public CountdownTimer timer;
    public GameObject timerDisplay;

    public void taskRead()
    {
        Time.timeScale = 1f;

        if (taskCanvas != null) taskCanvas.SetActive(false);
        if (pauseBtn != null) pauseBtn.SetActive(true);
        if (joystick != null) joystick.SetActive(true);
        if (healthBar != null) healthBar.gameObject.SetActive(true);
        if (itemDelivered != null) itemDelivered.SetActive(true);
        if (timerDisplay != null) timerDisplay.SetActive(true);

        if (timer != null)
        {
            timer.counting = true;
        }
    }
}
