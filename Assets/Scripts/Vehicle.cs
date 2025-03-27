using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Vehicle: MonoBehaviour {

    // RIGID BODY + CENTER OF MASS
    protected Rigidbody carRb;
    protected Transform centerMass;

    // PLAYER INPUT
    protected float moveInput;
    protected float steerInput;

    // CAR BODIES
    protected GameObject[] carBodies;
    // the car body index we want to use
    public int carBodyIndex;

    // WHEELS
    protected GameObject FL;
    protected GameObject FR;
    protected GameObject RL;
    protected GameObject RR;
    protected GameObject[] wheels;
    protected GameObject[] wheelsChild;
    protected bool[] isMotor;
    protected TrailRenderer[] skidMarks;
    protected ParticleSystem[] tireSmoke;
    protected GameObject[] brakeLights;
    protected Material brakeLightMatRed;
    protected Material brakeLightMatYellow;
    protected Material brakeLightMatDefault;
    
    // SOUNDS
    protected AudioSource engineSound;
    protected AudioSource tiresSound;
    protected AudioSource impactSound;

    protected AudioSource nitroSound;
    protected AudioSource finalLapSound;

    protected AudioSource firstPlaceSound;
    protected AudioSource secondPlaceSound;
    protected AudioSource thirdPlaceSound;
    protected AudioSource fourthPlaceSound;
    protected AudioSource fifthPlaceSound;
    protected AudioSource youLostSound;

    protected AudioSource makeHasteSound;
    protected AudioSource bestLapSound;
    protected AudioSource hazardDetectedVoice;

    // FOR ACKERMANN STEERING
    protected float wheelBase;
    protected float rearTrack;
    protected float ackAngleLeft;
    protected float ackAngleRight;
    protected float flAngle;
    protected float frAngle;

    // WHEEL RAY CASTS
    private RaycastHit hit;
    private Vector3[] hits = new Vector3[4];
    private Vector3 rayCastDir;
    private float rayCastLength;
    public LayerMask layerMask;

    // CAR STATUS CHECKS
    protected int[] wheelsGrounded = new int[4];
    public bool isGrounded = true;
    public bool rayDidHit { get; private set; }
    private bool isCarFlipped;
    protected float carDirection;
    protected float carSpeed;

    // TUNING
    // tuning is set via scriptable object and assigned during instiation via spawner
    public CarSettings tuning;

    // TRACK PATH
    // NOTE: the track paths are stored in the race manager script
    // all the nodes in the track path
    public List<Transform> nodes;
    // the current checkpoint or node we are travelling towards
    public int currentNode { get; private set; } = 0;

    // used to cache the distance of the player to current node so we don't double check
    // if the previous node is different than the current we know we are checking correctly
    protected int previousNode = 0;

    // BRAKE LIGHT TRAILS
    protected TrailRenderer[] brakeLightTrails;

    // LAP TIMES + RACE LOGIC
    public float bestLapTime { get; private set; } = Mathf.Infinity;
    public float lastLapTime { get; private set; } = 0f;
    public float totalLapTime { get; private set; } = 0f;

    public float currentLapTime { get; private set; } = 0f;
    public int currentLap { get; private set; } = 0;

    // number of laps in race
    protected int lapsInRace;

    // lap timer time stamp
    private float lapTimerTimeStamp = 0f;
    // checkpoint distance threshold for crossing 
    // NOTE: this also changes the size of the check point hoop player drives through
    protected float goalCompletedDistance = 40f; // 25-tight 30-more forgiving
    // amount of checkPoints the car has passed (for determining the race distance)
    public int checkPointsCollected = 0;
    // amount of checkPoints the car has passed (for determining the race distance)
    public int totalCheckPoints = 0;

    // race manager is set via Spawner script
    public RaceManager raceManager;

    // race states
    protected bool gameStarted = false;
    public bool isFinished = false;
    protected bool carCanFinish  = false;
    protected bool carCrossedFinish = false;

    // rank number of player
    public int rankNum;

    // engine sound, min and max pitch
    private float minEnginePitch = 2.5f;
    private float maxEnginePitch = 6f;

    // used for rubber banding the competition
    // we modify the enginepower to slow or speed up opponents
    public float engineModifier = 1f;
    // car is boosted
    public bool isBoosted = false;
    // deceleration factor of car for on track / off track
    // this slows the car down if going off road
    private float decelFactor;

    #region Rubber Banding

    // RUBBER BANDING
    // function to upgrade or downgrade engine performance
    // this allows us to maintain a sense of competition by either slowing down
    // or speeding up vehicles that a set number of checkpoints ahead
    public IEnumerator UpOrDownGradeEngine(float cooldown, float modifier)
    {
        //Debug.Log("started a boost, modifier is: " + modifier);
        isBoosted = true;
        engineModifier = modifier;
        yield return new WaitForSeconds(cooldown);
        engineModifier = 1f; // return engine to normal level
        isBoosted = false; // return to non boost state
        //Debug.Log("boost routine finished");
    }

    #endregion

    #region Start
    public virtual void Start()
    {
        // get the rigid body component
        carRb = GetComponent<Rigidbody>();

        // path to get all the references from the prefab (these are assigned in the inspector)
        PrefabScript scriptPath = GetComponentInParent<PrefabScript>();

        // path to brakeLights (we have to change the position of the truck's)
        // and because of that we have to put it before we set the car body
        brakeLights = scriptPath.brakeLights;

        // get the path to the car bodies
        carBodies = scriptPath.carBodies;
        // set the car body to the index set by the spawner
        SetCarBody(carBodyIndex);
        // set the center mass of the rigid body to the empty object's position
        SetCenterOfMass(carBodyIndex);

        // wheel front gameObjects (used for steering)
        FL = scriptPath.FL;
        FR = scriptPath.FR;
        // wheel rear gameObjects (used for motor power, but front can also be used...)
        RL = scriptPath.RL;
        RR = scriptPath.RR;

        // array of all the wheels (set inside the car prefab)
        wheels = scriptPath.wheels;
        // array of all the children meshes inside wheel (this is the actual wheel you see on screen)
        wheelsChild = scriptPath.wheelsChild;
        // array of which wheels are motors (set inside prefab)
        isMotor = scriptPath.isMotor;
        skidMarks = scriptPath.skidMarks;
        tireSmoke = scriptPath.tireSmoke;

        brakeLightTrails = scriptPath.brakeLightTrails;
        brakeLightMatRed= scriptPath.brakeLightMatRed;
        brakeLightMatYellow = scriptPath.brakeLightMatYellow;
        brakeLightMatDefault = scriptPath.brakeLightMatDefault;

        // SOUNDS
        //-----------------------------------
        engineSound = scriptPath.engineSound;
        tiresSound = scriptPath.tiresSound;
        impactSound = scriptPath.impactSound;

        nitroSound = scriptPath.nitroSound;
        finalLapSound = scriptPath.finalLapSound;

        firstPlaceSound = scriptPath.firstPlaceSound;
        secondPlaceSound = scriptPath.secondPlaceSound;
        thirdPlaceSound = scriptPath.thirdPlaceSound;
        fourthPlaceSound = scriptPath.fourthPlaceSound;
        fifthPlaceSound = scriptPath.fifthPlaceSound;
        youLostSound = scriptPath.youLostSound;

        makeHasteSound = scriptPath.makeHasteSound;
        bestLapSound = scriptPath.bestLapSound;
        hazardDetectedVoice = scriptPath.hazardDetectedVoice;

        //-----------------------------------
        // play engine sound on start
        engineSound.Play();
        //-----------------------------------

        // set the raycast's length for suspension
        rayCastLength = tuning.restLength + tuning.wheelRadius;

        // set the wheel size from tuning
        SetWheelSize();

        // set wheel base and rear track if using ackermann steering
        AckermannSteeringSetup();

        // set brakes inactive (puts material in default state)
        BrakeLightsInactive();

        // subscribe to the raceManager listener for startGame
        // when it fires called BeginGameForPlayers()
        raceManager.startGame.AddListener(BeginGameForPlayers);
        lapsInRace = GameManager.Instance.GetNumLaps();

        // set the total number of checkpoints -- used to calculate rankings if player finishes before opponents
        // subtract one because the finish line is a collider and not a check point
        totalCheckPoints = (nodes.Count * lapsInRace) - 1;

        decelFactor = tuning.decelFactor;
        //Debug.Log(lapsInRace + " laps in race");
        //Debug.Log(totalCheckPoints + " total checkPoints");      

    }
    #endregion

    #region Set Car Body / Set Center Of Mas of Car
    // set the car body for the game play
    // this is set by the spawner
    public void SetCarBody(int carBodyIndex)
    {
        carBodies[carBodyIndex].SetActive(true);

        if (carBodies[carBodyIndex].name == "Truck")
        {
            // translate the brake lights over for the truck
            brakeLights[0].transform.Translate(-.27f, 0, 0);
            brakeLights[1].transform.Translate(.27f, 0, 0);
        }
    }

    public void SetCenterOfMass(int carBodyIndex)
    {
        // sets the center of mass of vehicle using transform of the empty game object in car
        centerMass = this.transform.Find("Center Mass");

        // we can change the center of mass to accomodate for the different heights of vehicles
        // this also changes the handling of the cars on the track and how they behave
        // altering these numbers allows for different driving effects with the opponents

        if (carBodies[carBodyIndex].name == "Hot Rod")
        {
            // player car (hot rod) - center of mass is default of.1f
           carRb.centerOfMass = centerMass.localPosition;
        }
        else if (carBodies[carBodyIndex].name == "Station Wagon")
        {
            // station wagon
            carRb.centerOfMass = new Vector3(centerMass.localPosition.x, centerMass.localPosition.y + .01f, centerMass.localPosition.z);
        }
        else if (carBodies[carBodyIndex].name == "Van")
        {
            // nissan van
            carRb.centerOfMass = new Vector3(centerMass.localPosition.x, centerMass.localPosition.y + .015f, centerMass.localPosition.z);
        }
        else if (carBodies[carBodyIndex].name == "Truck")
        {
            // truck
           carRb.centerOfMass = new Vector3(centerMass.localPosition.x, centerMass.localPosition.y + .005f, centerMass.localPosition.z);
        }
    }
    #endregion

    #region Begin Game Event
    private void BeginGameForPlayers()
    {
        // RACE START
        // initial race trigger condition
        //Debug.Log("The game has started for: " + this.name);

        // set game state flag
        gameStarted = true;

        // start the race
        StartLap();

        // set the start time
        lapTimerTimeStamp = Time.time;

        // unsubscribe from listener
        raceManager.startGame.RemoveListener(BeginGameForPlayers);
    }
    #endregion

    #region Updates
    public virtual void Update()
    {
        //Debug.Log("normalized speed: " + GetNormalizedSpeed() + " speed: " + GetCarSpeed());

        // check car is on the ground, cache variable
        isGrounded = GroundCheck();

        // rotate wheels on movement
        for (int i = 0; i < wheelsChild.Length; i++)
        {
            RotateWheel(carDirection, wheelsChild[i]);
            Vfx(skidMarks[i]);
        }

        // check how far we are from the current checkpoint, this sets what node we are on
        CheckWayPointDistance();

        // do not check for finish or set lap times after car is done
        if (!isFinished)
        {
            SetCurrentLapTime();
            CheckCarCanFinish();
        }
    }

    public virtual void FixedUpdate()
    {

        // car sound
        EngineSound();

        // if  car is in the air apply extra force so the car doesn't fly into outerspace
        // takes the amount of grouding force as a param - between 50-70f seems good
        InAirDownForce(60f);

        // check if the car is flipped
        CheckIfFlipped();
        // check if the car is off road, and if so penalize it
        CheckOffRoad();

        // cache + get the carDirection (are we going forward or backwards)
        carDirection = GetCarDirection();
        carSpeed = GetCarSpeed();

        // cast ray down to ground
        rayCastDir = -transform.up;

        for (int i = 0; i < wheels.Length; i++)
        {
            // debug to draw rays
            //Debug.DrawRay(wheels[i].transform.position, rayCastDir * rayCastLength, Color.green);

            rayDidHit = Physics.Raycast(wheels[i].transform.position, rayCastDir, out hit, rayCastLength);
            hits[i] = Vector3.zero;

            if (rayDidHit)
            {
                // wheel is grounded
                wheelsGrounded[i] = 1;

                // for gizmos
                hits[i] = hit.point;

                // point at which to apply force
                Vector3 forcePoint = hit.point + (tuning.wheelRadius * transform.up);

                Suspension(forcePoint, wheels[i]);
                SidewaysDragForce(forcePoint, wheels[i]);

                // only apply power if wheel is a motor
                if (isMotor[i]) Acceleration(forcePoint, wheels[i]);

                // decelerate the car with a force so we slow down if not applying throttle
                Deceleration(forcePoint, wheels[i]);

                // set the visual position of the child wheel (the wheel child is a gameobject nested in the wheel)
                SetWheelPos(wheelsChild[i], forcePoint); // set wheel position

                // debug to draw ray showing a hit and where it is
                //Debug.DrawLine(wheels[i].transform.position, hit.point, Color.red);
            }
            else
            {
                // wheel not grounded
                wheelsGrounded[i] = 0;
                // set the visual position of the child wheel (the wheel child is a gameobject nested in the wheel)
                SetWheelPos(wheelsChild[i], wheels[i].transform.position + (-transform.up * tuning.restLength));
            }
        }
    }
    #endregion

    #region Lap Timer / Counter / Race Logic

    public float GetSquareDistance()
    {
        // gives distance to current node squared (this is needed to determine the race rankings)
        // the squared number provides a much larger number range of distance
        // this is used in lieu of Vector3.distance where small decimals values can flicker the
        // race position output...
        Vector3 dir = transform.position - nodes[currentNode].position;
        return dir.sqrMagnitude;
    }

    private float GetDistanceToCurrentCheckPoint(int currentNode)
    {
        // save as an accessible var so we can rank the car's position
        float distanceToCurrentCheckPoint = Vector3.Distance(transform.position, nodes[currentNode].position);
        return distanceToCurrentCheckPoint;
    }
    private void CheckWayPointDistance()
    {
        // check the distance of the car to the current node to see if we completed the goal
        if (GetDistanceToCurrentCheckPoint(currentNode) < goalCompletedDistance)
        {

            // reached the last node
            if (currentNode == nodes.Count - 1)
            {
                // NORMAL DRIVING RACE STATE
                // -------------------------------------------------------------------------------------------
                // check if the car is not finished and can't finish (not on the next to final checkpoint)
                //  because  we do not want to start new laps for opponents which will mess up the rankings
                // -------------------------------------------------------------------------------------------
                if (!carCanFinish && !isFinished)
                {
                    // car is not finished
                    EndLap(); // end the lap
                    StartLap(); // start a new lap
                } 
                else
                {
                    // # # # # # # # # # # # # ################################################################
                    // CAR CAN FINISH RACE STATE
                    // # # # # # # # # # # # # ################################################################
                    // carCanFinish = true if it's the last lap and at the checkpoint before the finish line.
                    // when this state happens we use the Finish Line trigger box to end the race, 
                    // instead of using the normal distance based detection for checkpoints.
                    // we want the finish line boundary to be exactly where the finish line graphic is
                    // ----------------------------------------------------------------------------------------
                    // see playerCar SetCheckPoint() in Update() for what happens in this condition
                    // we set the currentNode to the first (0) so we can position or "freeze" the finish line
                    // until the trigger is hit by the car, at which point isFinished = true
                    // #########################################################################################

                    currentNode = 0;
                }

            }
            else
            {
                // not at the end, increment to next node
                currentNode++;
            }

            // save the number of checkpoints collected so we can rank the car's position in the race
            // because we use distance to detect if a checkpoint is reached it is possible
            // to double count, so we check the currentNode is different than the previous
            // and stop counting if the race is finished
            if (previousNode != currentNode && !isFinished) checkPointsCollected++;

        }

        // cache previous for next update
        previousNode = currentNode;

    }

    public bool GetCarCanFinish()
    {
        // this is used by the UI HUD for the "wrong way" audio trigger
        // if the car is in the finishing state (checkpoint before finish line)
        // it will trigger a wrong way alert before crossing the finish line unless we turn this off temporarily
        return carCanFinish;
    }

    private void SetCurrentLapTime()
    {
        currentLapTime = lapTimerTimeStamp > 0 ? Time.time - lapTimerTimeStamp : 0;
    }

    // starts a new lap, resets timer and increments lap count
    private void StartLap()
    {
        // if the vehicle finished a race or completed all its laps don't let it start a new one
        if(currentLap < lapsInRace) currentLap++;
        lapTimerTimeStamp = Time.time;
        //Debug.Log("Start Lap! Current lap is: " + currentLap);
    }

    // checks if the car is at the checkpoint before finish
    // if so the finish line collider starts listening for a crossing
    private void CheckCarCanFinish()
    {
        // check if the car is elgibile to finish the race
        // this happens on the next to last checkpoint before the car drives
        // through the finish line collider
        // are we on the last lap and not yet finished?
        if (currentLap == lapsInRace && !isFinished)
        {
            // we are at the node before the finish line
            if (currentNode == nodes.Count - 2)
            {
                // car can finish race if true, see CrossFinishCheck()
                carCanFinish = true;
                //Debug.Log("Car can finish race");
            }
        }
    }

    // logic to check when the car crosses the finish line collider
    protected void CrossFinishCheck()
    {
        // this is triggered by the sub classes if they drive through the finish line collider
        // we use a collider for the finish line for more accuracy, instead of a distance
        // check the car can finish and that the race is not done already
        if (carCanFinish && !isFinished)
        {
            carCanFinish = false;
            // set flag that race is complete
            isFinished = true;
            EndLap();
            //Debug.Log(this.name + " crossed the finish line!");
            //FinishedRace();
        }
    }

    // ends a lap, records the timing metrics and sets the current node to 0
    // the current node is uses by the opponent cars for pathfinding
    private void EndLap()
    {
        lastLapTime = Time.time - lapTimerTimeStamp;
        bestLapTime = Mathf.Min(lastLapTime, bestLapTime);
        totalLapTime += lastLapTime;

        // Debug.Log(currentLap + " current lap");
        // Debug.Log(lapsInRace + " LAPS IN RACE");
        currentNode = 0;

        //Debug.Log("End Lap - Lap Time was " + lastLapTime + " seconds");
        //Debug.Log("End Lap - Best Time was " + bestLapTime + " seconds");
        //Debug.Log("End Lap - Total Lap Time is " + totalLapTime + " seconds");
    }

    // send the laps in race to the UI Manager so we can play the best lap announcment
    public int GetLapsInRace()
    {
        return lapsInRace;
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(carRb.transform.position, .4f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(carRb.transform.TransformPoint(carRb.centerOfMass), .75f);

        // draw speheres at position of parent wheel
        for (int i = 0; i < wheels.Length; i++)
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wheels[i].transform.position, .15f);
        }

        // draw speheres at position of children wheel
        for (int i = 0; i < wheelsChild.Length; i++)
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wheelsChild[i].transform.position, .15f);
        }

        // draw speheres at position of children wheel
        for (int i = 0; i < hits.Length; i++)
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hits[i], .05f);
        }

    }
    #endregion

    #region Collisions
    private void OnCollisionEnter(Collision collision)
    {
        // play impact sound effect if cars hit objects or each other
        if (!impactSound.isPlaying) impactSound.Play();

        if (collision.gameObject.name == "Opponent Car" || collision.gameObject.name == "Player Car")
        {
            // get the impact foce
            float impactForce = collision.relativeVelocity.magnitude;

            // define a min and max impact
            float minImpact = 20f;
            float maxImpact = 80f; // was 120

            if (impactForce > minImpact)
            {
                // clamp the force between min and max impact
                impactForce = Mathf.Clamp(impactForce, minImpact, maxImpact);


                // Apply a slowdown effect based on impact force
                //Debug.Log("-----------------------------" + (1 - (impactForce / 120f)));

                // scale the velocity  by a number ideally from ~.2 - .8
                //carRb.velocity *= (impactForce/maxImpact);
                // increase angular drag so car does not spin
                carRb.angularDrag = .96f;

            }
            //Debug.Log("crashed into opponent");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Opponent Car" || collision.gameObject.name == "Player Car")
        {
            // revert the angular velocity on collision exit
            carRb.angularDrag = 0.05f;
            //Debug.Log("exiting opponent crash");
        }
    }

    #endregion

    #region Car Forces + Suspension + Movement + Deceleration

    // if the car is off the ground, add an additional force to ground it
    private void InAirDownForce(float forceToApply)
    {
        if (!isGrounded)
        {
            carRb.AddForce(new Vector3(0, -forceToApply, 0), ForceMode.Impulse);
        }
    }
    private void Suspension(Vector3 forcePoint, GameObject wheels)
    {
        // direction force is applied
        Vector3 suspensionDir = wheels.transform.up;
        Vector3 tireWorldVel = carRb.GetPointVelocity(forcePoint);
        
        // when the car is off the ground we only use restLength to set child wheel position
        float offset = (tuning.restLength + tuning.springTravel) - hit.distance;
        float vel = Vector3.Dot(suspensionDir, tireWorldVel);
        //Debug.Log(vel + "springs");

        float forceToApply = (offset * tuning.springStrength) - (vel * tuning.damperStrength);
        Vector3 suspensionForce = forceToApply * suspensionDir;
        carRb.AddForceAtPosition(suspensionForce, forcePoint);
    }

    private void SidewaysDragForce(Vector3 forcePoint, GameObject wheels)
    {
        // direction force is applied
        Vector3 forceDirection = wheels.transform.right;
        Vector3 tireWorldVel = carRb.GetPointVelocity(forcePoint);
        float steeringVel = Vector3.Dot(forceDirection, tireWorldVel);

        float availableTraction = tuning.tractionCurve.Evaluate(GetNormalizedSpeed());
        float dragCoefficient = tuning.tireGrip * availableTraction;

        float desiredVelChange = -steeringVel * dragCoefficient;
        float forceToApply = desiredVelChange / Time.fixedDeltaTime;
        float tireMass = carRb.mass * tuning.tireMass;
        Vector3 steeringForce = forceToApply * forceDirection * tireMass;

        carRb.AddForceAtPosition(steeringForce, forcePoint);
        //DebugLine(forcePoint, (steeringForce) / 2f, Color.red);
    }

    private void Acceleration(Vector3 forcePoint, GameObject wheels)
    {
        Vector3 forceDirection = wheels.transform.forward;

        float availableTorque = 0f;

        // use a different curve for acceleration depending on if we are going forward or back
        // if backward we want to accelerate more quickly to get back to driving
        if (carDirection >= 0) availableTorque = tuning.powerCurve.Evaluate(GetNormalizedSpeed());
        else availableTorque = tuning.reversePowerCurve.Evaluate(GetNormalizedSpeed());

        // velocity at wheel
        //Vector3 tireWorldVel = (carRb.GetPointVelocity(forcePoint));

        // this is what drives the car (player input)
        float acceleration = (moveInput * availableTorque) * tuning.enginePower;

        // limit speed going forwards
        // engine modifier boosts maxSpeed for rubber banding
        if (carSpeed <= tuning.maxSpeed * engineModifier && carDirection >= 0) 
            carRb.AddForceAtPosition(acceleration * forceDirection, forcePoint);
       

        // limit speed going backwards (this is when we are reversing)
        if (carSpeed <= tuning.maxReverseSpeed && carDirection < 0) 
            carRb.AddForceAtPosition(acceleration * forceDirection, forcePoint);
       
    }

    private void Deceleration(Vector3 forcePoint, GameObject wheels)
    {
        Vector3 forceDirection = wheels.transform.forward;
        Vector3 tireWorldVel = (carRb.GetPointVelocity(forcePoint));

        float decelScalar = Vector3.Dot(tireWorldVel, forceDirection) * ((carRb.mass / decelFactor));

        // NOTE: we apply foce in the opposite direction of motion, but the dot product is taken from the forward direction
        Vector3 decelForce = decelScalar * -forceDirection;

        carRb.AddForceAtPosition(decelForce, forcePoint);
    }

    #endregion

    #region Nitro Boosts
    public void NitroBoost()
    {
        carRb.AddForce(40f * transform.forward, ForceMode.VelocityChange);
    }

    #endregion

    #region Wheels
    private void SetWheelSize()
    {
        for (int i = 0; i < wheelsChild.Length; i++)
        {
            wheelsChild[i].transform.localScale = new Vector3(tuning.wheelRadius * 2f, wheelsChild[i].transform.localScale.y, tuning.wheelRadius * 2f);
        }
    }
    private void SetWheelPos(GameObject wheelsChild, Vector3 newPosition)
    {
        // set the position of the wheel
        wheelsChild.transform.position = newPosition;
    }

    private void RotateWheel(float carSpeed, GameObject childWheel)
    {
        // set direction of wheel rotation
        int wheelRotationDirection = 0;

        if (carSpeed > 0) wheelRotationDirection = -1;
        else wheelRotationDirection = 1;

        childWheel.transform.Rotate(0f, wheelRotationDirection * (carRb.velocity.magnitude * Time.deltaTime * tuning.wheelSpinStrength), 0f);
    }
    #endregion

    #region Car Status: Car Speed / Direction / GroundCheck / Flip Checks / Driving Off Road

    public void PlayerDied()
    {
        // vehicle fell off map and was terminated routine

        // get previous node to place the car
        int previousNode = 0;
        
        // place the car at the last check point it visited
        if(currentNode == 0)
        {
            // previous node is last in list
            previousNode = nodes.Count - 1;
        } else
        {
            previousNode = currentNode - 1;
        }

        // reset all velocities for teleportation
        carRb.velocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;

        // we must pause the physics somehow to reset the car rotation and position on teleport
        carRb.isKinematic = true; // pause physics

        transform.position = new Vector3(nodes[previousNode].transform.position.x, 5f, nodes[previousNode].transform.position.z);

        // position car rotation to current check point
        Vector3 target = new Vector3(nodes[currentNode].transform.position.x, 0f, nodes[currentNode].transform.position.z);
        transform.LookAt(target);
        
        carRb.isKinematic = false; // resume physics
    }

    public float GetCarDirection()
    {
        // the dot product lets us rotate the wheel either forward or backward based on the car's velocity
        float carDirection = Vector3.Dot(carRb.velocity.normalized, carRb.transform.forward);
        return carDirection;
    }

    public float GetCarAngle()
    {
        // the dot product lets us rotate the wheel either forward or backward based on the car's velocity
        float carAngle = Vector3.Angle(carRb.velocity.normalized, carRb.transform.forward);
        return carAngle;
    }

    public float GetCarSpeed()
    {
        // provides the raw speed of the car
        float carSpeed = carRb.velocity.magnitude;
        return carSpeed;
    }

    public float GetNormalizedSpeed()
    {
        // get the normalized value of car speed from 0-1 in the forward direction
        //float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(GetCarSpeed()) / tuning.maxSpeed);
        float normalizedSpeed = Mathf.Clamp01(carRb.velocity.magnitude / tuning.maxSpeed);
        return normalizedSpeed;
    }

    public bool GroundCheck()
    {
        // counter for wheels
        int numWheelsGrounded = 0;

        // check each wheel in the array if it's grounded or not
        for (int i = 0; i < wheelsGrounded.Length; i++)
        {
            numWheelsGrounded += wheelsGrounded[i];
        }

        // 4 wheels are on ground, otherwise not
        if (numWheelsGrounded >= 3)
        {
            return true;
        } 
        else 
        {
            return false;
        }

    }

    public void CheckOffRoad()
    {
        // ray cast to check if the car is driving off road
        // if so we add extra deceleration
        bool rayDidHitMaterial = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 1.1f);

        if(rayDidHitMaterial)
        {
            if (hit.collider.name == "Terrain")
            {
                //if(this.name == "Player Car") Debug.Log("off road");

                // penalize driving off the track with friction
                // a lower number makes the car slow down more
                decelFactor = tuning.decelFactorOffRoad; // was 15
            }
            else if(hit.collider.name == "Track Surface")
            {
                //if (this.name == "Player Car") Debug.Log("on track");

                // if on the track or any other surface, revert to default
                decelFactor = tuning.decelFactor;
            }
        }
    }

    public void CheckIfFlipped()
    {
        if (isCarFlipped) return; // skip if flipped

        Vector3 carUp = transform.up;
        
        // if the car is upside down the dot will be -1
        // if the car is on it's side the dot will somewhere around 0
        float flipDot = Vector3.Dot(carUp.normalized, Vector3.up);

        float flipThresholdMin = 0f; // sideways threshold
        float flipThresholdMax = -0.85f; // upside down threshold
        
        if (flipDot <= flipThresholdMax)
        {
            // cast a ray from the car roof to check if we are upside down
            rayDidHit = Physics.Raycast(transform.position, transform.up, out RaycastHit hit1, 1.0f);
            if (rayDidHit) StartCoroutine(CarFlipTimeOutRoutine(0.25f));

            //Debug.Log(this.name + " is flipped upside down with a dot of: " + flipDot);
        } 
        else if(flipDot < flipThresholdMin && flipDot > flipThresholdMax)
        {
            // NOTE: have to shoot longer rays to account for the car body width
            bool rayDidHitLeft = Physics.Raycast(transform.position, -transform.right, out RaycastHit hit2, 1.25f);
            bool rayDidHitRight = Physics.Raycast(transform.position, transform.right, out RaycastHit hit3, 1.25f);
            if (rayDidHitLeft || rayDidHitRight) StartCoroutine(CarFlipTimeOutRoutine(0.25f));

            //Debug.Log(this.name + " is flipped sideways with a dot of: " + flipDot);
        }
    }

    IEnumerator CarFlipTimeOutRoutine(float cooldown)
    {
        // turns car right side up if it flipped
        isCarFlipped = true;

        // store the direction the car was facing
        float originalRotY = transform.eulerAngles.y;

        yield return new WaitForSeconds(cooldown);
        //Debug.Log("executing a flip over correction");

        // put car in air
        carRb.transform.position = new Vector3(carRb.transform.position.x, carRb.transform.position.y + 2f, carRb.transform.position.z);

        // reset rotation, but keep car facing it's original direction
        carRb.rotation = Quaternion.Euler(0f, originalRotY, 0f);
        
        // reset flag so coroutine can be started if car flips again
        isCarFlipped = false;
    }

    #endregion

    #region Car Steering
    public void StandardSteering(float steerInput)
    {
        // standard steering (both wheels rotate by the same amount, unlike Ackermann steering)
        float steeringRotation = steerInput * tuning.stdSteerAngle;

        float angle = Mathf.Clamp(FL.transform.rotation.y + steeringRotation, tuning.stdSteerAngle, tuning.stdSteerAngle);
        float newRotation = angle * Time.deltaTime; // multiply by deltaTime to lerp in next step
        float lerpedAngle = Mathf.Lerp(steeringRotation, newRotation, tuning.lerpSteerTime * Time.deltaTime);

        FL.transform.localEulerAngles = new Vector3(FL.transform.localEulerAngles.x,
                                                    lerpedAngle,
                                                    FL.transform.localEulerAngles.z);

        FR.transform.localEulerAngles = new Vector3(FR.transform.localEulerAngles.x,
                                                    lerpedAngle,
                                                    FR.transform.localEulerAngles.z);
    }

    public void AckermannSteeringSetup()
    {
        // rearTrack is the left and right distance between the two rear wheels centers
        rearTrack = Mathf.Abs(RL.transform.position.x - RR.transform.position.x);

        // wheelBase is the front to back distance between the front and rear wheel centers
        wheelBase = Mathf.Abs(FL.transform.position.z - RL.transform.position.z);
    }
    public void AckermannSteering(float steerInput)
    {
        // make the steering less drastic at higher speeds using a curve
        float turnRadius = tuning.steeringCurve.Evaluate(GetNormalizedSpeed()) * tuning.ackTurnRadius;

        // ackermann steering geometry (inside wheel turns more on turns)
        if (steerInput > 0)
        {
            // turning RIGHT
            ackAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;

        }
        else if (steerInput < 0)
        {
            // turning LEFT
            ackAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else
        {
            // WHEELS NOT TURNED - straight ahead
            ackAngleLeft = 0;
            ackAngleRight = 0;
        }

        flAngle = Mathf.Lerp(flAngle, ackAngleLeft, tuning.lerpSteerTime * Time.deltaTime);
        FL.transform.localRotation = Quaternion.Euler(Vector3.up * flAngle);

        frAngle = Mathf.Lerp(frAngle, ackAngleRight, tuning.lerpSteerTime * Time.deltaTime);
        FR.transform.localRotation = Quaternion.Euler(Vector3.up * frAngle);
    }
    #endregion

    #region Vfx

    private void Vfx(TrailRenderer skidMark)
    {
        // don't do vfx in the air
        if (!isGrounded) return;

        //https://discussions.unity.com/t/how-to-detect-my-car-is-drifting-or-not/167171
        // get the angle of the car compared to its direction dot product
        float slipAngle = Mathf.Acos(carDirection) * Mathf.Rad2Deg;
        float skidAngle = 15f; // lower value to trigger skids more easily (12-20 is good)
      
        // only do this if the wheel has a trail renderer and particle system
        if (skidMark != null)
        {
            // check if wheel is grounded, the car is going forward, and passed the skid angle
            if (rayDidHit && slipAngle > skidAngle && carDirection > 0f)
            {
                // set drift flag for player car to play skid sound (we don't want opponents to do this)
                if (this.name=="Player Car" && !tiresSound.isPlaying && carSpeed > 10f)
                {
                    tiresSound.Play();
                    // carDirection is a dot product (-1 to 1), so we modulate tire pitch
                    // higher if the car is not facing forward and turned at more of an angle
                    tiresSound.pitch = .7f + (1f - carDirection);
                }

                ToggleSkidMarks(skidMark, true);
                ToggleTireSmoke(true);
            }
            else
            {
                ToggleSkidMarks(skidMark, false);
                ToggleTireSmoke(false);
            }
        }
    }

    private void ToggleSkidMarks(TrailRenderer skidMark, bool toggle)
    {
        //skidMark.transform.position = new Vector3(skidMark.transform.position.x, hit.point.y + .05f, skidMark.transform.position.z);
        skidMark.emitting = toggle;
    }

    private void ToggleTireSmoke(bool toggle)
    {
        // loop all the tire smoke particle systems
        foreach(ParticleSystem smoke in tireSmoke)
        {
            if(toggle == true)
            {
                // play the particle system
                smoke.Play();
            }
            else
            {
                // stop the particle system
                smoke.Stop();
            }
        }
    }
    #endregion

    #region Audio
    private void EngineSound()
    {
        engineSound.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, GetNormalizedSpeed());
    }

    // announces best lap for the player car, called by UIController / HUD
    public void PlayBestLapAnnouncement()
    {
        bestLapSound.Play();
    }

    #endregion

    #region Brake Lights
    public void BrakeLightsActive()
    {
        bool inReverse = false;

        // we are going backwards
        if (carDirection < 0)
        {
            // flag to change brake lights to yellow when in reverse
            inReverse = true;
        } 
       
        // set the light color
        for (int i = 0; i < brakeLights.Length; i++)
        {
            Renderer brakeRenderer = brakeLights[i].GetComponent<Renderer>();

            // turn on brake lights, check if going backward
            if (inReverse)
            {
                brakeLightTrails[i].emitting = false;
                brakeRenderer.material = brakeLightMatYellow;
            }
            else
            {
                brakeLightTrails[i].emitting = true;
                brakeRenderer.material = brakeLightMatRed;
            }
        }
    }
    public void HandBrakeLights()
    {

        // set the light color
        for (int i = 0; i < brakeLights.Length; i++)
        {
            brakeLightTrails[i].emitting = true;
            Renderer brakeRenderer = brakeLights[i].GetComponent<Renderer>();
            brakeRenderer.material = brakeLightMatRed;
        }
    }

    public void BrakeLightsInactive()
    {

        for (int i = 0; i < brakeLights.Length; i++)
        {
            // turn off brake lights
            brakeLightTrails[i].emitting = false;
            Renderer brakeRenderer = brakeLights[i].GetComponent<Renderer>();
            brakeRenderer.material = brakeLightMatDefault;

        }
    }

    #endregion
}

