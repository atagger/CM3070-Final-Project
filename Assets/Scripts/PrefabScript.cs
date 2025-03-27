using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabScript : MonoBehaviour
{

    // CAR BODIES
    [SerializeField] public GameObject[] carBodies;

    // WHEELS
    [SerializeField] public GameObject FL;
    [SerializeField] public GameObject FR;
    [SerializeField] public GameObject RL;
    [SerializeField] public GameObject RR;
    [SerializeField] public GameObject[] wheels;
    [SerializeField] public GameObject[] wheelsChild;
    [SerializeField] public bool[] isMotor;

    // VFX
    [SerializeField] public TrailRenderer[] skidMarks;
    [SerializeField] public GameObject[] brakeLights;
    [SerializeField] public TrailRenderer[] brakeLightTrails;
    [SerializeField] public Material brakeLightMatRed;
    [SerializeField] public Material brakeLightMatYellow;
    [SerializeField] public Material brakeLightMatDefault;
    [SerializeField] public ParticleSystem[] tireSmoke;
   
    // SOUNDS
    [SerializeField] public AudioSource engineSound;
    [SerializeField] public AudioSource tiresSound;
    [SerializeField] public AudioSource impactSound;

    [SerializeField] public AudioSource nitroSound;
    [SerializeField] public AudioSource finalLapSound;

    [SerializeField] public AudioSource firstPlaceSound;
    [SerializeField] public AudioSource secondPlaceSound;
    [SerializeField] public AudioSource thirdPlaceSound;
    [SerializeField] public AudioSource fourthPlaceSound;
    [SerializeField] public AudioSource fifthPlaceSound;
    [SerializeField] public AudioSource youLostSound;

    [SerializeField] public AudioSource bestLapSound;
    [SerializeField] public AudioSource makeHasteSound;
    [SerializeField] public AudioSource hazardDetectedVoice;

}

