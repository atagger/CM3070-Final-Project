using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{

    [SerializeField] private GameObject player;
    private Transform target;


    // LOWER VALUES MAKE CAMERA PAN
    private float rotSpeed = 35f;
    private float followSpeed = 35f;

    private float camHeight = 5f;
    private float followDistance = 15f;

    void Start()
    {
        target = player.GetComponent<Transform>();
        Time.timeScale = 1f;

    }

    private void FixedUpdate()
    {

        Vector3 targetPos = target.TransformPoint(new Vector3(0f, camHeight, -followDistance));
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // point the camera to player
        var direction = target.position - transform.position;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSpeed * Time.deltaTime);
    }
}
