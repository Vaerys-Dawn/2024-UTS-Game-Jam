using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionReceiver : AbstractInteractor
{

    public override void RecieveInteractionUpdate(AbstractInteractor source, InteractionType type)
    {
        // ignored
    }
}
