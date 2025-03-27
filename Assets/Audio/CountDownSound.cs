using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDownSound : MonoBehaviour
{
    // AUDIO sources for countdown timer
    // these are triggered via animation events in the countdown animator
    [SerializeField] AudioSource three;
    [SerializeField] AudioSource two;
    [SerializeField] AudioSource one;
    [SerializeField] AudioSource go;

    public void PlayThree()
    {
        // play voice "three"
        three.Play();
    }

    public void PlayTwo()
    {
        // play voice "two"
        two.Play();
    }

    public void PlayOne()
    {
        // play voice "one"
        one.Play();
    }

    public void PlayGo()
    {
        // play voice "go"
        go.Play();
    }

}
