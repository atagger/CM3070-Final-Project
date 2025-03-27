using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics; // for remapping values
public class OpponentCar : Vehicle
{
    // RAYCASTS
    // number of rays to cast for obstacle avoidance
    private int numRays = 61;
    // the center ray
    private int centerRay = 30;
    // how many rays to the left and right of the centerRay
    // these rays check for car collisions in front of the car
    // but they also cause the car to slow down if there is a vehicle in front
    private int carCollisionWhiskerSpan = 2;
    private int carCollisionWhiskerLeft;
    private int carCollisionWhiskerRight;
    // if the ray hits and the car is below this threshold it will brake
    // if the ray hits and the is above this distance, the car will attempt to overtake
    // when travelling faster than the car in front and above .75f of normalized speed
    // between 5.5f and 6f is good, otherwise opponents can crash into the player car
    // which can send the player off course and mess up the race
    float carDistanceThreshold = 7f; 

    private RaycastHit hit;
    private Vector3[] rayDirections;

    // CONTEXT STEERING
    private float[] interest;
    private float[] danger;

    // throttle control - 1 is forward, -1 is backward
    private float throttle;
    // count of the amount of rays that have hit an obstacle
    private int dangerCount = 0;

    // STEERING / PATH FINDING
    float targetSpeed;

    // FRONT OF CAR
    // used as the origin point for turn angles on track
    // but also for raycasting to detect obstacles
    private Transform frontOfCar;

    // CAR STATES
    // bool for checking if the car is stuck
    private bool isCarStuck = false;
    // bool for checking if the car is driving backwards
    private bool isReverse = false;
    // bool for checking if the car is slowing down
    private bool isSlowing = false;

    #region Start
    public override void Start()
    {
        // REF: https://discussions.unity.com/t/why-is-start-from-my-base-class-ignored-in-my-sub-class/25421/6
        // call start in the base class
        base.Start();

        // initialize arrays
        interest = new float[numRays];
        danger = new float[numRays];
        rayDirections = new Vector3[numRays];

        // setup the opponent car raycasts for collision avoidance
        SetupRayCasts();

        // get the transform for the front of the car
        frontOfCar = this.transform.Find("Front Of Car");

    }

    #endregion

    #region Updates

    public override void Update()
    {
        base.Update();

        // apply throttle
        moveInput = throttle;

        // control the brake lights by what the throttle is doing
        BrakeLightsControl(throttle);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // get + cache the normalized car speed
        float normalizedCarSpeed = GetNormalizedSpeed();
        Vector3 targetDirection = GetTargetDirection(normalizedCarSpeed);

        // check if the targetSpeed is less than the speed we are going
        isSlowing = targetSpeed < carRb.velocity.magnitude ? true : false;

        if (isSlowing)
        {
            // if the car's velocity is less than the target speed we are done slowing down
            if (carRb.velocity.magnitude < targetSpeed)
            {
                isSlowing = false;
            }
           // Debug.Log("I am slowing");
        }
        else
        {
            // if we are not slowing down, we travel at the maximum speed
            // NOTE: maxSpeed is capped by the Accelerate() function in vehicle class
            // we inflate the speed here for the nitros to work, otherwise the cars will brake
            targetSpeed = tuning.maxSpeed * 3f;
        }

        // brake for obstacles -- there is danger present and we are about to turn
        if (dangerCount > 0 && GetCarAngle() > 7f && GetCarSpeed() > 20f)
        {
            targetSpeed = tuning.maxSpeed * .6f; //<--- this could be like a risk factor of the driver
        }

        // get the difference of our target speed and current velocity
        float desiredSpeed = (targetSpeed - carRb.velocity.magnitude);

        // check if the game started before we steer or throttle
        if (gameStarted) { 
            if (!isReverse)
            {
                // going forward, muliply by targetSpeed as a throttle curve
                throttle = (desiredSpeed * targetSpeed) / tuning.maxSpeed;
                throttle = Mathf.Clamp(throttle, -1f, 1f);

                // use context steering algorithm when driving forward
                AckermannSteering(ApplySteer(ContextSteering(targetDirection)));
            }
            else
            {
                // going backwards,muliply by targetSpeed as a throttle curve
                throttle = (desiredSpeed * targetSpeed) / -tuning.maxReverseSpeed;
                throttle = Mathf.Clamp(throttle, -1f, 1f);

                // keep wheels straight, we are going backwards
                AckermannSteering(ApplySteer(-transform.forward));
            }
        }
      
        //Debug.Log("vel: " + carRb.velocity.magnitude + " throttle: " + throttle + " targetSpeed is: " + targetSpeed);
        //Debug.DrawRay(carRb.position, targetDirection, Color.magenta);
    }

    #endregion

    #region Get Target Direction
    private Vector3 GetTargetDirection(float normalizedCarSpeed)
    {
        int lookAheadNode = 0;

        if (normalizedCarSpeed > tuning.lookAheadThreshold)
        {
            // look three nodes ahead when we are past a certain amount of speed
            lookAheadNode = 2;
            //Debug.Log("looking ahead 3 nodes");
        }
        else if (normalizedCarSpeed <= tuning.lookAheadThreshold)
        {
            // look two nodes ahead
            lookAheadNode = 1;
            //Debug.Log("looking ahead 2 node");
        }

        // set the steering to the average of the current node we are driving to and the one we are looking at ahead
        Vector3 lookAheadTarget = (nodes[currentNode].position + nodes[nextNode(lookAheadNode)].position) / 2f;

        // normal one look ahead
        //Vector3 lookAheadTarget = (nodes[currentNode].position);
        Vector3 targetDirection = lookAheadTarget - carRb.position;
        return targetDirection;
    }
    #endregion

    #region Vehicle Stuck Check + Reverse Car + Collision Stay
    private IEnumerator ReverseVehicle(float waitTime)
    {
        isReverse = true;
        targetSpeed = -tuning.maxReverseSpeed;
        yield return new WaitForSeconds(waitTime);
        //carRb.AddForceAtPosition(new Vector3(0f, 0f, -10000f), frontOfCar.position, ForceMode.Impulse);
        isReverse = false;
        isCarStuck = false;
        //Debug.Log("coroutine ended: " + Time.time + " seconds");
    }

    void OnCollisionStay()
    {
        // check if we are stopped
        if (carRb.velocity.magnitude < 0.1f)
        {
            //Debug.Log("we are stuck on an object");
            if(isCarStuck == false)
            {
                StartCoroutine(ReverseVehicle(3));
                isCarStuck = true;
            }
        }
    }
    #endregion

    #region Set Up Ray Casts
    private void SetupRayCasts()
    {

        // whiskers to the left and right of center ray, used for behaviors if 
        // there is a car in front
        carCollisionWhiskerLeft = centerRay - carCollisionWhiskerSpan;
        carCollisionWhiskerRight = centerRay + carCollisionWhiskerSpan;

        // set the angles of the raycasts
        for (int i = 0; i < numRays; i++)
        {
            // angle in radians
            float angle = -Mathf.PI / 2 + (Mathf.PI / (numRays - 1)) * i;
            // ray directions along y-axis
            rayDirections[i] = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, Vector3.up) * Vector3.forward;
        }
    }

    #endregion

    #region Context Steering
    private Vector3 ContextSteering(Vector3 target)
    {
        // set the interest point to the current track node
        SetInterest(target);
        // raycast to any surrounding dangers
        SetDanger(target);
        // chose a steering direction via contextSteer
        return ChooseDirection();
    }

    private void SetInterest(Vector3 target)
    {
        for (int i = 0; i < interest.Length; i++)
        {
            //Vector3 pathDirection = target - transform.position;
            // returns a number from -1 to 1 where 0 is perpendicular to ray, 1 is same direction, -1 is opposite direction
            float weight = Vector3.Dot(target.normalized, transform.TransformDirection(rayDirections[i]).normalized);
            //Debug.DrawRay(frontOfCar.position, rayDirections[i].normalized * 50f, Color.white);
            //Debug.Log("ray: " + i + "interest weight val: " + weight);

            // set a level of interest weight to the target object
            interest[i] = Mathf.Max(0, weight);
        }

         //Debug.DrawRay(transform.position, target - transform.position, Color.yellow);
         
         // ray to show the car's current interest
         //Debug.DrawRay(transform.position, target, Color.yellow);
    }

    private void SetDanger(Vector3 target)
    {
        dangerCount = 0;

        for (int i = 0; i < danger.Length; i++)
        {
            // transform the ray direction to world space
            Vector3 rayDirection = transform.TransformDirection(rayDirections[i]);
            // * Vector3.Dot(rayDirections[i].normalized, transform.forward);

            // lock y-axis of ray direction so it doesn't point up or down when car turns w/the suspension
            // this is important to avoid hitting areas like terrain if the car tilts slightly in turns
            rayDirection.y = 0f;

            // make a sphere / fan of raycasts by multiplying by the dot product of the transform.forward
            float castLength = tuning.rayLength * Vector3.Dot(rayDirection.normalized, transform.forward);
  

            if (Physics.Raycast(transform.position, rayDirection, out hit, castLength, layerMask))
            {
                // debug to draw ray showing a hit and where it is
                //Debug.DrawLine(transform.position, hit.point, Color.red);
                
                //Debug.Log("opponent hit object: " + hit.collider.gameObject.name);

                // do not steer in this direction
                danger[i] = 1f;

                // if we collided ith another car
                if (hit.collider.gameObject.name == "Car Body" && i >= carCollisionWhiskerLeft && i <= carCollisionWhiskerRight)
                {
                    //Debug.Log(hit.collider.gameObject.transform.position);

                    //float distanceToOther = hit.distance;
                    float speedDelta = 0f;

                    // because the collider is inside the pre-fab as a child we have to get the 
                    // interface from the root level of the object
                    Vehicle otherCar = hit.transform.root.gameObject.GetComponent<Vehicle>();

                    // check if the otherCar component is not null
                    // if it is not, we can get it's current speed
                    if(otherCar != null)
                    {
                        // compare the other car's speed to our own speed
                        speedDelta = GetCarSpeed() - otherCar.GetCarSpeed();
                        //Debug.Log("my speed is " + GetCarSpeed() + " other car's speed is: " + otherCar.GetCarSpeed() + " and delta is: " + speedDelta + " distance to impact is: " + hit.distance);
                        //Debug.Log("I am: " + this.name + "the other car is: " + otherCar.name);
                    }

                    // if we are two car lengths away slow down if there is at least a small speed delta
                    // we need to include speedDelta > # in case the cars are not moving (i.e. the finish line)
                    // if we don't it is possible they slow down and then don't start accelerating resulting in them stuck
                    if(hit.distance < carDistanceThreshold && speedDelta > .5f)
                    {
                        // slow down to the speed of the car in front of it
                        isSlowing = true;
                        // scale the speed differential by a scalar
                        targetSpeed = speedDelta * 0.7f;
                    }

                    // if we are approaching another car and moving at a higher rate of speed than the other car
                    // increase speed to try and overtake it, but we also try to avoid the other car
                    if (hit.distance >= carDistanceThreshold && speedDelta > 5f && GetNormalizedSpeed() > 0.75f)
                    {
                        ///Debug.Log("trying to overtake an opponent");
                        carRb.AddForce(10f * transform.forward, ForceMode.Acceleration);
                    }

                }
                else
                {
                    // there is a raycast hit, set danger to 1, we do not want to go in this direction
                    danger[i] = 1f;

                    // obstacles present
                    dangerCount++;
                }

            }
            else
            {
                //Debug.DrawRay(transform.position, rayDirection * castLength, Color.blue);

                //shows the car collision whisker rays
                //if (i >= carCollisionWhiskerLeft && i <= carCollisionWhiskerRight)
                //{
                //    Debug.DrawRay(transform.position, rayDirection * castLength, Color.blue);
                //}

                // there is no hit, set the danger to 0
                danger[i] = 0.0f;
            }
        }
    }

    private Vector3 ChooseDirection()
    {
        float highestInterestValue = 0f; // stores highest value of interest
        int highestInterestIndex = 0; // stores index of the highest interest
        Vector3 chosenDirection = Vector3.zero; // stores the direction we want to go

        for (int i = 0; i < rayDirections.Length; i++)
        {
            // check for danger
            if (danger[i] > 0f)
            {
                // negate the interest if there is danger and set interest to 0
                interest[i] = 0f;
                //interest[i] = interest[i] - danger[i];
            }

            if (interest[i] > highestInterestValue)
            {
                highestInterestValue = interest[i]; // update value of highest interest for next pass
                highestInterestIndex = i; // the index of the best interest value
            }
        }

        // the highest interest ray from context steering
        chosenDirection = transform.TransformDirection(rayDirections[highestInterestIndex]);

        // additional buffer for feeler raycasts (this functions as a look ahead)
        // adding a buffer allows us to make sure there are no further obstacles to the left or right
        // of the chosen direction the context steering is wanting the car to go
        float additionalRayDist = 10f;

        // amount to offset the car by for a clear path to drive around obstacle
        // should be several times the width of the vehicle to ensure safe passage
        // NOTE: the car body is 1.5f wide, so we scale this by a factor
        float avoidanceDistance = 1.5f * 3f;

        // angle of whisker from chosen direction to left and right sides of it
        // 4.5 - 6f seem to work well
        float whiskerAngle = 6f;

        // send a ray left and right of the chosen direction, rotated by the whisker angle
        // this determines if there is an obstacle on the left or right side of the car
        Vector3 rayCheckLeft = Quaternion.Euler(0, -whiskerAngle, 0) * chosenDirection; 
        Vector3 rayCheckRight = Quaternion.Euler(0, whiskerAngle, 0) * chosenDirection;

        bool hitOnLeft = Physics.Raycast(frontOfCar.position, rayCheckLeft, out RaycastHit leftHit, tuning.rayLength + additionalRayDist, layerMask);
        bool hitOnRight = Physics.Raycast(frontOfCar.position, rayCheckRight, out RaycastHit rightHit, tuning.rayLength + additionalRayDist, layerMask);
        //Debug.DrawRay(frontOfCar.position, rayCheckLeft * (tuning.rayLength + additionalRayDist), Color.green);
        //Debug.DrawRay(frontOfCar.position, rayCheckRight * (tuning.rayLength + additionalRayDist), Color.green);
        //Debug.DrawRay(transform.position, chosenDirection * 50f, Color.green);

        // if we have a hit on left, we go right
        if (hitOnLeft && !hitOnRight)
        {
            //Debug.Log("hitOnLeft");            
            
            // get the avoidance angle using trig from opp/adjacent
            float avoidanceAngle = Mathf.Rad2Deg * Mathf.Atan(avoidanceDistance / leftHit.distance);

            //Debug.DrawRay(frontOfCar.position, chosenDirection * (tuning.rayLength + 50f), Color.yellow);

            chosenDirection = Quaternion.Euler(0, avoidanceAngle, 0) * rayCheckRight;
            
            //Debug.DrawRay(transform.position, rayCheckLeft * (tuning.rayLength + additionalRayDist), Color.red);
            //Debug.DrawRay(transform.position, rayCheckRight * (tuning.rayLength + additionalRayDist), Color.red);

            //Debug.DrawRay(frontOfCar.position, chosenDirection * (tuning.rayLength + additionalRayDist), Color.magenta);
        }

        // if we have a hit on right, we go left
        if (hitOnRight && !hitOnLeft)
        {
            //Debug.Log("hitOnRight");

            // get the avoidance angle using trig from opp/adjacent
            float avoidanceAngle = Mathf.Rad2Deg * Mathf.Atan(avoidanceDistance / rightHit.distance);

            //Debug.DrawRay(frontOfCar.position, chosenDirection * (tuning.rayLength + 50f), Color.yellow);
            chosenDirection = Quaternion.Euler(0, -avoidanceAngle, 0) * rayCheckLeft;

            //Debug.DrawRay(frontOfCar.position, chosenDirection * (tuning.rayLength + 50f), Color.magenta);

            //Debug.DrawRay(transform.position, rayCheckLeft * (tuning.rayLength + additionalRayDist), Color.green);
            //Debug.DrawRay(transform.position, rayCheckRight * (tuning.rayLength + additionalRayDist), Color.red);
        }

        //Debug.DrawRay(transform.position, chosenDirection * 50f, Color.magenta);

        return chosenDirection.normalized;
    }

    #endregion

    #region Finding Track Nodes
    private int nextNode(int lookAhead)
    {
        // returns the track node n positions in the future
        int nextNode;

        // if we are before the end of the list, we add
        if (currentNode < nodes.Count - lookAhead)
        {
            nextNode = currentNode + lookAhead;
        }
        else
        {
            // we are at the end of the list, the nextNode is 0
            nextNode = 0;
        }

        return nextNode;
    }

    //private int GetPreviousNode()
    //{
    //    // get the previous node from the current one
    //    if (currentNode == 0)
    //    {
    //        // if we are on the first node, the previous is the last node in list
    //        previousNode = nodes.Count - 1;
    //    }
    //    else
    //    {
    //        // otherwise use the node before it
    //        previousNode = currentNode - 1;
    //    }

    //    return previousNode;
    //}

    #endregion

    #region Braking + Speed Control + Race Finish Trigger

    private void OnTriggerEnter(Collider other)
    {
        // check if the car crossed finish line
        if (other.gameObject.name == "Finish Line Trigger")
        {
            // in Vehicle() base class
            CrossFinishCheck();
        }

        // check the script is attached and the colliding object is the car body
        if (other.gameObject.name == "Slow Down")
        {
            // set a slow down speed
            targetSpeed = tuning.maxSpeed * 0.75f;
            //Debug.Log("collision");
        }

        // occurs if an opponent falls into the water or drives off the map
        if (other.gameObject.name == "Death Trigger")
        {
            PlayerDied();
            //Debug.Log("an opponent just croaked...");
        }
    }

    #endregion

    #region Visual Effects
    private void BrakeLightsControl(float throttle)
    {
        if (throttle < 0)
        {
            BrakeLightsActive();
        }
        else
        {
            // throttle is negative, show brake lights
            BrakeLightsInactive();
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.yellow;    
    }

    #endregion

    #region Steering
    private float ApplySteer(Vector3 steerTarget)
    {
        Vector3 relativeVector = steerTarget; // - transform.position;
        Vector3 steeringCrossProduct = Vector3.Cross(transform.forward, relativeVector.normalized);
        float steer = steeringCrossProduct.y;
        return steer;
    }

    #endregion

}