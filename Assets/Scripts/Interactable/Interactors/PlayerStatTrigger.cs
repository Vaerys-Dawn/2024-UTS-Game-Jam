using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatTrigger : Interactable
{

    [SerializeField] private Utility.PowerUpType powerUpType;
    private PlayerMovement move;

    public bool getValue() => powerUpType switch
    {
        Utility.PowerUpType.ATTACK => move._attackUnlocked,
        Utility.PowerUpType.DASH => move._dashUnlocked,
        Utility.PowerUpType.SLIDE => move._slideUnlocked,
        Utility.PowerUpType.WALL_JUMP => move._wallJumpUnlocked,
        Utility.PowerUpType.WALL_CLIMB => move._wallClimbUnlocked,
        Utility.PowerUpType.WALL_HOLD => move._wallHoldUnlocked,
        _ => false
    };

    public override void InteractionUpdate()
    {
        if (move == null) move = FindObjectOfType<PlayerMovement>();
        receiver.SendContinuousUpdate(this, getValue());
    }
}
