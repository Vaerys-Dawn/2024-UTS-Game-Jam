using UnityEngine;

public abstract class Interactable : AbstractInteractor
{
    public AbstractInteractor receiver;

    public override void RecieveStateChange(AbstractInteractor source, bool currentState, InteractionType lastInteractionType)
    {
        // ignored
    }

    public override void RecieveInteractionUpdate(AbstractInteractor source, InteractionType type)
    {
        // ignored
    }

    private void OnDrawGizmos()
    {
        if (receiver != null) { Gizmos.DrawLine(this.transform.position, receiver.transform.position); }
        
    }
}
