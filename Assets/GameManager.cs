using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    //make the GameManager persistent
    public static GameManager Instance;

    // game manager holds the basic race info - it is set to DONOTDESTROY so is persistent across all scenes
    // spawner sends the details to the cars
    // THIS MUST STAY HERE - moving it to the race manager will cause weird sync issues because of lack of persistence with Start()
    private int[] lapsInRace = new int[4] { 2, 3, 2, 3 };

    // track direction - this needs to stay here since some objects can call on it at start
    // THIS MUST STAY HERE - moving it to the race manager will cause weird sync issues because of lack of persistence with Start()
    private bool[] isTrackForward = new bool[4] { true, false, true, false };
    // set player steering radius for each level (25f is for first levels)
    private float[] steering = new float[4] { 60f, 60f, 25f, 25f };

    // order of racers (determines the placement on starting line and pole position)
    // NOTE: 0 is the pole position, 3 is the outside position in the back (worst spot on block)
    // this changes per level depending on the placements of racers
    // the first slot is the player car
    private int[] raceStartOrder = new int[4] { 3, 0, 1, 2 };

    // flag to check if the tutorial is completed (happens only on first time racing)
    [HideInInspector] public bool tutorialComplete = false;

    // the amount of upgrade points the player has
    private int upgradePointsEarned = 0;
    // the trophies the player has accumulated - shown on the upgrades screen
    [HideInInspector] public List<int> trophyChest = new List<int>();
    // flag for race before winning the game (can also be used for testing, see Start())
    [HideInInspector] public bool finalRace = false;
    // current level of player
    private int currentLevel = 0;

    // player settings object, this is a copy of the scritable object
    // we need this to keep persistence between levels of the upgrades
    [SerializeField] CarSettings playerSettings;
    public CarSettings playerCarTuningClone { get; private set; }

    // list in the inspector containing all the opponent settings in their default state
    // we copy these to a list of clones to use in game so they are not altered
    [HideInInspector] public List<CarSettings> opponentSettings;

    // opponent settings in the inspector
    [SerializeField] CarSettings opponentSettings1;
    [SerializeField] CarSettings opponentSettings2;
    [SerializeField] CarSettings opponentSettings3;

    // list to hold opponentTuningClones
    [HideInInspector] public List<CarSettings> opponentCarTuningClones;

    // each of the opponent car settings copies (these are instantiated)
    [HideInInspector] public CarSettings opponentCarTuningClone1;
    [HideInInspector] public CarSettings opponentCarTuningClone2;
    [HideInInspector] public CarSettings opponentCarTuningClone3;

    // default setting for difficulty (hard)
    [HideInInspector] public bool gameIsEasy;

    // vars to store what the player has spent on each upgrade
    public int maxSpeedPtsSpent { get; private set; }
    public int tiresPtsSpent { get; private set; }
    public int accelerationPtsSpent { get; private set; }
    public int suspensionPtsSpent { get; private set; }

    private void Awake()
    {

        // don't create another instance of the game manager
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // copy of the scriptable object we will use
        playerCarTuningClone = Instantiate(playerSettings);

        // adds the clones above to the opponentCarTuningClones list
        //opponentCarTuningClones.Clear();
        CreateOpponentClones();

        // set game to easy on awake
        SetGameEasy();
    
        // FOR TESTING ONLY -> shortcut to final race for testing
        //finalRace = true;
    }

    #region Game Difficulty Switchers

    public bool IsGameLevelEasy()
    {
        return gameIsEasy;
    }

    // this sets up the initial game state as EASY
    // this is set by the buttons on the initial race screen and also the difficulty menu button
    public void SetGameEasy()
    {

            for (int i = 0; i < opponentCarTuningClones.Count; i++)
            {
                // reduce all opponents max speed by #
                opponentCarTuningClones[i].maxSpeed -= 15f;
                Debug.Log("easy mode is active, opponent max speed is set to: " + opponentCarTuningClones[i].maxSpeed);
            }

            gameIsEasy = true;
    }

    // this sets up the initial game state as HARD
    // this is set by the buttons on the initial race screen and also the difficulty menu button
    public void SetGameHard()
    {
            for (int i = 0; i < opponentCarTuningClones.Count; i++)
            {
                opponentCarTuningClones[i].maxSpeed += 15f;
                Debug.Log("hard mode is active, opponent max speed is set to: " + opponentCarTuningClones[i].maxSpeed);
                Debug.Log("this is firing again");
            }

            gameIsEasy = false;
    }

    #endregion

    #region Opponent Tuning / Clones / OPPONENT UPGRADES

    // adds the opponent settings to the opponent settings list
    public void CreateOpponentClones()
    {
        // instantiate clones of the opponent settings
        // using a loop here breaks in the webGL build for some odd reason...
        // because of this we do it manually
        opponentCarTuningClone1 = Instantiate(opponentSettings1);
        opponentCarTuningClone2 = Instantiate(opponentSettings2);
        opponentCarTuningClone3 = Instantiate(opponentSettings3);

        // add the clones to the list
        opponentCarTuningClones.Add(opponentCarTuningClone1);
        opponentCarTuningClones.Add(opponentCarTuningClone2);
        opponentCarTuningClones.Add(opponentCarTuningClone3);

        Debug.Log("opponent car settings count is: " + opponentCarTuningClones.Count);
    }

    // upgrades all the opponents
    // this happens on confirmation of the upgrade screen when the user presses button
    // and before the next level is loaded
    public void UpgradeAllOpponents()
    {
        // clone all the opponent settings
        for (int i = 0; i < opponentCarTuningClones.Count; i++)
        {
            Debug.Log("opponent speed before upgrade: " + opponentCarTuningClones[i].maxSpeed);
            
            if (gameIsEasy)
            {
                opponentCarTuningClones[i].enginePower += 70f;
                opponentCarTuningClones[i].maxSpeed += 1f;
            }
            else
            {
                opponentCarTuningClones[i].enginePower += 10f;
                opponentCarTuningClones[i].maxSpeed += 2.5f;
            }

            opponentCarTuningClones[i].tireGrip += .001f;
            opponentCarTuningClones[i].tireMass += .01f;
            Debug.Log("opponent speed after upgrade: " + opponentCarTuningClones[i].maxSpeed);
        }
    }

    // clears tuning to default on game reset
    public void ClearTuningToDefault()
    {
        playerCarTuningClone = Instantiate(playerSettings);
        Debug.Log("player max speed after reset is: " + playerCarTuningClone.maxSpeed);

        for (int i = 0; i < opponentCarTuningClones.Count; i++)
        {
            Debug.Log("opponent max speed after reset is: " + opponentCarTuningClones[i].maxSpeed);
        }

    }

    // clears opponent tuning to default on game reset
    public void ClearOpponentTuningToDefault()
    {
        // clear the list
        opponentCarTuningClones.Clear();

        // add the copies back to the list
        CreateOpponentClones();
    }
    #endregion

    #region Upgrade Points
    public void ResetUpgradePoints()
    {
        // reset the upgrade point vals
        maxSpeedPtsSpent = 0;
        tiresPtsSpent = 0;
        accelerationPtsSpent = 0;
        suspensionPtsSpent = 0;
    }

    public void ReplenishUpgradePoints(int amount)
    {
        upgradePointsEarned = amount;
    }

    public int PtsSpentToDate()
    {
        int pointsSpent = maxSpeedPtsSpent + tiresPtsSpent + accelerationPtsSpent + suspensionPtsSpent;
        return pointsSpent;
    }

    // adding points for specific upgrades
    // needed to keep the state of the upgrades screen in UpgradesManager
    public void SpendMaxSpeedPoint()
    {
        maxSpeedPtsSpent++;
        Debug.Log(maxSpeedPtsSpent);
        RemoveUpgradePoint();
    }

    public void SpendTiresPoint()
    {
        tiresPtsSpent++;
        RemoveUpgradePoint();
    }

    public void SpendAccelerationPoint()
    {
        accelerationPtsSpent++;
        RemoveUpgradePoint();
    }

    public void SpendSuspensionPoint()
    {
        suspensionPtsSpent++;
        RemoveUpgradePoint();
    }

    // award points for upgrades based on rank
    public void AwardUpgradePoints(int raceRank)
    {
        // add upgrade point for winning
        if (upgradePointsEarned <= 16)
        {
            if (raceRank == 1)
            {
                // 1st place
                upgradePointsEarned += 4;
            }
            else if (raceRank == 2)
            {
                // 2nd place
                upgradePointsEarned += 2;

            }
            else if (raceRank == 3)
            {
                // 3rd place
                upgradePointsEarned += 1;

            }
        }
    }

    // remove upgrade points on spend
    public void RemoveUpgradePoint()
    {
        // remove upgrade point (done on upgrade screen)
        if (upgradePointsEarned > 0)
        {
            upgradePointsEarned--;
        }
    }

    // returns the amount of upgrade points player has
    public int GetUpgradePoints()
    {
        return upgradePointsEarned;
    }

    #endregion

    #region Tuning
    public void IncreaseMaxSpeed(float amt)
    {
        playerCarTuningClone.maxSpeed += amt;
    }

    public void IncreaseTireGrip(float amt)
    {
        playerCarTuningClone.tireGrip += amt;
    }

    public void IncreaseTireMass(float amt)
    {
        playerCarTuningClone.tireMass += amt;
    }

    public void IncreaseEnginePower(float amt)
    {
        playerCarTuningClone.enginePower += amt;
    }

    public void IncreaseSuspension(float amt)
    {
        playerCarTuningClone.restLength += amt;
    }

    public void IncreaseSpringStrength(float amt)
    {
        playerCarTuningClone.springStrength += amt;
    }

    #endregion

    #region Level Information Get / Set

    // returns the start race slots for when each race starts
    public int GetStartSlot(int slot)
    {
        return raceStartOrder[slot];
    }

    // sets the position of the cars for the race, this is done in the spawner
    // after each race is complete to determine the starting order of the cars
    public void SetStartSlot(int slot, int raceStartPosition)
    {
        raceStartOrder[slot] = raceStartPosition;
    }

    public void TutorialFinished()
    {
        tutorialComplete = true;
    }

    public float GetSteering()
    {
        // get the steering strength per level
        return steering[currentLevel];
    }

    public int GetNumLaps()
    {
        // we must subtract one because the levels start at 1, not 0
        return lapsInRace[currentLevel];
    }

    // triggered when race is finished and player qualified
    public void SetNextLevel()
    {
        if (currentLevel < lapsInRace.Length)
        {
            // up the level (this is used as the array lookup for # laps)
            currentLevel++;
        }

        // END GAME CONDITION
        // we are on the final level
        if (currentLevel == GetFinalLevel())
        {
            // when we get to the trophy screen with this flag the game "WIN" screen will display
            finalRace = true;
            Debug.Log("FINAL RACE CONDITION STARTED");
        }
    }

    public bool GetTrackDirection()
    {
        // returns the track direction of the current level
        return isTrackForward[currentLevel];
    }

    public int GetLevel()
    {
        // return the level
        return currentLevel;
    }

    public int GetTotalLevelsNum()
    {
        // return the level
        return lapsInRace.Length;
    }

    public int GetFinalLevel()
    {
        // return the level
        return lapsInRace.Length;
    }

    #endregion

    #region Trophy Chest

    public void AddTrophy(int rank)
    {
        // we must subtract one because the levels start at 1, not 0
        trophyChest.Add(rank);
    }

    #endregion

    #region Reload / ResetGame

    // reload the scene, needed for when we have a new race
    public void Reload()
    {
        if (currentLevel < lapsInRace.Length)
        {
            Time.timeScale = 1f;
            //-----------------------------------------------------------------------
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } 
    }

    public void ResetTutorialScreen()
    {
        // flag to show the tutorial on restarting level 1
        tutorialComplete = false;
    }


    // called by the upgrades manager when finishing the game
    // also called by the menu when quitting to main menu
    public void ResetGame()
    {
        // reset the level and tophies counter back to the initial levels
        // this happens when we win or quit to the main menu
        currentLevel = 0;
        upgradePointsEarned = 0;
        finalRace = false;
        trophyChest.Clear();

        // clear tuning to default for player and opponents
        ClearTuningToDefault();
        ClearOpponentTuningToDefault();

        // reset the tutorial screen so it shows up the next time the player starts level 1
        ResetTutorialScreen();

        // get rid of any upgrade points the player had
        ResetUpgradePoints();

        // when we start the game the first time the game mananger sets gameIsEasy = true
        // ----> this happens only once on Awake
        // because we are resetting the game without awake we have to reset this flag to false
        // so that upgrade points are maintained and we maintain the same state as when the game
        // launches the first time since Awake is not launched again
        gameIsEasy = false;

        // reset the starting block order so the player is late
        raceStartOrder = new int[4] { 3, 0, 1, 2 };
    }

    #endregion

}
