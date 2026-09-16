using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public GameObject textDisplay;
    public int secondsLeft = 0;
    public bool takingAway = false; // Kept for prefab/backward compatibility.
    public bool counting = false;
    public int minutesLeft = 4;

    private TextMeshProUGUI textMeshPro;
    private float secondAccumulator;
    private bool finished;

    void Awake()
    {
        ResolveText();
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!counting || finished)
        {
            return;
        }

        secondAccumulator += Time.deltaTime;
        while (secondAccumulator >= 1f && counting && !finished)
        {
            secondAccumulator -= 1f;
            TickOneSecond();
        }
    }

    public void SetTime(int minutes, int seconds)
    {
        minutesLeft = Mathf.Max(0, minutes);
        secondsLeft = Mathf.Clamp(seconds, 0, 59);
        secondAccumulator = 0f;
        finished = false;
        takingAway = false;
        UpdateTimerDisplay();
    }

    private void TickOneSecond()
    {
        int totalSeconds = Mathf.Max(0, minutesLeft * 60 + secondsLeft);
        if (totalSeconds <= 0)
        {
            FinishTimer();
            return;
        }

        totalSeconds--;
        minutesLeft = totalSeconds / 60;
        secondsLeft = totalSeconds % 60;
        UpdateTimerDisplay();

        if (totalSeconds <= 0)
        {
            FinishTimer();
        }
    }

    private void FinishTimer()
    {
        if (finished)
        {
            return;
        }

        finished = true;
        counting = false;
        secondAccumulator = 0f;

        GamePlayManager manager = FindObjectOfType<GamePlayManager>();
        if (manager != null)
        {
            manager.endGame();
        }
        else
        {
            Debug.LogWarning("El Mandoob: timer ended but no GamePlayManager was found.");
        }
    }

    private void ResolveText()
    {
        if (textDisplay != null)
        {
            textMeshPro = textDisplay.GetComponent<TextMeshProUGUI>();
        }

        if (textMeshPro == null)
        {
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (textMeshPro == null)
        {
            ResolveText();
        }

        if (textMeshPro != null)
        {
            textMeshPro.text = minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00");
        }
    }
}
