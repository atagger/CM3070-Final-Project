using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{

    [SerializeField] private Vector3 rotation = Vector3.zero;
    [SerializeField] private float maxRotationSpeed = 40f;
    Wind wind;

    private void Start()
    {
        GameObject windGeneratorObject = GameObject.FindGameObjectWithTag("WindGenerator");
        wind = windGeneratorObject.GetComponent<Wind>();
    }

    void Update()
    {
        // get the dot product of the wind direction
        float windDot = Vector3.Dot(transform.forward, wind.windDirection);
        // modulate the rotation by the dot product
        transform.Rotate(new Vector3(0f, 0f, windDot * maxRotationSpeed * Time.deltaTime));
    }
}
