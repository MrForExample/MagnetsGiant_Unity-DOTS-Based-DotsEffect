using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A simple class used to handle enemies in Feel's Barbarian demo scene
	/// </summary>
	public class BarbarianEnemy : MonoBehaviour
	{
		/// a feedback to play when getting damage 
		public MMFeedbacks DamageFeedback;
		/// a cooldown, in seconds, during which the character can't be damaged
		public float DamageCooldown = 1f;
        
		protected float _lastDamageTakenAt = -10f;
        
		/// <summary>
		/// Applies damage to this character, if not in cooldown
		/// </summary>
		/// <param name="damage"></param>
		public virtual void TakeDamage(int damage)
		{
			// we make sure enough time has passed since the last time this enemy took damage
			if (Time.time - _lastDamageTakenAt < DamageCooldown)
			{
				return;
			}
			_lastDamageTakenAt = Time.time;
			DamageFeedback?.PlayFeedbacks(this.transform.position, damage);
		}
	}
}