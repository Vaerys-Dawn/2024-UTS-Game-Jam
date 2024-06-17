using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpEvent : InteractionReceiver
{

    [SerializeField] private Utility.PowerUpType powerUpType;
    [SerializeField] private bool enabling = true;
    
    public override void InteractionUpdate()
    {
        // ignore
    }

    public override void RecieveStateChange(AbstractInteractor source, bool currentState, InteractionType lastInteractionType)
    {
        if (!currentState) return;
        PlayerMovement move = FindObjectOfType<PlayerMovement>();
        if (move)
        {
           // AudioManager.Instance.PlaySound("Ability_Pickup");
            switch (powerUpType)
            {
                case Utility.PowerUpType.ATTACK:
                    move._attackUnlocked = enabled;
                    break;
                case Utility.PowerUpType.DASH: 
                    move._dashUnlocked = enabling;
                    break;
                case Utility.PowerUpType.SLIDE:
                    move._slideUnlocked = enabling;
                    break;
                case Utility.PowerUpType.WALL_JUMP:
                    move._wallJumpUnlocked = enabling;
                    break;
                case Utility.PowerUpType.WALL_CLIMB:
                    move._wallClimbUnlocked = enabling;
                    break;
                case Utility.PowerUpType.WALL_HOLD:
                    move._wallHoldUnlocked = enabling;
                    break;
                default:
                    Debug.Log("HOw???");
                    break;
            }
        }
        Destroy(this.gameObject);
    }
}
