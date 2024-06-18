using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timer;

    float overalTime = 0;
    private bool timerStarted;
    private float intialTime;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ((UserInput.instance.MoveInput.x != 0 || UserInput.instance.SprintHeld) && !timerStarted)
        {
            timerStarted = true;
            intialTime = Time.time;
        }
        if (timerStarted)
        {
            overalTime = Time.time - intialTime;
        }
        TimeSpan span = TimeSpan.FromSeconds(overalTime);
        timer.SetText(String.Format("{0:00}:{1:00}:{2:000}",span.Minutes, span.Seconds, span.Milliseconds )); 
    }
}
