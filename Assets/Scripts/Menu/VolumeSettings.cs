using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{

    public Slider slider;
    public float SliderValue;
    public Image ImageMute;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("VolumeAudio", 0.5f);
        AudioListener.volume = slider.value;
        Mute();
    }

    public void ChangeSlider(float value)
    {
        slider.value = value;
        PlayerPrefs.SetFloat("VolumeAudio", SliderValue);
        AudioListener.volume = slider.value;
        Mute();
    }
    public void Mute()
    {
        if (slider.value == 0)
        {
            ImageMute.enabled = true;
        }
        else
        {
            ImageMute.enabled= false;
        }
    }
}
