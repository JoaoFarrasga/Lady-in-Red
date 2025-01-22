using UnityEngine;
using System.Collections.Generic;

using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;


public class VolumeSettings : MonoBehaviour
{
    public Slider masterVol, musicVol, sfxVol;
    public AudioMixer mainAudioMixer;

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterVol", masterVol.value);
    }public void ChangeMuscVolume()
    {
        mainAudioMixer.SetFloat("MusicVol", musicVol.value);
    }public void ChangeSfxVolume()
    {
        mainAudioMixer.SetFloat("SfxVol", sfxVol.value);
    }
}
