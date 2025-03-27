using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    private float rotateSpeed = 50f;

    void Update()
    {
        transform.Rotate(0f, Time.deltaTime * rotateSpeed, 0f, Space.Self);
    }
}
