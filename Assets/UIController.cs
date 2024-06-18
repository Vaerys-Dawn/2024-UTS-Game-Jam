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
    [SerializeField] private GameObject nextLevelScreen;


    private void Start()
    {
        nextLevelScreen.SetActive(false);
    }

    public override void InteractionUpdate()
    {
        if (UserInput.instance.MenuInput)
        {
            Debug.Log("Escape Attempt");
            Application.Quit();
        }

        if (UserInput.instance.MapInput)
        {
            Debug.Log("Toggle Fullscreen");
            Screen.fullScreen = !Screen.fullScreen;
        }

        if (timerStopped && UserInput.instance.JumpPressed)
        {
            timerStarted = false;
            timerStopped = false;
            overalTime = 0;
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            player.resetSpeed();
            link.SendState(this, true);
            nextLevelScreen.SetActive(false);

            //Time.timeScale = 1;
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
        nextLevelScreen.SetActive(true);
        //Time.timeScale = 0;
    }
}
