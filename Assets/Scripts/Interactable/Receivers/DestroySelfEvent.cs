using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfEvent : InteractionReceiver
{
    
    public override void InteractionUpdate()
    {
        // ignore
    }

    public override void RecieveStateChange(AbstractInteractor source, bool currentState, InteractionType lastInteractionType)
    {
        if (currentState) Destroy(this.gameObject);
    }

}
