using UnityEngine;
using UnityEngine.UI;

public class pauseBtnHandler : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseBtn;
    public GameObject joystick;
    public GameObject taskList;
    public HealthBar healthBar;
    public GameObject itemsDelivered;
    public Button deliverButton, receiveButton;
    public CountdownTimer timer;

    public void pauseGame()
    {
        if (taskList != null) taskList.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (pauseBtn != null) pauseBtn.SetActive(false);
        if (joystick != null) joystick.SetActive(false);
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (itemsDelivered != null) itemsDelivered.SetActive(false);
        if (deliverButton != null) deliverButton.gameObject.SetActive(false);
        if (receiveButton != null) receiveButton.gameObject.SetActive(false);
        if (timer != null) timer.counting = false;

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.enabled = false;
        }

        ElMandoobHUD hud = FindObjectOfType<ElMandoobHUD>();
        if (hud != null)
        {
            hud.SetPaused(true);
        }

        Time.timeScale = 0f;
    }
}
