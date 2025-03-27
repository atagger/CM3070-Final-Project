using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nitro : MonoBehaviour
{
    [SerializeField] RaceManager raceManager;
    private void Start()
    {
        // get the original rotation
        float originalRotY = transform.eulerAngles.y;

        // check if the track if forward or backwards so we can point the nitro arrow
        // in the correct direction of the race track
        bool trackDir = GameManager.Instance.GetTrackDirection();
       
        // track is backwards, reverse the nitro arrow direction
        if(trackDir == false)
        {
            Quaternion rot = Quaternion.Euler(0f, originalRotY - 180f, 0f);
            transform.rotation = rot;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // NOTE: trigger enter returns the collider, which may not be the object the item is attached to!
        // these are nested in the prefab, so we have to get component the parent object's script
        Vehicle vehicle = other.GetComponentInParent<Vehicle>();

        //Debug.Log("Object that collided with me: " + other.gameObject.name);

        // check the script is attached and the colliding object is the car body
        if (vehicle != null && other.gameObject.name == "Car Body")
        {
            // trigger nitro boost
            vehicle.NitroBoost();
        }
    }

}
