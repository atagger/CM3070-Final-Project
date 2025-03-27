using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceCamera : MonoBehaviour
{
    [SerializeField] CameraSettings cameraSettings;
    [SerializeField] RaceManager raceManager;

    private GameObject objectToFollow;
    private Transform target;

    // higher values are a slower zoom
    private float dampSpeed = 40f;
    private bool isZooming = true;

    private Vector3 velocity = Vector3.zero;

    public void setObjectToFollow(GameObject objectToFollow)
    {
        // this is set in spawner so we can dynamically assign the camera to instantiated objects
        target = objectToFollow.GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        if (this.gameObject.activeSelf == false) return;

            // create a vector for the camera vector based on the player position
            Vector3 targetPosition = target.TransformPoint(new Vector3(0f, cameraSettings.cameraHeight, -cameraSettings.followDistance));

            // fix the camera so it doesn't flip under the terrain if the car is turned over
            targetPosition = new Vector3(targetPosition.x, Mathf.Abs(targetPosition.y), targetPosition.z);

            //transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSettings.followSpeed * Time.deltaTime);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dampSpeed * Time.fixedDeltaTime);

            // create a vector pointing to the player
            Vector3 direction = target.position - transform.position;

            // check if the zoom is done, we add a small amount because the smooth damp 
            // never actually reaches the target, but comes close to it
            if (isZooming && direction.magnitude < cameraSettings.followDistance + 0.9f)
            {
                // set the follow speed to fast
                dampSpeed = 1f;

            if (GameManager.Instance.tutorialComplete)
            {
                // start the race countdown animation
                raceManager.StartCountDown();
            } else
            {
                // if we want to show the tutorial
                //raceManager.ShowTutorial();

                // if we want to show the difficulty screen
                raceManager.ShowDifficultyScreen();
            }

                // set a flag so we only do this once
                isZooming = false;
            }

            //Debug.Log("camera is: " + direction.magnitude + " from target car");

            // rotate the camera towards the player
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, cameraSettings.rotationSpeed * Time.deltaTime);
    }
}
