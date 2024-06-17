using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDirectionTrigger : Interactable
{
    bool sent = false;
    PlayerMovement movement;

    public override void InteractionUpdate()
    {
        if (receiver == null) return;
        if (movement && UserInput.instance.InteractInput && !sent)
        {
            receiver.SendState(this, true);
            sent = true;
        } else if (!UserInput.instance.InteractInput)
        {
            receiver.SendState(this, false);
            sent = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerMovement mov = collision.GetComponent<PlayerMovement>();
        if (mov) movement = mov;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement mov = collision.GetComponent<PlayerMovement>();
        if (mov) movement = null;
    }

}
