using UnityEngine;

[CreateAssetMenu(fileName = "CarSettings", menuName = "ScriptableObjects/CarSettingsObject", order = 1)]
public class CarSettings : ScriptableObject
{
    [SerializeField] public AnimationCurve steeringCurve;
    [SerializeField] public AnimationCurve powerCurve;
    [SerializeField] public AnimationCurve tractionCurve;
    [SerializeField] public AnimationCurve reversePowerCurve;

    [SerializeField] public float maxSpeed = 75f;
    [SerializeField] public float maxReverseSpeed = 10f;

    [SerializeField] public float enginePower = 2000f;
    [SerializeField] public float decelFactor = 35f;
    [SerializeField] public float decelFactorOffRoad = 6f;

    [SerializeField] public float springStrength = 7500f;
    [SerializeField] public float damperStrength = 500f;
    [SerializeField] public float restLength = .4f;
    [SerializeField] public float springTravel = .2f;

    [SerializeField] public float tireGrip = .08f;
    [SerializeField] public float tireMass = .5f;
    [SerializeField] public float wheelRadius = .35f;
    [SerializeField] public float wheelSpinStrength = 100f;

    [SerializeField] public float lerpSteerTime = 8f;
    [SerializeField] public float ackTurnRadius = 10f;
    [SerializeField] public float stdSteerAngle = 30f;

    [SerializeField] public float lookAheadThreshold = 0.5f;
    [SerializeField] public float rayLength = 55f;
}
