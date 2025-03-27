using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "ScriptableObjects/CameraSettingsObject", order = 2)]
public class CameraSettings : ScriptableObject
{
    [SerializeField] public float rotationSpeed = 35f;
    [SerializeField] public float followSpeed = 35f;
    [SerializeField] public float cameraHeight = 5f;
    [SerializeField] public float followDistance = 15f;
}

