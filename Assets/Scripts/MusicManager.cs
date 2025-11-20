using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    public Slider volumeSlider;

    void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);
        volumeSlider.value = musicSource.volume; // sync slider with audio
    }

    void SetVolume(float value)
    {
        musicSource.volume = value;
    }
}