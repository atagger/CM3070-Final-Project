using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // text mesh pro for fonts    

public class UIController : MonoBehaviour
{
    // to get race results
    [SerializeField] Spawner spawner;

    // reference to the canvas object
    public GameObject UIRacePanel;

    // text fields
    public TextMeshProUGUI UITextCurrentLap;
    public TextMeshProUGUI UITextCurrentLapTime;
    public TextMeshProUGUI UITextLastLapTime;
    public TextMeshProUGUI UITextBestLapTime;
    public TextMeshProUGUI UITextSpeed;
    public TextMeshProUGUI UIWrongWay;
    public TextMeshProUGUI UIPositionInRace;

    public Animation wrongWayAnime;

    private PlayerCar playerCar; // reference to player car

    private int currentLap = -1;
    private float currentLapTime;
    private float lastLapTime;
    private float bestLapTime;

    private bool wrongWayFlag = false;

    [SerializeField] AudioSource wrongWayAudio;

    public void setCarReference(GameObject car)
    {
        // reference to the player car, sent by the spawner script
        playerCar = car.GetComponent<PlayerCar>();

    }
    private void Start()
    {
        // disable wrong way indicator on start
        UIWrongWay.enabled = false;
    }
    private void Update()
    {
        // reference is broken, exit out
        if (playerCar == null) return;

        // get the lapTimer from the car
        LapTimers();

        // print the car speed on screen
        SpeedOut();

        // check the player is travelling the correct direction on track
        CheckWrongWay();

        // show players rank in race
        UIPositionInRace.text = spawner.GetRaceRankingText();
    }

    private void LapTimers()
    {
        // we have a new lap, update UI
        if (playerCar.currentLap != currentLap)
        {
            if (playerCar.currentLap <= 0)
            {
                // for the UI display "Lap 1" if the currentLap is actually 0 or less
                // this happens when we first start
                UITextCurrentLap.text = $"Lap 1 : {GameManager.Instance.GetNumLaps()}";
            }
            else
            {
                currentLap = playerCar.currentLap;
                UITextCurrentLap.text = $"Lap {currentLap} : {GameManager.Instance.GetNumLaps()}";
            }
        }

        // we have a new current lap time, update UI
        if (playerCar.currentLapTime != currentLapTime)
        {
            currentLapTime = playerCar.currentLapTime;
            UITextCurrentLapTime.text = $"Time: {(int)currentLapTime / 60}:{(currentLapTime) % 60:00.000}";
        }

        // we have a new last lap time, update UI
        if (playerCar.lastLapTime != lastLapTime)
        {
            // the two opposing checkpoints to the finish need to be place in close proximity to it
            // otherwise there can be a delay in the actual lap time recording (player records lap time at a far away checkpoint)
            // and the race tracks can be driven in both directions, so the first and last point need to be close
            lastLapTime = playerCar.lastLapTime;
            UITextLastLapTime.text = $"Last: {(int)lastLapTime / 60}:{(lastLapTime) % 60:00.000}";
        }

        // we have a new best lap time, update UI
        if (playerCar.bestLapTime != bestLapTime)
        {
            bestLapTime = playerCar.bestLapTime;
            // set a high number for the best lap time, this will trigger when the player first crosses the finish line
            // on first lap since it will be less, otherwise show the text as "Best: None"
            UITextBestLapTime.text = bestLapTime < 1000000 ? $"Best: {(int)bestLapTime / 60}:{(bestLapTime) % 60:00.000}" : "Best: NONE";

            // play the best lap announcement
            // do not play it on final lap or the last lap because there are announcements for final lap and the results
            // this is mainly the best for longer races, but provides a nice indicator of player performance
            if (playerCar.currentLap > 0 && playerCar.currentLap < playerCar.GetLapsInRace())
            {
                playerCar.PlayBestLapAnnouncement();
            }
        }

    }

    private void CheckWrongWay()
    {
        // provide an indicator if player is going wrong direction
        // do not check or show animation if the race is finished
        if(playerCar.CheckWrongWay() == true && !playerCar.isFinished)
        {
            UIWrongWay.enabled = true;

            // play wrong way audio alert one time
            // do not play this if the car can finish the race
            if (!wrongWayFlag && playerCar.isGrounded)
            {
                wrongWayAudio.Play();
                wrongWayFlag = true;
            }

        }
        else
        {
            wrongWayFlag = false;
            UIWrongWay.enabled = false;
            wrongWayAnime.Stop();
        }
    }

    private void SpeedOut()
    {
        // get speed  in 2 decimal places
        //float speedOut = Mathf.Round(playerCar.GetCarSpeed() * 10) / 10;

        float speedOut = playerCar.GetCarSpeed();

        // provide a buffer so the speed does not fluctuate because we clamp maxSpeed
        float buffer = 0.5f;

        // keep a buffer so speed read out doesn't fluctuate so much at top speed
        // the buffer also allows the nitro to change the speed in the case we are over maxSpeed
        if (speedOut > playerCar.tuning.maxSpeed - buffer && speedOut < playerCar.tuning.maxSpeed + buffer)
        {
            speedOut = playerCar.tuning.maxSpeed;
        }

        // output text to canvas, multiply by 3.6 to get km/h, convert to int
        UITextSpeed.text = $"{(int)(speedOut * 3.6f)} \nkm/h";
    }

}
