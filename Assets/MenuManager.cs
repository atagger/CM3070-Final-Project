using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // references to the menus
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsMenuCanvas;
    [SerializeField] private GameObject helpMenuCanvas;

    // first items for menu selection -- needed for scrolling with keys
    [SerializeField] private GameObject mainMenuFirst;
    [SerializeField] private GameObject settingsMenuFirst;
    [SerializeField] private GameObject helpMenuFirst;

    [SerializeField] private AudioMixerSnapshot pausedAudio;
    [SerializeField] private AudioMixerSnapshot unpausedAudio;

    // paused audio voice sound
    [SerializeField] private AudioSource pausedMenuVoice;
    [SerializeField] private AudioSource settingsMenuVoice;
    [SerializeField] private AudioSource practiceMakesPerfectMenuVoice;
    [SerializeField] private AudioSource tryAgainMenuVoice;

    [SerializeField] private AudioMixer masterMixer;

    // paused state
    private bool isPaused;

    // save state of effects volume
    // the paused voice must not be affected by the transition filters
    // because of this it is in its own audio group
    private float menuVoiceVol;

    [SerializeField] TextMeshProUGUI gameModeButtonText;

    public void ChangeDifficulty()
    {
        if (GameManager.Instance.IsGameLevelEasy() == true)
        {
            GameManager.Instance.SetGameHard();
        } else {
            GameManager.Instance.SetGameEasy();

        }

        SetGameDifficultyText();
    }

    public void SetGameDifficultyText()
    {
        if (GameManager.Instance.IsGameLevelEasy() == true)
        {
            gameModeButtonText.SetText("Easy");
        }
        else
        {
            gameModeButtonText.SetText("Hard");
        }
    }

    private void Start()
    {
        // set both menus to inactive
        mainMenuCanvas.SetActive(false);
        settingsMenuCanvas.SetActive(false);
        SetGameDifficultyText();
    }

    private void Update()
    {
        // if the actions is triggered either pause or unpause, and show menus
        if (InputManager.instance.MenuOpenInput)
        {
            if (!isPaused)
            {
                PausedAudioFilter();
                Pause();
            }
        
        } 
        else if(InputManager.instance.MenuCloseInput)
        {
            if (isPaused)
            {
                UnPausedAudioFilter();
                UnPause();
            }
        }
    }

    public void PausedAudioFilter()
    {
        // low pass filter audio
        pausedAudio.TransitionTo(.01f);
    }

    public void UnPausedAudioFilter()
    {
        // remove low pass filter audio
        unpausedAudio.TransitionTo(.01f);
    }

    public float GetEffectsVolume()
    {
        return PlayerPrefs.GetFloat("effectsVolume");
    }
    public void Pause()
    {
        isPaused = true;

        // set the volume before we filter it for the paused menu voice
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        // main menu voice sound
        pausedMenuVoice.Play();

        // freeze time
        Time.timeScale = 0f;
        // set input manager to UI so we can use the keys
        InputManager.PlayerInput.SwitchCurrentActionMap("UI");
        OpenMainMenu();
    }

    public void UnPause()
    {
        isPaused = false;
        // reset time
        Time.timeScale = 1f;
        // set input manager back to the game play scheme
        InputManager.PlayerInput.SwitchCurrentActionMap("Player");
        CloseAllMenus();
    }

    public void OpenMainMenu()
    {

        // show main menu
        mainMenuCanvas.SetActive(true);
        // hide settings menu
        settingsMenuCanvas.SetActive(false);
        // set first menu item
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }

    public void OpenMainMenuFromSettings()
    {

        // show main menu
        mainMenuCanvas.SetActive(true);
        // hide settings menu
        helpMenuCanvas.SetActive(false);
        // set first menu item
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }

    public void OpenSettingsMenu()
    {

        // set the volume before we filter it for the settings menu voice
        // settings menu voice sound
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        settingsMenuVoice.Play();

        // hide main menu
        mainMenuCanvas.SetActive(false);
        // show settings menu
        settingsMenuCanvas.SetActive(true);
        // set first menu item
        EventSystem.current.SetSelectedGameObject(settingsMenuFirst);
    }

    public void OpenHelpMenu()
    {

        // hide main menu
        mainMenuCanvas.SetActive(false);
        // show settings menu
        helpMenuCanvas.SetActive(true);
        // set first menu item
        EventSystem.current.SetSelectedGameObject(helpMenuFirst);
    }

    public void CloseAllMenus()
    {
        mainMenuCanvas.SetActive(false);
        settingsMenuCanvas.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnSettingsPress()
    {
        // open settings menu
        OpenSettingsMenu();
    }

    public void OnHelpPress()
    {
        // open help menu
        OpenHelpMenu();
    }

    public void OnResumePress()
    {
        // resume menu voice sound
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        practiceMakesPerfectMenuVoice.Play();

        // going back to home we have to return the audio back to normal
        UnPausedAudioFilter();
        // unpause the game
        UnPause();
    }

    public void OnSettingsBackPress()
    {
        // open the in game menu
        OpenMainMenu();
    }

    public void OnHelpBackPress()
    {
        // open the in game menu
        OpenMainMenuFromSettings();
    }

    public void OnBackToHomeScreen()
    {
        // going back to home we have to return the audio back to normal
        UnPausedAudioFilter();
        // clear all tuning to default
        GameManager.Instance.ResetGame();
        // goes back to the home screen menu
        SceneManager.LoadScene("Home");
        Time.timeScale = 1f;
    }

    // game state foward buttons (happens when going to next level or restarting current level)
    public void OnRestartPressFromMenu()
    {
        // turn off low pass filter
        UnPausedAudioFilter();
        GameManager.Instance.Reload();
    }

}
