using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPlayerEvent : InteractionReceiver
{
    [SerializeField] private bool continuousHeal;
    [SerializeField] private int healAmount;
    [SerializeField] private float healCooldown;
    [SerializeField] private int maxHealing = -1;
    [SerializeField] private bool destroyAfterHeal = true;

    private float healTime = 0;
    private int healCount = 0;

    public override void InteractionUpdate()
    {
        healTime -= Time.deltaTime;
        if (!continuousHeal || healTime > 0) return;
        Heal();
        healTime = healCooldown;
        if (destroyAfterHeal && healCount >= maxHealing) Destroy(this.gameObject);
    }

    private void Heal()
    {
        PlayerMovement movement = FindObjectOfType<PlayerMovement>();
        if (movement == null) return;
        int temp = healAmount;
        if (maxHealing != -1) // if max healing = -1 ignore heal cap
        {
            // don't heal over healing stored
            int healRemaining = maxHealing - healCount;
            if (healAmount > healRemaining) temp = healRemaining;
            // don't overheal
            int playerTemp = movement.CurrentHealth + temp;
            if (playerTemp > movement._maxHealth) temp = movement._maxHealth - movement.CurrentHealth;
        }
        healCount += temp;
        movement.Heal(temp);
    }

    public override void RecieveStateChange(AbstractInteractor source, bool currentState, InteractionType lastInteractionType)
    {
        if (!currentState) return;
        Debug.Log("Stuff");

        if (continuousHeal || healTime > 0) return;
        Heal();
        healTime = healCooldown;
        if (destroyAfterHeal) Destroy(this.gameObject);
    }
}
