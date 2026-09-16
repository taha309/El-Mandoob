using UnityEngine;
using UnityEngine.Audio;

public class VolumeManager : MonoBehaviour
{
    public AudioMixer mixer;

    public GameObject musicOn;
    public GameObject musicOff;
    public GameObject slider;

    public GameData data;

    private void Start()
    {
        data = SaveSystem.Load();
        AudioListener.volume = data.audios == 0 ? 0f : 1f;

        if (mixer != null)
        {
            mixer.SetFloat("musicvol", data.volume);
        }

        if (musicOn != null)
        {
            musicOn.SetActive(data.audios != 0);
        }
        if (musicOff != null)
        {
            musicOff.SetActive(data.audios == 0);
        }
    }

    public void SetLevel(float sliderValue)
    {
        EnsureData();
        if (mixer != null)
        {
            mixer.SetFloat("musicvol", sliderValue);
        }
        data.volume = sliderValue;
        SaveSystem.Save(data);
    }

    public void btnOn()
    {
        EnsureData();
        AudioListener.volume = 0;
        if (musicOn != null) musicOn.SetActive(false);
        if (musicOff != null) musicOff.SetActive(true);
        data.audios = 0;
        SaveSystem.Save(data);
    }

    public void btnOff()
    {
        EnsureData();
        AudioListener.volume = 1;
        if (musicOn != null) musicOn.SetActive(true);
        if (musicOff != null) musicOff.SetActive(false);
        data.audios = 1;
        SaveSystem.Save(data);
    }

    private void EnsureData()
    {
        if (data == null)
        {
            data = SaveSystem.Load();
        }
    }
}
