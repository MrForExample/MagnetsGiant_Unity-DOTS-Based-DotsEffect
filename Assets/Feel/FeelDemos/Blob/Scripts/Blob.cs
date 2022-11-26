using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A simple class used to pilot Feel's Blob demo character, who simply moves on a loop when its target key is pressed
	/// </summary>
	public class Blob : MonoBehaviour
	{
		[Header("Cooldown")]
		/// a duration, in seconds, between two moves, during which moves are prevented
		[Tooltip("a duration, in seconds, between two moves, during which moves are prevented")]
		public float CooldownDuration = 1f;

		[Header("Feedbacks")]
		/// a feedback to call when moving
		[Tooltip("a feedback to call when moving")]
		public MMFeedbacks MoveFeedback;
		/// a feedback to call when trying to move while in cooldown
		[Tooltip("a feedback to call when trying to move while in cooldown")]
		public MMFeedbacks DeniedFeedback;

		protected float _lastMoveStartedAt = -100f;

		/// <summary>
		/// On Update we look for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// Detects input
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame())
			{
				Move();
			}
		}

		/// <summary>
		/// Performs a move if possible, otherwise plays a denied feedback
		/// </summary>
		protected virtual void Move()
		{
			if (Time.time - _lastMoveStartedAt < CooldownDuration)
			{
				DeniedFeedback?.PlayFeedbacks();
			}
			else
			{
				MoveFeedback?.PlayFeedbacks();
				_lastMoveStartedAt = Time.time;
			}
		}
	}
}