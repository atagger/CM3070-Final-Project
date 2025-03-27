using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UpgradesManager : MonoBehaviour
{
    // upggrades text field
    [SerializeField] TextMeshProUGUI upgradesAmtText;
    [SerializeField] Slider maxSpeedSlider;
    [SerializeField] Slider tiresSlider;
    [SerializeField] Slider accelerationSlider;
    [SerializeField] Slider suspensionSlider;

    [SerializeField] GameObject trophyScreen;
    [SerializeField] GameObject upgradesScreen;
    [SerializeField] GameObject confirmModal;

    // main trophy
    [SerializeField] GameObject FirstTrophy;
    [SerializeField] GameObject SecondTrophy;
    [SerializeField] GameObject ThirdTrophy;

    // trophies
    [SerializeField] GameObject FirstTrophyPreFab;
    [SerializeField] GameObject SecondTrophyPreFab;
    [SerializeField] GameObject ThirdTrophyPreFab;
    [SerializeField] GameObject youWonFlag;

    [SerializeField] TextMeshProUGUI trophyCaption;
    [SerializeField] TextMeshProUGUI ptsAwardCaption;
    [SerializeField] TextMeshProUGUI upgradesEarnedCaption;
    [SerializeField] TextMeshProUGUI youWonCaption;
    [SerializeField] TextMeshProUGUI raceTipCaption;

    [SerializeField] Button restartButton;
    [SerializeField] Button mainMenuButton;
    [SerializeField] ParticleSystem youWonParticles;

    // audio for upgrades on selection
    [SerializeField] AudioSource topSpeedAudio;
    [SerializeField] AudioSource tiresAudio;
    [SerializeField] AudioSource accelerationAudio;
    [SerializeField] AudioSource suspensionAudio;
    [SerializeField] AudioSource confirmedAudio;
    [SerializeField] AudioSource cancelledAudio;

    [SerializeField] AudioSource gameWonAudio;
    [SerializeField] AudioSource gameWonMusic;
    [SerializeField] AudioSource defaultMusic;
    [SerializeField] AudioSource trophyTwinkle;

    [SerializeField] TextMeshProUGUI topSpeedText;
    [SerializeField] TextMeshProUGUI tiresText;
    [SerializeField] TextMeshProUGUI accelerationText;
    [SerializeField] TextMeshProUGUI suspensionText;

    // text for technical specs on screen
    [SerializeField] TextMeshProUGUI specSpeedText;
    [SerializeField] TextMeshProUGUI specTiresGripText;
    [SerializeField] TextMeshProUGUI specTiresMassText;
    [SerializeField] TextMeshProUGUI specAccelerationText;
    [SerializeField] TextMeshProUGUI specSuspensionLengthText;
    [SerializeField] TextMeshProUGUI specSuspensionStiffnessText;

    // trophy chest
    private List<GameObject> trophiesDisplay= new List<GameObject>();

    // random race tips to display on trophy screen (goes under the trophy)
    private string[] raceTips = new string[9] {"tip - use space bar to hand brake", 
                                               "tip - pick you race line wisely...",
                                               "tip - first place awards 4 points",
                                               "tip - better race, better start position...",
                                               "tip - avoid aggressive opponents!",
                                               "tip - clear upgrades for new combos",
                                               "tip - try sight seeing!",
                                               "tip - try cutting the grass",
                                               "tip - practice makes perfect" };

    // shortcut to gameManager instance
    private GameManager gm;

    void Start()
    {
        // init game manager
        gm = GameManager.Instance;

        // get the amoung of avilable points we have to spend
        upgradesAmtText.text = gm.GetUpgradePoints().ToString();

        // show the amount 
        maxSpeedSlider.value = gm.maxSpeedPtsSpent;
        tiresSlider.value = gm.tiresPtsSpent;
        accelerationSlider.value = gm.accelerationPtsSpent;
        suspensionSlider.value = gm.suspensionPtsSpent;

        // initialize the upgrade captions
        InitUpgradeCaptions();
        // initialize the spec captions
        InitSpecs();

        // hide the confirmation modal
        confirmModal.SetActive(false);
        // hide the upgrade screen
        upgradesScreen.SetActive(false);
        // show the main trophy screen
        trophyScreen.SetActive(true);

        // hide all the "you won" the game objects, game is not completed
        youWonCaption.enabled = false;
        restartButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);

        // hide the game won particles and flag on default
        youWonParticles.Stop();
        youWonFlag.SetActive(false);


        // initialize the trophy screen, takes player rank
        if (gm.trophyChest.Count > 0)
        {
            InitTrophyScreen(gm.trophyChest[gm.trophyChest.Count - 1]);
        } else
        {
            // if list is empty initialize with first place (for testing purposes)
            InitTrophyScreen(1);
        }

        //Debug.Log(gm.finalRace);

        if (gm.finalRace != true)
        {
            // hide the trophy screen after n seconds
            StartCoroutine(TropyCountdownRoutine(4f));
            defaultMusic.Play();
            trophyTwinkle.Play();
        }
        else
        {
            // player "WON" the game graphics
            gameWonAudio.Play();
            gameWonMusic.Play();
            raceTipCaption.enabled = false;
            upgradesEarnedCaption.enabled = false;
            ptsAwardCaption.enabled = false;
            youWonCaption.enabled = true;
            // show the winning flag and end game particles
            youWonFlag.SetActive(true);
            youWonParticles.Play();

            // show buttons to restart the game or exit to main menu
            restartButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
        }
    }

    IEnumerator TropyCountdownRoutine(float cooldown)
    {
        // timeout on trophy award screen
        // the trophy screen shows for the cooldown before landing player on upgrades interface
        yield return new WaitForSeconds(cooldown);
        trophyScreen.SetActive(false);
        upgradesScreen.SetActive(true);
    }
    public void InitTrophyScreen(int playerRank)
    {
        // initializes the trophy screen with the player rank
        // trophies shown here are the main large trophy on screen
        if (playerRank == 1)
        {
            FirstTrophy.SetActive(true);
            SecondTrophy.SetActive(false);
            ThirdTrophy.SetActive(false);
            trophyCaption.text = "1st Place";
            ptsAwardCaption.text = "4";
        }
        else if(playerRank == 2)
        {
            FirstTrophy.SetActive(false);
            SecondTrophy.SetActive(true);
            ThirdTrophy.SetActive(false);
            trophyCaption.text = "2nd Place";
            ptsAwardCaption.text = "2";
        }
        else if(playerRank == 3)
        {
            FirstTrophy.SetActive(false);
            SecondTrophy.SetActive(false);
            ThirdTrophy.SetActive(true);
            trophyCaption.text = "3rd Place";
            ptsAwardCaption.text = "1";
        }

        // don't instantiate anything if the trophy chest list is empty
        if (gm.trophyChest.Count > 0)
        {
            // display the trophies in the chest
            for (int i = 0; i < gm.trophyChest.Count; i++)
            {
                // instantiating objects for the trophy chest
                GameObject prefabToUse = FirstTrophyPreFab;

                // pick which prefab to use based on the trophy list
                if (gm.trophyChest[i] == 1)
                {
                    prefabToUse = FirstTrophyPreFab;
                }
                else if (gm.trophyChest[i] == 2)
                {
                    prefabToUse = SecondTrophyPreFab;
                }
                else if (gm.trophyChest[i] == 3)
                {
                    prefabToUse = ThirdTrophyPreFab;
                }

                // horizontal offset for trophies in chest
                float horizOffset = 90f;
                // instantiate trophy and offset it
                GameObject trophy = Instantiate(prefabToUse, new Vector3(-865f + (i * horizOffset), -495f, -500f), Quaternion.identity);
                // set the scale of the trophy
                trophy.transform.localScale = new Vector3(25, 25, 25);
                // set the parent so we can see it on the canvas
                trophy.transform.SetParent(GameObject.FindGameObjectWithTag("Trophy Screen").transform, false);
                // add the trophy to the list
                trophiesDisplay.Add(trophy);
            }
        }

        // display a random race tip on the trophy screen display
        int randCaption = Random.Range(0, raceTips.Length);
        raceTipCaption.text = raceTips[randCaption];
    }

    public void RefreshPtsText()
    {
        upgradesAmtText.text = gm.GetUpgradePoints().ToString();
    }

    public void SpeedUpgradePress()
    {

        if(gm.maxSpeedPtsSpent < 4 && gm.GetUpgradePoints() > 0)
        {
            gm.SpendMaxSpeedPoint();
            topSpeedText.text = gm.maxSpeedPtsSpent.ToString() + " UPGRADED";
            topSpeedAudio.Play();
            // the car upgrade
            gm.IncreaseMaxSpeed(3f);
        }

        maxSpeedSlider.value = gm.maxSpeedPtsSpent;

        // refresh points available text
        RefreshPtsText();

        // refresh specs text
        InitSpecs();
    }

    public void TireUpgradePress()
    {
        if(gm.tiresPtsSpent < 4 && gm.GetUpgradePoints() > 0)
        {
            gm.SpendTiresPoint();
            tiresText.text = gm.tiresPtsSpent.ToString() + " UPGRADED";
            // the car upgrade
            gm.IncreaseTireGrip(.005f);
            gm.IncreaseTireMass(.05f);
            tiresAudio.Play();
        }

        tiresSlider.value = gm.tiresPtsSpent;

        // refresh points available text
        RefreshPtsText();

        // refresh specs text
        InitSpecs();
    }

    public void AccelerationUpgradePress()
    {
        if (gm.accelerationPtsSpent < 4 && gm.GetUpgradePoints() > 0)
        {
            gm.SpendAccelerationPoint();
            accelerationText.text = gm.accelerationPtsSpent.ToString() + " UPGRADED";
            // the car upgrade
            gm.IncreaseEnginePower(100f);
            accelerationAudio.Play();
        }

        accelerationSlider.value = gm.accelerationPtsSpent;

        // refresh points available text
        RefreshPtsText();

        // refresh specs text
        InitSpecs();
    }

    public void SuspensionUpgradePress()
    {

        if (gm.suspensionPtsSpent < 4 && gm.GetUpgradePoints() > 0)
        {
            gm.SpendSuspensionPoint();
            suspensionText.text = gm.suspensionPtsSpent.ToString() + " UPGRADED";
            // the car upgrade
            gm.IncreaseSuspension(-.02f);
            gm.IncreaseSpringStrength(175f); // was 50f
            suspensionAudio.Play();
        }

        suspensionSlider.value = gm.suspensionPtsSpent;

        // refresh points available text
        RefreshPtsText();

        // refresh specs text
        InitSpecs();
    }

    public void InitUpgradeCaptions()
    {
        // initial text caption values
        topSpeedText.text = gm.maxSpeedPtsSpent + " UPGRADED";
        tiresText.text = gm.tiresPtsSpent + " UPGRADED";
        accelerationText.text = gm.accelerationPtsSpent + " UPGRADED";
        suspensionText.text = gm.suspensionPtsSpent + " UPGRADED";
    }

    public void InitSpecs()
    {
        // settings text caption values
        // multiply speed by 3.6 since the game read out is in km/h (3.6 * speed = km/h)
        specSpeedText.text = "MAX SPEED: " + (gm.playerCarTuningClone.maxSpeed * 3.6);
        specTiresGripText.text = "GRIP: " + gm.playerCarTuningClone.tireGrip;
        specTiresMassText.text = "MASS: " + gm.playerCarTuningClone.tireGrip;
        specAccelerationText.text = "ENGINE POWER: " + gm.playerCarTuningClone.enginePower;
        specSuspensionLengthText.text = "LENGTH: " + gm.playerCarTuningClone.restLength;
        specSuspensionStiffnessText.text = "STRENGTH: " + gm.playerCarTuningClone.springStrength;
    }

    public void ResetPress()
    {
        // cache the amount of points we have spent
        // these are added back after resetting everything
        int previousPtsSpent = gm.PtsSpentToDate();

        // reset the upgrade points, sets each upgrade to zero
        gm.ResetUpgradePoints();

        // reset all the slider values
        maxSpeedSlider.value = gm.maxSpeedPtsSpent;
        tiresSlider.value = gm.tiresPtsSpent;
        accelerationSlider.value = gm.accelerationPtsSpent;
        suspensionSlider.value = gm.suspensionPtsSpent;

        // replenish all the upgrade points current and previously spent on reset    
        gm.ReplenishUpgradePoints(gm.GetUpgradePoints() + previousPtsSpent);

        // play canceled audio
        cancelledAudio.Play();
        // reset the available upgrade caption slots back to original
        InitUpgradeCaptions();
        // refresh points available text after all the resets
        RefreshPtsText();
        //////////////////////////////////////////////////////////////////////////////
        // revert to the original, default tuning state when we first started the game
        // once we spend points we alter it again
        //////////////////////////////////////////////////////////////////////////////
        gm.ClearTuningToDefault();

        // refresh specs text
        InitSpecs();
    }

    public void ConfirmPress()
    {
        // mouse press confirming upgrades (happens before modal)
        confirmedAudio.Play();
        confirmModal.SetActive(true);    
    }

    public void ConfirmCancelPress()
    {
        // canceling out of confirmation modal
        confirmModal.SetActive(false);
    }

    public void ConfirmModalPress()
    {
        // upgrade all the opponents to stay competitive
        gm.UpgradeAllOpponents();
        // player is confirming upgrades in modal
        SceneManager.LoadScene("Level 1");
    }

    public void MainMenuPress()
    {
        // player is exiting to main menu after winning
        gm.ResetGame();
        SceneManager.LoadScene("Home");
    }

    public void RestartGamePress()
    {
        // player is restarting the game after winning
        gm.ResetGame();
        SceneManager.LoadScene("Level 1");
    }
}
