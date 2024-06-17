using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damagable /*this is misspelt and it's driving me insane lol*/: MonoBehaviour
{
	[SerializeField] protected int maxHealth;
	[SerializeField] protected int currentHealth;//why private if this script isn't modifying it?

	public void DamageDealt(int damage, GameObject from, Vector2? force = null)
	{
		Vector2 actualForce = force == null ? Vector2.zero : (Vector2)force;
		ReceiveDamage(damage, from, actualForce);
		if (currentHealth <= 0)
		{
			ProcessDeath(from);
		}
	}



	protected abstract void ReceiveDamage(int damage, GameObject from, Vector2 force);//should be protected as only inheriting classes should call

	protected abstract void ProcessDeath(GameObject from);
}
