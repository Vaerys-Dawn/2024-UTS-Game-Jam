using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndFlag : MonoBehaviour
{
    // Start is called before the first frame update
    public UIController controller;

    void Start()
    {
        controller = FindObjectOfType<UIController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == null)
        {
            controller = FindObjectOfType<UIController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player")) {
            controller.stopTimer();
            controller.goalFlag(this.gameObject);
        }
    }
}
