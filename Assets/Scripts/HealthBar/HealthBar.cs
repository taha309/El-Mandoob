using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxHealth(int health)
    {
        if (slider == null)
        {
            return;
        }

        int safeHealth = Mathf.Max(1, health);
        slider.maxValue = safeHealth;
        slider.value = safeHealth;
        UpdateFill();
    }

    public void SetHealth(int health)
    {
        if (slider == null)
        {
            return;
        }

        slider.value = Mathf.Clamp(health, 0, Mathf.RoundToInt(slider.maxValue));
        UpdateFill();
    }

    private void UpdateFill()
    {
        if (fill != null && gradient != null && slider != null)
        {
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }
}
