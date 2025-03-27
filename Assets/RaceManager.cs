using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // text mesh pro for fonts    
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.XR;
using UnityEngine.AI; // for reloading scene

// coordinates races
public class RaceManager : MonoBehaviour
{
    // ---------------------------------------------------
    // NOTE: race track laps are set in the GAME MANAGER
    // NOTE: race track direction is set in the GAME MANAGER
    // ----------------------------------------------------

    // finish line group, always placed on the first node
    [SerializeField] GameObject finishLine;

    // race track graphics
    [SerializeField] List<GameObject> raceTracks;

    // track nodes
    [SerializeField] List<Transform> trackNodes;

    // the main camera for the birds-eye view for the level
    [SerializeField] List<int> cameraForLevel;

    // race track names
    [SerializeField] List<string> raceTrackNames;

    [SerializeField] List<GameObject> cameraOriginsForward;
    [SerializeField] List<GameObject> cameraOriginsBackward;

    // list of the music for each level
    [SerializeField] List<AudioSource> levelMusic;

    // list of the voice announcements for each level
    [SerializeField] List<AudioSource> levelAnnouncements;

    // countdown animation
    [SerializeField] Animator animator;
    // game started event
    [HideInInspector] public UnityEvent startGame;
    // game started flag
    private bool gameStarted = false;
    // the amount of laps in the race
    public int lapsInRace { get; private set; }

    // tutorial screens
    [SerializeField] GameObject tutorialParent;
    [SerializeField] GameObject tutorialScreen1;
    [SerializeField] GameObject tutorialScreen2;
    // tutorial screen instruction voice
    [SerializeField] AudioSource instructionVoice;

    // difficulty setting screen
    [SerializeField] GameObject difficultyScreen;
    // difficulty setting screen sound effect
    [SerializeField] AudioSource twinkleSound;

    // text for if you won or lost
    [SerializeField] Button qualifiedButton;
    // button to start race over on disqualification
    [SerializeField] Button disqualifiedButton;
    // button to start next race because you placed in top 3
    [SerializeField] TextMeshProUGUI finalRaceCTAText;

    // the final race leaderboard after completion
    [SerializeField] private GameObject raceResultsCanvas;

    // text field for race results
    public TextMeshProUGUI UIRaceResults;

    // animator component that starts the race
    [SerializeField] Animator countDown;

    // the camera manager for switching cameras
    [SerializeField] CameraManager cameraManager;

    // the camera manager for switching cameras
    [SerializeField] GameObject racingHUD;

    // level info UI panel to hide / show
    [SerializeField] GameObject levelInfo;

    // button to start next race because you placed in top 3
    [SerializeField] TextMeshProUGUI levelInfoLevel;

    // button to start next race because you placed in top 3
    [SerializeField] TextMeshProUGUI levelInfoLaps;

    // button to start next race because you placed in top 3
    [SerializeField] TextMeshProUGUI levelNum;

    // needed to set difficulty button in main menu
    [SerializeField] MenuManager menuManager;

    // player rank
    private int playerRank;
    // level index
    private int levelIndex;

    private void Awake()
    {
        // PAUSE the race countdown when we start the game
        // get animator component
        countDown = countDown.GetComponent<Animator>();
        // pause countdown animation
        countDown.speed = 0f;
    }

    public void StartCountDown()
    {
        // start the countdown --> the race camera starts this when it zooms into cars
        countDown.speed = 1f;
    }

    public void StartLeveLPress()
    {
        // switch to the race camera (follows the car)
        cameraManager.RaceCam();
        // hide the level info on zoom
        levelInfo.SetActive(false);
        // show the racing HUD
        racingHUD.SetActive(true);
    }

    public void Start()
    {
        // get the level index from game manager
        levelIndex = GameManager.Instance.GetLevel();

        //Debug.Assert(trackNodes.Count == GameManager.Instance.GetTotalLevelsNum(),"WARNING! Track nodes not equal to the number of levels");
        //Debug.Assert(levelMusic.Count == GameManager.Instance.GetTotalLevelsNum(), "WARNING! Music tracks not equal to the number of levels");
        //Debug.Assert(levelAnnouncements.Count == GameManager.Instance.GetTotalLevelsNum(), "WARNING! Level annoucements not equal to the number of levels");
        //Debug.Assert(raceTracks.Count == GameManager.Instance.GetTotalLevelsNum(), "WARNING! Race tracks not equal to the number of levels");
        //Debug.Assert(cameraForLevel.Count == GameManager.Instance.GetTotalLevelsNum(), "WARNING! Cameras not equal to the number of levels");
        //Debug.Assert(cameraOriginsForward.Count == GameManager.Instance.GetTotalLevelsNum(), "WARNING! Camera origins forward not equal to the number of levels");
        //Debug.Assert(cameraOriginsBackward.Count == GameManager.Instance.GetTotalLevelsNum(), "WARNING! Camera origins backward not equal to the number of levels");

        // hide all the race tracks except the one we are racing on
        for (int i = 0; i < raceTracks.Count; i++)
        {
            if (i != levelIndex)
            {
                raceTracks[i].SetActive(false);
            }
        }

        // show the track we are using for the current level
        raceTracks[levelIndex].SetActive(true);

        // NOTE: NEED TO TURN FINISH LINE TOWARDS TRACK DIRECTION, not working!
        Vector3 finishLineLoc = trackNodes[levelIndex].GetChild(0).GetComponent<Transform>().position;

        // get the next node if ware driving in the normal direction, the finish line will face this
        Vector3 nextNode = trackNodes[levelIndex].GetChild(1).GetComponent<Transform>().position;

        // get the last index and node of the track in case we are racing in the opposite direction
        // the finish line will face this node
        int lastIndex = trackNodes[levelIndex].transform.childCount - 1;
        Vector3 lastNode = trackNodes[levelIndex].GetChild(lastIndex).GetComponent<Transform>().position;

        // target for storing where to face the finish line toward
        Vector3 finishLineLookTarget;

        if (GameManager.Instance.GetTrackDirection())
        {
            // going in normal direction, use the next node to point the finish line toward
            finishLineLookTarget = new Vector3(nextNode.x, 0f, nextNode.z);
        } else
        {
            //going in the opposite direction, use the last node to point the finish line toward
            finishLineLookTarget = new Vector3(lastNode.x, 0f, lastNode.z);
        }

        // set the finish line position to the first node
        finishLine.transform.position = new Vector3(finishLineLoc.x, 0.1f, finishLineLoc.z);

        // rotate the finish line to face either the first or last tracks node, depending on track driving direction
        finishLine.transform.LookAt(finishLineLookTarget);

        // get the countdown animator
        Animator animator = GetComponent<Animator>();
        raceResultsCanvas.SetActive(false);

        // get the number of laps from the persistent game manager instance
        lapsInRace = GameManager.Instance.GetNumLaps();

        // hide the racing hud
        racingHUD.SetActive(false);

        // get the level name for display on the birds-eye view screen before play
        levelInfoLevel.text = raceTrackNames[levelIndex];

        levelNum.text = "LEVEL " + (levelIndex + 1).ToString() 
            + " / " + GameManager.Instance.GetTotalLevelsNum().ToString();

        if (lapsInRace <= 1) levelInfoLaps.text = lapsInRace.ToString() + " lap";
        else levelInfoLaps.text = lapsInRace.ToString() + " laps";

        // show level info text
        levelInfo.SetActive(true);

        // play level and 
        levelMusic[levelIndex].Play();
        levelAnnouncements[levelIndex].Play();

        // pause screen for n seconds before starting game
        StartCoroutine(GameStartCountdownRoutine(3f));

        // switch camera to the appropriate one for the level interstitial
        // this is set via the inspector
        cameraManager.CameraSwitcher(cameraForLevel[levelIndex]);
    }

    // countdown screen before race showing the level design
    // from a birds-eye perspective
    IEnumerator GameStartCountdownRoutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        // zoom into players and start race when zoom finishes
        cameraManager.RaceCam();
        // hide the level info on zoom
        levelInfo.SetActive(false);
        // show the racing HUD
        racingHUD.SetActive(true);
    }

    public void Update()
    {
        // gets how much of animation is completed via GetCurrentAnimatorStateInfo(0)
        float normalizedAnimationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        // countdown animation finished and game has not started
        if(normalizedAnimationTime >= 1f && !gameStarted)
        {
            //Debug.Log("start animation ended");
            // STARTS THE GAME FOR ALL PLAYERS AFTER ANIMATION ENDS IN THE VEHICLE CLASS
            // startGame is an anonymous unity event that all the vehicles are subscribed to
            // when the animation is finished, the vehicles call the BeginGameForPlayers function in the Vehicle class 
            startGame.Invoke();
            gameStarted = true;
        }
    }

    #region Start Tutorial / Difficulty
    public void ShowTutorial()
    {
        // set by the race camera script the first time game is played
        // shows the fist tutorial screen
        instructionVoice.Play();
        tutorialParent.SetActive(true);
        tutorialScreen1.SetActive(true);
    }

    // shows the second slide of the tutorial screen
    public void ShowTutorial2()
    {
        // hide the first screen, show the second
        tutorialScreen1.SetActive(false);
        tutorialScreen2.SetActive(true);
    }

    // set by the race camera script the first time game is played
    // shows the difficulty settings screen
    public void ShowDifficultyScreen()
    {
        tutorialParent.SetActive(true);
        twinkleSound.Play();

        // hide the first screen, show the second
        tutorialScreen1.SetActive(false);
        tutorialScreen2.SetActive(false);
        difficultyScreen.SetActive(true);
    }

    // hide the difficulty screen
    public void HideDifficultyScreen()
    {
        tutorialParent.SetActive(false);
        difficultyScreen.SetActive(false);
    }

    // this is triggered by the orange "HARD" button the first time the player starts a race
    // it only shows this screen on the first level
    // make the game hard for the player (uses default values in scriptable objects)
    public void SetHardGamePress()
    {
        // only set it to hard if it's not currently set to it
        if (GameManager.Instance.gameIsEasy != false) GameManager.Instance.SetGameHard();
        // set the button in the menu to reflect current state
        menuManager.SetGameDifficultyText();
        // hides the screen and cleans up
        FinishDifficultyScreen();
    }

    // this is triggered by the orange "EASY" button the first time the player starts a race
    // it only shows this screen on the first level
    // make the game easy for the player
    public void SetEasyGamePress()
    {
        // only set it to easy if it's not currently set to it
        if (GameManager.Instance.gameIsEasy != true) GameManager.Instance.SetGameEasy();
        // set the button in the menu to reflect current state
        menuManager.SetGameDifficultyText();
        // hides the screen and cleans up
        FinishDifficultyScreen();
    }

    // clean up after hiding the difficulty setting screen
    public void FinishDifficultyScreen()
    {
        HideDifficultyScreen();
        // set the tutorial as complete, so we don't see the screen again
        GameManager.Instance.TutorialFinished();
        StartCountDown();
    }

    // unused: used to clean up after hiding the tutorial screen
    public void FinishTutorial()
    {
        // set by the race camera script the first time game is played
        tutorialScreen2.SetActive(false);
        tutorialParent.SetActive(false);
        // set the tutorial as complete, so we don't see the screen again
        GameManager.Instance.TutorialFinished();
        // start the race
        StartCountDown();
    }

    #endregion

    #region Getters
    public Vector3 GetForwardCamPos(int levelIndex)
    {
        // returns the track direction of the current level
        return cameraOriginsForward[levelIndex].transform.position;
    }

    public Quaternion GetForwardCamRot(int levelIndex)
    {
        // returns the track direction of the current level
        return cameraOriginsForward[levelIndex].transform.rotation;
    }

    public Vector3 GetBackwardsCamPos(int levelIndex)
    {
        // returns the track direction of the current level
        return cameraOriginsBackward[levelIndex].transform.position;
    }
    public Quaternion GetBackwardsCamRot(int levelIndex)
    {
        // returns the track direction of the current level
        return cameraOriginsBackward[levelIndex].transform.rotation;
    }
    #endregion

    #region Track Nodes
    public List<Transform> GetTrackNodes()
    {
        // get the track nodes
        int levelIndex = GameManager.Instance.GetLevel();
        Transform[] path_objs = trackNodes[levelIndex].GetComponentsInChildren<Transform>();
        List<Transform> nodes = new List<Transform>();

        for (int i = 0; i < path_objs.Length; i++)
        {
            // don't add the parent object to list
            if (path_objs[i] != trackNodes[levelIndex].transform)
            {
                nodes.Add(path_objs[i]);
            }
        }

        // this reverses the nodes if going in backwards direction
        if (GameManager.Instance.GetTrackDirection() == false)
        {
            // reversed the nodes
            nodes.Reverse();
        }

        return nodes;
    }

    #endregion

    #region Restarts and Presses
    public void RestartLevel()
    {
        //-- NOTE: RESETTING THE TIME SCALE IS CRITICAL TO SCENE RELOAD!!! --/
        // REF: https://discussions.unity.com/t/animation-not-working-after-reload-scene/65799
        //-----------------------------------------------------------------------
        Time.timeScale = 1f;
        //-----------------------------------------------------------------------
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        //InputManager.PlayerInput.SwitchCurrentActionMap("Player");
    }

    public void NextLevel()
    {
        //-- NOTE: RESETTING THE TIME SCALE IS CRITICAL TO SCENE RELOAD!!! --/
        // REF: https://discussions.unity.com/t/animation-not-working-after-reload-scene/65799
        //-----------------------------------------------------------------------
        Time.timeScale = 1f;
        //-----------------------------------------------------------------------
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        //InputManager.PlayerInput.SwitchCurrentActionMap("Player");
    }

    public void ContinuePress()
    {
        // award upgrade points based on player rank
        GameManager.Instance.AwardUpgradePoints(playerRank);
        GameManager.Instance.AddTrophy(playerRank);

        // continue to tropy / upgrades screen
        SceneManager.LoadScene("Upgrades");
    }

    #endregion

    #region Rankings and Race Results

    // shows the final race results canvas when race is completed
    public void ShowRaceResults(string resultsText)
    {
        // show the results
        raceResultsCanvas.SetActive(true);
        // populate it with the text
        UIRaceResults.text = resultsText;
        InputManager.PlayerInput.SwitchCurrentActionMap("UI");

    }

    public void SetPlayerRank(int rank)
    {
        playerRank = rank;
        string textRank = rank.ToString();

        // attach prefixes to numberical rank
        if (rank == 1)
        {
            textRank = textRank + "st";
        }
        else if (rank == 2)
        {
            textRank = textRank + "nd";
        }
        else if (rank == 3)
        {
            textRank = textRank + "rd";
        }
        else if (rank >= 4)
        {
            textRank = textRank + "th";
        }

        if (rank <= 3)
        {
            // player qualified
            GameManager.Instance.SetNextLevel();
            finalRaceCTAText.text = "You scored " + textRank + " place!";

            if (GameManager.Instance.GetLevel() < GameManager.Instance.GetFinalLevel())
            {
                disqualifiedButton.transform.gameObject.SetActive(false);
                // show button to continue to upgrades
                qualifiedButton.transform.gameObject.SetActive(true);
            }
            else
            {
                // player WON GAME here
                // don't show any buttons we won the game
                disqualifiedButton.transform.gameObject.SetActive(false);
                qualifiedButton.transform.gameObject.SetActive(true);
            }
        }
        else
        {
            // player did not qualify, they must start race over
            finalRaceCTAText.text = "You did not place!";
            // show button to restart race
            disqualifiedButton.transform.gameObject.SetActive(true);
            qualifiedButton.transform.gameObject.SetActive(false);
        }
    }

    #endregion
}
