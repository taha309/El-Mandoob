using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSpawn : MonoBehaviour
{
    public AudioMixer mixer;
    public GameObject musicOn;
    public GameObject musicOff;
    public GameObject slider;
    public GameData data;

    void Awake()
    {
        data = SaveSystem.Load();

        if (slider != null)
        {
            Slider sliderComponent = slider.GetComponent<Slider>();
            if (sliderComponent != null)
            {
                sliderComponent.value = data.volume;
            }
        }

        if (mixer != null)
        {
            mixer.SetFloat("musicvol", data.volume);
        }

        AudioListener.volume = data.audios == 0 ? 0f : 1f;

        if (musicOn != null) musicOn.SetActive(data.audios != 0);
        if (musicOff != null) musicOff.SetActive(data.audios == 0);
    }
}
