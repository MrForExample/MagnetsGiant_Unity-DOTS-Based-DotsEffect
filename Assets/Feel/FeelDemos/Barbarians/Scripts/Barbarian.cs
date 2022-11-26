using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A simple class controlling the hero character in the Barbarians demo scene
	/// It simulates a very simple character controller in an ARPG game
	/// </summary>
	public class Barbarian : MonoBehaviour
	{
		[Header("Cooldown")]
		/// a duration, in seconds, between two attacks, during which attacks are prevented
		[Tooltip("a duration, in seconds, between two attacks, during which attacks are prevented")]
		public float CooldownDuration = 0.1f;

		[Header("Feedbacks")]
		/// a feedback to call when the attack starts
		[Tooltip("a feedback to call when the attack starts")]
		public MMFeedbacks AttackFeedback;
		/// a feedback to call when each individual attack phase starts
		[Tooltip("a feedback to call when each individual attack phase starts")]
		public MMFeedbacks IndividualAttackFeedback;
		/// a feedback to call when trying to attack while in cooldown
		[Tooltip("a feedback to call when trying to attack while in cooldown")]
		public MMFeedbacks DeniedFeedback;

		[Header("Attack settings")]
		/// a curve on which to move the character when it attacks
		public MMTween.MMTweenCurve AttackCurve = MMTween.MMTweenCurve.EaseInOutOverhead;
		/// the duration of the attack in seconds
		public float AttackDuration = 2.5f;
		/// an offset at which to attack enemies
		public float AttackPositionOffset = 0.3f;
		/// a duration by which to reduce movement duration after every attack (making each attack faster than the previous one)
		public float IntervalDecrement = 0.1f;

		protected List<Vector3> _targets;
		protected float _lastAttackStartedAt = -100f;
		protected Vector3 _initialPosition;
		protected Vector3 _initialLookAtTarget;
		protected Vector3 _lookAtTarget;
		protected BarbarianEnemy _enemy;

		/// <summary>
		/// On Awake we store our initial position
		/// </summary>
		protected virtual void Awake()
		{
			_initialPosition = this.transform.position;
			_initialLookAtTarget = this.transform.position + this.transform.forward * 10f;
			_lookAtTarget = _initialLookAtTarget;
		}

		/// <summary>
		/// On Update we look for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
			LookAtTarget();
		}

		/// <summary>
		/// Makes the character look at the target it's attacking
		/// </summary>
		protected virtual void LookAtTarget()
		{
			Vector3 direction = _lookAtTarget - _initialPosition;
			this.transform.LookAt(_lookAtTarget + direction * 5f);
		}

		/// <summary>
		/// Detects input
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame())
			{
				Attack();
			}
		}

		/// <summary>
		/// Performs an attack if possible, otherwise plays a denied feedback
		/// </summary>
		protected virtual void Attack()
		{
			if (Time.time - _lastAttackStartedAt < CooldownDuration + AttackDuration)
			{
				DeniedFeedback?.PlayFeedbacks();
			}
			else
			{
				AcquireTargets();
				StartCoroutine(AttackCoroutine());
				_lastAttackStartedAt = Time.time;
			}
		}

		/// <summary>
		/// Finds targets around the Barbarian and stores them
		/// </summary>
		protected virtual void AcquireTargets()
		{
			_targets = new List<Vector3>();

			Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 5f);
			foreach (var hitCollider in hitColliders)
			{
				Vector3 enemyPosition = hitCollider.transform.position;
				Vector3 direction = this.transform.position - enemyPosition;

				if (hitCollider.GetComponent<BarbarianEnemy>() != null)
				{
					_targets.Add(enemyPosition + direction * AttackPositionOffset);
				}               
			}
			_targets.MMShuffle();
		}
        
		/// <summary>
		/// A coroutine used to move to each stored target to attack them
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AttackCoroutine()
		{
			float intervalDuration = AttackDuration / _targets.Count;
            
			// we play our initial attack feedback
			AttackFeedback?.PlayFeedbacks();

			int enemyCounter = 0;

			foreach (Vector3 destination in _targets)
			{
				// for each new enemy, we play an attack feedback
				IndividualAttackFeedback?.PlayFeedbacks();
				MMTween.MoveTransform(this, this.transform, this.transform.position, destination, null, 0f, intervalDuration, AttackCurve);
				_lookAtTarget = destination;                
				yield return MMCoroutine.WaitFor(intervalDuration - enemyCounter * IntervalDecrement);
				enemyCounter++;
			}

			MMTween.MoveTransform(this, this.transform, this.transform.position, _initialPosition, null, 0f, intervalDuration, AttackCurve);
			_lookAtTarget = _initialLookAtTarget;
		}

		/// <summary>
		/// When we collide with an enemy, we apply damage to it
		/// </summary>
		/// <param name="other"></param>
		protected virtual void OnTriggerEnter(Collider other)
		{
			_enemy = other.GetComponent<BarbarianEnemy>();
			if (_enemy != null)
			{
				// we randomize the damage done and apply it to the enemy
				int damage = Random.Range(50, 250);
				_enemy.TakeDamage(damage);
			}
		}
	}
}