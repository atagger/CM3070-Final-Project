using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    // menus
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject helpMenu;

    // audio
    [SerializeField] AudioSource mainMenuVoice;
    [SerializeField] AudioSource settingsMenuVoice;
    [SerializeField] AudioSource welcomeToSolulu;
    [SerializeField] AudioSource instructionProgram;

    // mixer reference for setting volume
    [SerializeField] AudioMixer masterMixer;
    [SerializeField] TextMeshProUGUI gameModeButtonText;

    // this is triggered by the difficulty button press in the menu screen
    public void ChangeDifficultyLevelPress()
    {
        if (GameManager.Instance.IsGameLevelEasy() == true)
        {
            GameManager.Instance.SetGameHard();
        }
        else
        {
            GameManager.Instance.SetGameEasy();

        }

        SetGameDifficultyText();
    }

    public void SetGameDifficultyText()
    {
        Debug.Log(GameManager.Instance.IsGameLevelEasy());

        if (GameManager.Instance.IsGameLevelEasy() == true)
        {
            Debug.Log("menu should say easy");
            gameModeButtonText.SetText("Easy");
        }
        else
        {
            Debug.Log("menu should say hard");
            gameModeButtonText.SetText("Hard");
        }
    }

    public void Start()
    {
        Debug.Log("game manager state is ------------>:" + GameManager.Instance.IsGameLevelEasy());
        // play welcome sound
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        welcomeToSolulu.Play();

        // if the game is not set to easy on the main menu, return it to easy since
        // the main menu always restarts the game
        if (!GameManager.Instance.IsGameLevelEasy())
        {
            GameManager.Instance.SetGameEasy();
        }

        // set the difficulty text on load
        SetGameDifficultyText();
    }

    public void PlayGame()
    {
        // buildIndices are in Unity file >> build settings
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level 1");
    }
    public void SettingsMenuPress()
    {
        // play voice sound
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        settingsMenuVoice.Play();

        // toggle settings menu
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
        helpMenu.SetActive(false);
    }

    public void MainMenuPress()
    {
        // play voice sound
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        mainMenuVoice.Play();

        // toggle main menu
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
        helpMenu.SetActive(false);
    }

    public void HelpMenuPress()
    {
        // play voice sound
        masterMixer.SetFloat("Menu Sounds Vol", GetEffectsVolume());
        instructionProgram.Play();

        // toggle main menu
        settingsMenu.SetActive(false);
        mainMenu.SetActive(false);
        helpMenu.SetActive(true);
    }
    public void QuitGame()
    {
        // quit game
        Application.Quit();
    }

    public float GetEffectsVolume()
    {
        // get stored effects volume to set voice volumes
        return PlayerPrefs.GetFloat("effectsVolume");
    }

}
