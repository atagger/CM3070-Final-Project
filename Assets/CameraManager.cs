using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Camera level1Camera;
    [SerializeField] Camera level2Camera;
    [SerializeField] Camera level3Camera;
    [SerializeField] Camera raceCamera;

    private void Awake()
    {
        // default on the level camera when we start the game
        level1Camera.gameObject.SetActive(false);
        level2Camera.gameObject.SetActive(false);
        level3Camera.gameObject.SetActive(false);
        raceCamera.gameObject.SetActive(false);
    }

    public void RaceCam()
    {
        // switch to the race camera
        level1Camera.gameObject.SetActive(false);
        level2Camera.gameObject.SetActive(false);
        level3Camera.gameObject.SetActive(false);
        raceCamera.gameObject.SetActive(true);
    }

    public void CameraSwitcher(int level)
    {
        // switches camera
        // set in inspector under race manager
        switch(level)
        {
            case 0:
                level1Camera.gameObject.SetActive(true);
                level2Camera.gameObject.SetActive(false);
                level3Camera.gameObject.SetActive(false);
                raceCamera.gameObject.SetActive(false);
                break;
            case 1:
                level1Camera.gameObject.SetActive(false);
                level2Camera.gameObject.SetActive(true);
                level3Camera.gameObject.SetActive(false);
                raceCamera.gameObject.SetActive(false);
                break;
            case 2:
                level1Camera.gameObject.SetActive(false);
                level2Camera.gameObject.SetActive(false);
                level3Camera.gameObject.SetActive(true);
                raceCamera.gameObject.SetActive(false);
                break;
            default:
                break;
        }

    }

}
