using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : Interactable
{

    [SerializeField] TextMeshProUGUI timer;

    float overalTime = 0;
    private bool timerStarted;
    private float intialTime;
    private bool timerStopped;

    LevelLink link = null;

    public override void InteractionUpdate()
    {
        if (timerStopped && UserInput.instance.JumpPressed)
        {
            link.SendState(this, true);
        }

        if (timerStopped) return;
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
        timer.SetText(String.Format("{0:00}:{1:00}:{2:000}", span.Minutes, span.Seconds, span.Milliseconds));
    }

    internal void goalFlag(GameObject endFlag)
    {
        link = endFlag.GetComponent<LevelLink>();
    }

    internal void stopTimer()
    {
        timerStopped = true;
    }
}
