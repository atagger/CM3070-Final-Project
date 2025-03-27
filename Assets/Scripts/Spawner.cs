using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // text mesh pro for fonts    
using UnityEngine.InputSystem;
using System; // for tuples
using System.Linq; // tuple sorting
using UnityEngine.Rendering;
//using UnityEditor.Experimental.GraphView; // for motion blur trigger

public class Spawner : MonoBehaviour
{
    // list of all the game's car
    public List<GameObject> cars = new List<GameObject>();

    // list of all the cars in the race
    List<Vehicle> carsInRace = new List<Vehicle>();

    // reference to the car prefab
    public GameObject carPreFab;

    // tuning for the vehicles
    [SerializeField] CarSettings playerTuning;
    [SerializeField] CarSettings opponentTuning;
    [SerializeField] CarSettings opponentTuning2;
    [SerializeField] CarSettings opponentTuning3;

    // slots for racing line
    [SerializeField] GameObject slot1;
    [SerializeField] GameObject slot2;
    [SerializeField] GameObject slot3;
    [SerializeField] GameObject slot4;

    // the finish line parent object
    [SerializeField] GameObject finishLine;

    // reference to race camera we want to use
    [SerializeField] RaceCamera raceCamera;
    // reference to racing HUD UI (see UI / Managers in Inspector hierarchy)
    [SerializeField] UIController UI;
    // text for when the player goes the wrong way
    [SerializeField] TextMeshProUGUI UIWrongWay;
    // layer mask for the raycasts
    [SerializeField] LayerMask layerMask;
    // input control scheme for cars
    [SerializeField] InputManager playerInputManager;
    // reference to the race manager (starts and coordinates the next game)
    [SerializeField] RaceManager raceManager;

    //[SerializeField] Transform track1Nodes;
    List<Transform> currentRaceTrackNodes;

    // race ranking strings for racing HUD UI 
    private string playerRaceRanking;
    private int playerRaceRankingNumber;

    // flag for if the race is finished
    private bool raceFinished = false;

    // VFX volume for motion blur on nitros
    [SerializeField] Volume nitroBlurVolume;

    int levelIndex;

    #region RacerStats Class
    // custom class for race rankings
    private class RacerStats    
    {
        // reference to the car object
        public Vehicle car { get; set; }

        // vehicle name
        public string name { get; set; }

        // vehicle index of carsInRace
        public int carIndex { get; set; }

        // checkpoints collected
        public int collected { get; set; }

        // distance car is to checkpoint
        public float distance { get; set; }

        // total checkpoints in race
        public int total { get; set; }

        // total lap time of car
        public float totalLapTime { get; set; }

        // flag if the individual car is finished
        public bool carIsFinished { get; set; }

        // racer stats constructor
        public RacerStats(Vehicle _car, string _name, int _carIndex, int _collected, float _distance, int _totalCheckPoints, float _totalLapTime, bool _carIsFinished)
        {
            car = _car;
            name = _name;
            carIndex = _carIndex;
            collected = _collected;
            distance = _distance;
            total  = _totalCheckPoints;
            totalLapTime = _totalLapTime;
            carIsFinished = _carIsFinished;
        }
    }
    #endregion

    private void Start()
    {
        levelIndex = GameManager.Instance.GetLevel();
        //Debug.Log("starting level is:" + levelIndex);

        // disable motion blur effect volume on start for nitro, this is triggered when player car runs over it
        nitroBlurVolume.enabled = false;

        // get the current track nodes the car is racing on, from the race manager
        currentRaceTrackNodes = raceManager.GetTrackNodes();

        // get the track direction we are racing --> false is backward
        bool isTrackForward = GameManager.Instance.GetTrackDirection();
        //Debug.Log("track direction is: " + isTrackForward);

        // cache the start position and rotation for spawning the cars
        Vector3[] racePositionsStart;

        if (isTrackForward == true)
        {
            // DRIVING TRACK IN FORWARD DIRECTION
            // list arranged in order of places - forward direction
            racePositionsStart = new[]{ new Vector3(slot1.transform.position.x, .5f, slot1.transform.position.z),
                                        new Vector3(slot2.transform.position.x, .5f, slot2.transform.position.z),
                                        new Vector3(slot3.transform.position.x, .5f, slot3.transform.position.z),
                                        new Vector3(slot4.transform.position.x, .5f, slot4.transform.position.z) };

            // camera origin for zoom
            //raceCamera.transform.position = new Vector3(1200, 120, -1200);
            raceCamera.transform.position = raceManager.GetForwardCamPos(levelIndex);
            raceCamera.transform.rotation = raceManager.GetForwardCamRot(levelIndex);

            //raceCamera.transform.rotation = raceManager.GetForwardCam(levelIndex).transform.rotation;
        }
        else
        {
            // DRIVING THE TRACK IN REVERSE DIRECTION
            // list arranged in order of places - reversed direction, pole position is on opposite side
            racePositionsStart = new[]{ new Vector3(slot2.transform.position.x, .5f, slot2.transform.position.z),
                                        new Vector3(slot1.transform.position.x, .5f, slot1.transform.position.z),
                                        new Vector3(slot4.transform.position.x, .5f, slot4.transform.position.z),
                                        new Vector3(slot3.transform.position.x, .5f, slot3.transform.position.z) };

            // camera origin for zoom
            raceCamera.transform.position = raceManager.GetBackwardsCamPos(levelIndex);
            raceCamera.transform.rotation = raceManager.GetBackwardsCamRot(levelIndex);
        }

        // face the cars the direction of the finish line
        Quaternion startRotation = finishLine.transform.rotation;

        // names of all the vehicles
        String[] vehicleNames = { "Player Car", "Opponent Car 1", "Opponent Car 2", "Opponent Car 3" };

        //-----------------------------------------------------------------------------------------------------------
        // *** CAR SPAWNING ***
        // there is not a constructor in Unity, and since we are spawning vehicles dynamically we must pass them
        // references to any objects they will use....these references are set in the spawner's inspector
        //-----------------------------------------------------------------------------------------------------------
        // PlayerCar is a sub-class of vehicle, OpponentCar is a sub-class of vehicle
        //-----------------------------------------------------------------------------------------------------------

        // add ONE player car (in the future there might be more)
        for (int i = 0; i < 1; i++)
        {
            // the game manager holds the placement of racers for the starting line
            // this list is updated based on wins in races and sorted in the order cars are created
            // slot zero is always the player, slots 1-3 are the opponents
            int carStartingPosition = GameManager.Instance.GetStartSlot(i);

            // instantiates the player car at a specific position and rotation, based on the gameManager
            GameObject vehicle = Instantiate(carPreFab, racePositionsStart[carStartingPosition], startRotation);
            // set the car's name
            vehicle.name = "Player Car";
            // add the PlayerCar sub-class
            vehicle.AddComponent<PlayerCar>();

            /////////////////////////////////////////////////////////////////////
            // set the car body type // 0 = player hot rod, 1 = station wagon , 2 = van, 3 = truck
            vehicle.GetComponent<PlayerCar>().carBodyIndex = 0;
            //SetCarBody(vehicle, "Station Wagon");
            /////////////////////////////////////////////////////////////////////

            // pass the global volume object for the motion blur for when player triggers nitro
            vehicle.GetComponent<PlayerCar>().nitroBlurVolume = nitroBlurVolume;
            // pass the race manager object to check when the race starts
            vehicle.GetComponent<PlayerCar>().raceManager = raceManager;
            // assign an input manager to control the player car (this is the controls)
            vehicle.GetComponent<PlayerCar>().inputManager = playerInputManager;
            // set player tuning from scriptable object
            //vehicle.GetComponent<PlayerCar>().tuning = playerTuning;
            vehicle.GetComponent<PlayerCar>().tuning = GameManager.Instance.playerCarTuningClone;
            // set the steering radius based on the track for a more enjoyable experience
            vehicle.GetComponent<PlayerCar>().tuning.ackTurnRadius = GameManager.Instance.GetSteering();

            // send the car the current track nodes
            vehicle.GetComponent<PlayerCar>().nodes = currentRaceTrackNodes;
            // add the car to carsInRace list so we can rank the cars, check for race end, and have a reference to their script
            carsInRace.Add(vehicle.GetComponent<Vehicle>());
            // set a reference to the player car for the race UI
            UI.setCarReference(vehicle);
            // follow the player with camera
            raceCamera.setObjectToFollow(vehicle);

            // add vehicle to cars list in case we want to destroy all the car gameobjects
            cars.Add(vehicle);
        }

        // OPPONENTS -> THIS must start at 1, since player is always at 0 index in the list
        for (int i = 1; i < 4; i++)
        {
            //Debug.Log("position is: " + GameManager.Instance.GetStartSlot(i));
            // the game manager holds the placement of racers for the starting line
            // this list is updated based on wins in races and sorted in the order cars are created
            // slot zero is always the player, slots 1-3 are the opponents
            int carStartingPosition = GameManager.Instance.GetStartSlot(i);

            // instantiates the opponent car at a specific position and rotation
            GameObject vehicle = Instantiate(carPreFab, racePositionsStart[carStartingPosition], startRotation);
            // set the car's name
            vehicle.name = "Opponent Car";
            // add the OpponentCar sub-class
            vehicle.AddComponent<OpponentCar>();

            /////////////////////////////////////////////////////////////////////
            // set the car body type // 0 = player hot rod, 1 = station wagon , 2 = van, 3 = truck
            vehicle.GetComponent<OpponentCar>().carBodyIndex = i;
            /////////////////////////////////////////////////////////////////////

            // pass the race manager object to check when the race starts
            vehicle.GetComponent<OpponentCar>().raceManager = raceManager;
            // set opponent tuning from scriptable object
            //vehicle.GetComponent<OpponentCar>().tuning = opponentTuning;
            vehicle.GetComponent<OpponentCar>().tuning = GameManager.Instance.opponentCarTuningClones[i-1];

            // layermask for opponent ray casts
            vehicle.GetComponent<OpponentCar>().layerMask = layerMask;
            // send the car the current track nodes
            vehicle.GetComponent<OpponentCar>().nodes = currentRaceTrackNodes;
            // add the car to carsInRace list so we can rank the cars, check for race end, and have a reference to their script
            carsInRace.Add(vehicle.GetComponent<Vehicle>());

            // add vehicle to cars list in case we want to destroy all the car gameobjects
            cars.Add(vehicle);

            // if we want to follow an opponent instead of the player car
            //raceCamera.setObjectToFollow(vehicle);
        }
    }

    public void SetCarBody(GameObject objToSearch, String name)
    {
        // find the specified car body
        Transform cBody = objToSearch.transform.Find(name);
        // cast to a game object
        GameObject carBody = cBody.gameObject;
        // set the car body to active
        carBody.SetActive(true);
    }

    private void Update()
    {
        if (!raceFinished)
        {
            // rank of all the cars in the race for UI Controller
            // this shows on the screen as 1/2, 3/5, etc.
            List<RacerStats> raceRankings = GetRaceRankings();

            // create a delta of how many checkpoints ahead the lead car is from the second ranked
            // if it is ahead n checkpoints, we need to slow it down
            int checkPointDelta = raceRankings[0].collected - raceRankings[1].collected;

            for (int i = 0; i < raceRankings.Count; i++)
            {
                // ----- ON SCREEN RANKING -----
                // string to output for interface that shows the player position
                // we use (i+1) so the output text makes sense to the person playing game
                if (raceRankings[i].name == "Player Car")
                {
                    playerRaceRanking = (i + 1).ToString() + "/" + raceRankings.Count.ToString();
                }

                // RUBBER BANDING
                // if the leader is more than two checkpoints ahead slow the leader down and boost everyone else
                if (checkPointDelta > 2)
                {
                    // check if the player is not in first place and it is an opponent, so we can penalize it
                    if (raceRankings[i].car.name != "Player Car") {
                        if (i == 0)
                        {
                            // penalize the lead car
                            //Debug.Log("Penalizing: " + raceRankings[0].name);

                            // penalize the engine
                            // takes length of cooldown and engine modifier as args
                            if (raceRankings[0].car.isBoosted == false) StartCoroutine(raceRankings[0].car.UpOrDownGradeEngine(4f, 0.75f));
                        }
                        else
                        {
                            // boost all the other cars so they can catch up to leader
                            // boost the engine
                            // takes length of cooldown and engine modifier as args
                            if (raceRankings[i].car.isBoosted == false) StartCoroutine(raceRankings[i].car.UpOrDownGradeEngine(7f, 1.65f));
                        }
                    }
                }
                
            }

            // RACE FINISH CHECK
            // loop to check if race is done (PLAYER HAS CROSSED THE FINISH)
            for (int i = 0; i < carsInRace.Count; i++)
            {
                // RACE FINISHED condition, player triggers it
                if (carsInRace[i].name == "Player Car" && carsInRace[i].isFinished)
                {  
                    raceFinished = true;
                    GenerateFinalRankings(raceRankings);
                    return; // break out of loop as soon as player car finishes race
                }
            }

        }
    }

    private void GenerateFinalRankings(List<RacerStats> raceRankings)
    {
        // reset UI place position rank
        playerRaceRanking = "-";

        // container to hold rankings for output
        string finalResults = "";

        // order list ascending
        raceRankings = raceRankings.OrderBy(t => t.totalLapTime).ToList();

        // get the worst current completion time of all the racers
        float worstTime = raceRankings[raceRankings.Count-1].totalLapTime;
        // pick a random base time to add to all of the losers
        float randomBaseTime = UnityEngine.Random.Range(1f, 6f);

        for (int i = 0; i < raceRankings.Count; i++)
        {
            // check for racers that have not finished
            if(raceRankings[i].carIsFinished == false)
            {
                // check how much of the race they have completed (0-1)
                // NOTE: IMPORTANT: raceRankings.collected / total are ints!
                // we have to cast int to float to get a floast, otherwise we will not get a percentage
                float raceCompletion = (float)raceRankings[i].collected / (float)raceRankings[i].total;
                // create a positive scalar based on their completion
                float timeScalar = 1f + (1f - raceCompletion);
                // multiply the random base val by i and then add some random numbers so i+1 is always greater
                float random = (i + randomBaseTime) + UnityEngine.Random.Range(0f, randomBaseTime-1f);
                // make up a fake time of completion based on the scalar and worst time, then add some randomness
                //Debug.Log("race completion at end: " + raceCompletion + " time scalar is: " + timeScalar + " collected: " + raceRankings[i].collected + " total " + raceRankings[i].total);
                raceRankings[i].totalLapTime = (worstTime * timeScalar) + random;
            }

            //Debug.Log("R: " + (i + 1) + " " + raceRankings[i].carIsFinished + " " + raceRankings[i].name + " T " + raceRankings[i].totalLapTime + "!!!!!!!!!!!!BEFORE!!!!!!!!!!");
        }

        // re-sort rakings after inserting fake times
        raceRankings = raceRankings.OrderBy(t => t.totalLapTime).ToList();

        // FINAL race rankings routine
        for (int i = 0; i < raceRankings.Count; i++)
        {

            // set the starting position for the next race
            // this is set by the carIndex (player car is always 0 index)
            // and then by the rank the car came in for the race
            // i.e., a 1st place win results in the player starting pole position for next race
            GameManager.Instance.SetStartSlot(raceRankings[i].carIndex, i);

            // add one to rank since i starts at zero
            int rank = i + 1;

            // if it's the player car run the SetPlayerRank routing
            // this put the ranking on screen and allows player to proceed to next race
            if (raceRankings[i].name == "Player Car")
            {
                raceManager.SetPlayerRank(rank);
            }

            // the ranking number for the results output
            raceRankings[i].car.rankNum = rank;

            // the race results for output
            String res = (rank).ToString() + "  " + 
                         raceRankings[i].name + "  " + 
                         FormatLapTime(raceRankings[i].totalLapTime).ToString() + "<br>";

            // concat all results into one long string with line breaks for output on screen
            finalResults += res;

            // Debug.Log("R: " + (i + 1) + " " + raceRankings[i].carIsFinished + " " + raceRankings[i].name + " T " + raceRankings[i].totalLapTime + "!!!!!!!!!!!!AFTER!!!!!!!!!!");
        }

        // show the results screen
        raceManager.ShowRaceResults(finalResults);

    }

    // formats the lap time for leaderboard
    private string FormatLapTime(float timeIn)
    {
        // format lap time for race results leaderboard
        string formattedTime = $"{(int)timeIn / 60}:{(timeIn) % 60:00.000}";
        return formattedTime;
    }

    public String GetRaceRankingText()
    {
        // used by the "UI Controller" to get the race ranking of player car
        // this is shown on the screen as 1/4, 3/5, etc.
        return playerRaceRanking;
    }

    private List<RacerStats> GetRaceRankings()
    {
        // THIS USED FOR THE PLAYER POSITION i.e. 1/4, 2/4, etc.
        // creates a list of tuples that stores the car name, amount of checkpoints collected,
        // and the distance to the checkpoint
        List<RacerStats> raceRankings = new List<RacerStats>();

        for (int i = 0; i < carsInRace.Count; i++)
        {
            Vehicle car = carsInRace[i];
            string carName = carsInRace[i].name;
            int carIndex = i;
            int checkPointsCollected = carsInRace[i].checkPointsCollected;
            int totalCheckPoints = carsInRace[i].totalCheckPoints;
            float totalLapTime = carsInRace[i].totalLapTime;
            bool carIsFinished = carsInRace[i].isFinished;

            // subtract the distance from a big number so it is a greater value when car is close
            // we need this for the sorting routine below
            float distanceToCheckPoint = 10000000f - carsInRace[i].GetSquareDistance();

            // create the custom racerStats object
            RacerStats record = new RacerStats(car, carName, carIndex, checkPointsCollected, distanceToCheckPoint, totalCheckPoints, totalLapTime, carIsFinished);
            // add the object to the raceRanking, this is what we will sort
            raceRankings.Add(record);

            //Debug.Log(carName + " finished with: " + checkPointsCollected);
        }

        // sort list order by the checkPoints hit and then the distance to check point
        // see class definition for RacerStats above Start() block for more info
        raceRankings = raceRankings.OrderByDescending(t => t.collected).ThenByDescending(t => t.distance).ToList();
        return raceRankings;
    }

    public void KillAllPlayers()
    {
        // destroy all the cars
        foreach (GameObject car in cars)
        {
            Destroy(car);
        }
    }

} // END