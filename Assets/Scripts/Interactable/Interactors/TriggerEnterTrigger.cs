using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnterTrigger : Interactable
{

    [SerializeField] public bool resetOnLeave = false;
    private bool triggered = false;
    public override void InteractionUpdate()
    {
        // ignore
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        PlayerMovement movement = collision.GetComponent<PlayerMovement>();
        if (movement && !triggered)
        {
            receiver.SendState(this, true);
            triggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement movement = collision.GetComponent<PlayerMovement>();
        if (movement)
        {
            triggered = false;
            receiver.SendState(this, false);
        }
    }
}
