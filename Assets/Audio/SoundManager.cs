using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class SoundManager : MonoBehaviour
{
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider effectsVolumeSlider;
    [SerializeField] AudioMixer masterMixer;
    [SerializeField] Toggle muteButton;
    [SerializeField] TextMeshProUGUI muteButtonText;

    void Start()
    {
        // it does not have a saved pref fo rmusic
        if (PlayerPrefs.HasKey("musicVolume") == false)
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
        }

        // it does not have a saved pref for effects
        if (PlayerPrefs.HasKey("effectsVolume") == false)
        {
            PlayerPrefs.SetFloat("effectsVolume", 1);
        }

        // load previous settings
        Load();

        // does not have a saved pref for mute
        if (PlayerPrefs.HasKey("mute") == false)
        {
            MuteOff();

        } 
        else
        {
            if (PlayerPrefs.GetInt("mute") == 1)
            {
                MuteOn();
            }
            else
            {
                MuteOff();
            }
        }
    }

    public void toggleMute()
    {
        if (muteButton.isOn)
        {
            Debug.Log("mute is on");
            MuteOn();
        }
        else
        {
            Debug.Log("mute is off");
            MuteOff();
        }
    }

    private void MuteOn()
    {
        muteButton.isOn = true;
        AudioListener.volume = 0;
        PlayerPrefs.SetInt("mute", 1);
        muteButtonText.SetText("MUTE ON");
    }

    private void MuteOff()
    {
        muteButton.isOn = false;
        AudioListener.volume = 1;
        PlayerPrefs.SetInt("mute", 0);
        muteButtonText.SetText("MUTE OFF");
    }
    private void Load()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
        effectsVolumeSlider.value = PlayerPrefs.GetFloat("effectsVolume");
    }
    public void ChangeVolumeMusic()
    {
        // this is set in mixer (exposed variables <-- very much hidden)
        // check this video by Unity as a refresher, because it is confusing
        // REF: https://www.youtube.com/watch?v=2nYyws0qJOM
        masterMixer.SetFloat("MusicVol", musicVolumeSlider.value);
        SaveMusicSetting();
    }

    public void ChangeVolumeEffects()
    {
        // this is set in mixer (exposed variables)
        masterMixer.SetFloat("SoundEffectsVol", effectsVolumeSlider.value);
        SaveFxSetting();
    }

    private void SaveMusicSetting()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolumeSlider.value);
    }

    private void SaveFxSetting()
    {
        PlayerPrefs.SetFloat("effectsVolume", effectsVolumeSlider.value);
    }

}
