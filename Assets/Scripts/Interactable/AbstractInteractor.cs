using System;
using System.Collections;
using UnityEngine;

// acts as both an interaction source and an interaction receiver
public abstract class AbstractInteractor : MonoBehaviour
{
    public bool LastPowerState { get; private set; }
    public bool PowerState { get; private set; }

    public bool ImpulseActive { get; private set; }

    public enum InteractionType {
        IMPULSE,
        TOGGLE,
        CONTINUOUS,
        STATE_UPDATE
    }

    public AbstractInteractor LinkedInteractor { get; private set; } = null;

    public AbstractInteractor LastInteractor { get; private set; }

    public InteractionType LastInteractionType { get; private set; }

    private void Update()
    {
        // Linked Receiver Overrides State updates
        if (LinkedInteractor)
        {
            PowerState = LinkedInteractor.PowerState;
            LastInteractor = LinkedInteractor.LastInteractor;
            LastInteractionType = LinkedInteractor.LastInteractionType;
            if (LastPowerState != PowerState) RecieveStateChange(LastInteractor, PowerState, LastInteractionType);
            LastPowerState = PowerState;
            return;
        }
        
        // continuously turn on state when impulse or Continiuous is activae
        if (ImpulseActive) PowerState = true;

        // Send State change update
        if (LastPowerState != PowerState) RecieveStateChange(LastInteractor, PowerState, LastInteractionType);
        // Save last State
        LastPowerState = PowerState;
        InteractionUpdate();
    }

    /// <summary>
    ///     Sent Whenever PowerState is changed
    /// </summary>
    /// <param name="source">The source of the PowerState change.</param>
    /// <param name="currentState">The current value of PowerState.</param>
    /// <param name="lastInteractionType">The type of interactor that initiated the state change.</param>
    public abstract void RecieveStateChange(AbstractInteractor source, bool currentState, InteractionType lastInteractionType);

    /// <summary>
    ///     Activated after any Interaction Update
    /// </summary>
    /// <param name="source">The Source of the Interaction Update</param>
    /// <param name="type">The Type of Interaction Update performed.</param>
    public abstract void RecieveInteractionUpdate(AbstractInteractor source, InteractionType type);

    /// <summary>
    ///     Updates after PowerState is processed.
    /// </summary>
    public abstract void InteractionUpdate();

    public void SendImpulse(AbstractInteractor source, float time)
    {
        ImpulseActive = true;
        RecieveInteractionUpdate(source, InteractionType.IMPULSE);
        StartCoroutine(nameof(InpulseWait), new Tuple<AbstractInteractor, float>(source,time));
    }

    private IEnumerator InpulseWait(Tuple<Interactable, float> args)
    {
        yield return new WaitForSeconds(args.Item2);
        ImpulseActive = false;
        RecieveInteractionUpdate(args.Item1, InteractionType.IMPULSE);
    }

    public void SendContinuousUpdate(AbstractInteractor source, bool state)
    {
        LastInteractor = source;
        LastInteractionType = InteractionType.CONTINUOUS;
        PowerState = state;
        RecieveInteractionUpdate(source, InteractionType.CONTINUOUS);
    }

    public void SendState(AbstractInteractor source, bool state)
    {
        PowerState = state;
        LastInteractionType = InteractionType.STATE_UPDATE;
        LastInteractor = source;
        RecieveInteractionUpdate(source, InteractionType.STATE_UPDATE);
    }

    public bool SendToggle(AbstractInteractor source)
    {
        PowerState = !PowerState;
        LastInteractionType = InteractionType.TOGGLE;
        LastInteractor = source;
        RecieveInteractionUpdate(source, InteractionType.TOGGLE);
        return PowerState;
    }

}