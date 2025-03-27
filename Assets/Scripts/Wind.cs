using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    // Start is called before the first frame update
    private float noiseOffsetX = 0f;
    private float noiseOffsetY = 0f;
    public Vector3 windDirection = Vector3.zero;

    [SerializeField] float noiseScaleX = .02f;
    [SerializeField] float noiseScaleY = .1f;

    void Update()
    {
        // multiply deltaTime by a scalar
        noiseOffsetX += Time.deltaTime * noiseScaleX;
        noiseOffsetY += Time.deltaTime * noiseScaleY;

        // generate perlin noise in two dimensions
        float noise = Mathf.PerlinNoise(noiseOffsetX, noiseOffsetY);
    
        // translate noise to an angle on a circle
        float angle = noise * Mathf.PI * 2f;

        // generate a wind direction vector
        windDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
        Debug.DrawRay(transform.position, windDirection * 200f, Color.blue);
    }
}
