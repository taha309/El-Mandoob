using UnityEngine;

public class ContBtnHandler : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseBtn;
    public GameObject joystick;
    public HealthBar healthBar;
    public GameObject itemDelivered;
    public CountdownTimer timer;

    public void Continue()
    {
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseBtn != null) pauseBtn.SetActive(true);
        if (joystick != null) joystick.SetActive(true);
        if (healthBar != null) healthBar.gameObject.SetActive(true);
        if (itemDelivered != null) itemDelivered.SetActive(true);
        if (timer != null) timer.counting = true;

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.enabled = true;
            player.RefreshInteractionButtons();
        }

        ElMandoobHUD hud = FindObjectOfType<ElMandoobHUD>();
        if (hud != null)
        {
            hud.SetPaused(false);
        }
    }
}
