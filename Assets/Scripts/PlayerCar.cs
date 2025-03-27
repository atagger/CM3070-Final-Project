using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering; // for motion blur

public class PlayerCar : Vehicle
{
    // player input manager
    private CustomInput playerInput;

    // input manager is assigned when car is spawned
    public InputManager inputManager;
    
    // glowing checkpoint indicator for player to steer to
    private GameObject checkPoint;

    // global motion blur volume for car on nitros
    public Volume nitroBlurVolume;

    // final lap flag
    private bool isFinalLap = false;

    // final announcnement flag for rank
    private bool isAnnounced = false;

    // checkpoint threshold for make haste announcement
    private int makeHasteThreshold;
    private bool hasteIsAnnounced = false;

    #region Awake / Start / Updates
    private void Awake()
    {
        // set the custom input control scheme on awake
        playerInput = new CustomInput();
    }

    public override void Start()
    {
        // https://discussions.unity.com/t/why-is-start-from-my-base-class-ignored-in-my-sub-class/25421/6
        // call start in the base class
        base.Start();

        // disable motion blur for nitro
        nitroBlurVolume.enabled = false;

        // find and then set the glowing check point indicator
        // we can't use serialize field here because this is an instantiated item
        checkPoint = GameObject.FindWithTag("CheckPoint");
        
        // place the check point marker at current node so player knows where to drive
        SetCheckPointPosition();

        makeHasteThreshold = totalCheckPoints - 20;
    }

    public override void Update()
    {
        base.Update();

        steerInput = inputManager.moveVector.x;

        // normal game controls before race is finish
        if (!isFinished)
        {
            // set the move input and braking appropriately
            if (gameStarted) SetMoveInput();

            // normal player steering
            AckermannSteering(steerInput);
        } 
        else 
        {
            // post finish the car drives on auto-pilot
            Vector3 target = nodes[currentNode].position - carRb.position;
            moveInput = .4f;
            AckermannSteering(ApplySteer(target));
        }


        // set checkpoint indicator position to next
        // if we can finish we freeze the checkpoint on the finish until the player finishes
        if (!carCanFinish) SetCheckPointPosition();
        else FreezeCheckPointPositionFinish();

        FinalLapCheck();

        // check to make "make haste" announcement when player is close to finishing the race
        if(!hasteIsAnnounced) MakeHasteCheck();

        // voice annoucnement of player's rank when they cross the finish
        if (!isAnnounced) FinishAnnounce();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // check if we should apply the blur
        MotionBlurCheck();
    }
    #endregion

    #region Auto-Pilot / Apply Steering
    private float ApplySteer(Vector3 steerTarget)
    {
        Vector3 relativeVector = steerTarget; // - transform.position;
        Vector3 steeringCrossProduct = Vector3.Cross(transform.forward, relativeVector.normalized);
        float steer = steeringCrossProduct.y;
        return steer;
    }
    #endregion

    #region Motion Blur / VFX / Audio

    public void MakeHasteCheck()
    {
        if(currentLap == lapsInRace && checkPointsCollected > makeHasteThreshold) 
        {
            makeHasteSound.Play();
            hasteIsAnnounced = true;
        }
    }
    public void FinalLapCheck()
    {
        // if final lap, trigger final lap warning speech
        if (currentLap == lapsInRace && isFinalLap == false)
        {
            //Debug.Log("final lap");
            finalLapSound.Play();
            isFinalLap = true;
        }
    }

    public void FinishAnnounce()
    {
        if (isFinished)
        {
            //Debug.Log("------------------------------>sending" + rankNum);
            isAnnounced = true;
            AnnounceWin(rankNum);
        }
    }

    private void AnnounceWin(int rank)
    {

        // announce the final place sounds
        switch (rank)
        {
            case 1:
                firstPlaceSound.Play();
                break;
            case 2:
                secondPlaceSound.Play();
                break;
            case 3:
                thirdPlaceSound.Play();
                break;
            case 4:
                fourthPlaceSound.Play();
                break;
            case 5:
                fifthPlaceSound.Play();
                break;
            default:
                youLostSound.Play();
                break;
        }
    }
    private void MotionBlurCheck()
    {
        // if car is over max speed turn on the blur
        if (GetCarSpeed() > tuning.maxSpeed+10f)
        {
            ToggleBlur(true);
        }
        else
        {
            ToggleBlur(false);
        }
    }
    private void ToggleBlur(bool toggle)
    {
        nitroBlurVolume.enabled = toggle;
    }

    #endregion

    #region Set Check Point Position

    // positions the glowing check point markers for player
    // this helps the player know where they need to drive
    private void SetCheckPointPosition()
    {
        // positions a glowing ring showing player where checkpoint is
        // set the node to 0
        int nextNode = 0;

        // if the last node, the next node is 0
        if(currentNode == nodes.Count-1)
        {
            nextNode = 0;
        } 
        else
        {
            nextNode = currentNode + 1;
        }

        // get the direction of the trackpath and set the position, rotation and scale of the checkpoint
        Vector3 direction = nodes[currentNode].position - nodes[nextNode].position;
        checkPoint.transform.rotation = Quaternion.LookRotation(direction);
        checkPoint.transform.position = nodes[currentNode].position;
        checkPoint.transform.localScale = new Vector3(goalCompletedDistance, 
                                                      goalCompletedDistance, 
                                                      goalCompletedDistance);
    }

    private void FreezeCheckPointPositionFinish()
    {
        // positions a glowing ring showing player where checkpoint is
        // set the node to 0
        int nextNode = 0;
        int currentNode = nodes.Count - 1;

        // get the direction of the trackpath and set the position, rotation and scale of the checkpoint
        Vector3 direction = nodes[currentNode].position - nodes[nextNode].position;
        checkPoint.transform.rotation = Quaternion.LookRotation(direction);
        // set the node before finish to 0
        checkPoint.transform.position = nodes[0].position;
        checkPoint.transform.localScale = new Vector3(goalCompletedDistance,
                                                      goalCompletedDistance,
                                                      goalCompletedDistance);
    }
    #endregion

    #region Triggers

    private void OnTriggerEnter(Collider other)
    {
        // cross finish line check -- see method in Vehicle parent class
        if (other.gameObject.name == "Finish Line Trigger")
        {
            CrossFinishCheck();
        }

        if (other.gameObject.name == "Nitro")
        {
            nitroSound.Play();
        }

        if (other.gameObject.name == "Hazard")
        {
            hazardDetectedVoice.Play();
        }

        if (other.gameObject.name == "Death Trigger")
        {
            PlayerDied();
        }

    }

    #endregion

    #region Movement + Brake Input

    private void SetMoveInput()
    {
        // if we are not braking
        if (!inputManager.isBrake)
        {
            // if we are not braking the input is the vertical axis
            // see input manager for details 
            moveInput = inputManager.moveVector.y;
        }

        // normal braking (the down arrow)
        if (inputManager.isBrake)
        {
            // braking, set brake lights to red
            moveInput = -1;
            BrakeLightsActive();
        }
        else
        {
            // we are no longer breaking, set brake lights to default state
            BrakeLightsInactive();
        }

        // if we are using handbrake and not the brake
        if (inputManager.isHandBrake)
        {
            // check if the car is moving forwards
            if (carDirection >= 0)
            {
                // using the hand brake going forward
                moveInput = -1;
                HandBrakeLights();
            }
            else
            {
                // the car is moving backwards
                moveInput = 1;
                HandBrakeLights();
            }
        }
    }

    #endregion

    #region Player Car Checks (Wrong Way, etc.)
    public bool CheckWrongWay()
    {
        // direction from current node to car
        Vector3 targetDir = nodes[currentNode].position - carRb.position;
        // dot product from current node and car, normalized
        float dotResult = Vector3.Dot(carRb.transform.forward.normalized, targetDir.normalized);

        // if less than zero we are not going towards the checkpoint
        if (dotResult < 0f)
        {
            //Debug.Log(dotResult + " going wrong way from node: " + nodes[currentNode]);
            return true;
        } else
        {
            return false;
        }
    }

    #endregion

}
