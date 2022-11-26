using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// An example class part of the Feel demos
	/// This class acts as a character controller for the Duck in the FeelDuck demo scene
	/// It looks for input, and jumps when instructed to
	/// </summary>
	public class Duck : MonoBehaviour
	{
		[Header("Cooldown")]
		/// a duration, in seconds, between two jumps, during which jumps are prevented
		[Tooltip("a duration, in seconds, between two jumps, during which jumps are prevented")]
		public float CooldownDuration = 1f;

		[Header("Feedbacks")]
		/// a feedback to call when jumping
		[Tooltip("a feedback to call when jumping")]
		public MMFeedbacks JumpFeedback;
		/// a feedback to call when landing
		[Tooltip("a feedback to call when landing")]
		public MMFeedbacks LandingFeedback;
		/// a feedback to call when trying to jump while in cooldown
		[Tooltip("a feedback to call when trying to jump while in cooldown")]
		public MMFeedbacks DeniedFeedback;

		protected float _lastJumpStartedAt = -100f;

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
				Jump();
			}
		}

		/// <summary>
		/// Performs a jump if possible, otherwise plays a denied feedback
		/// </summary>
		protected virtual void Jump()
		{
			if (Time.time - _lastJumpStartedAt < CooldownDuration)
			{
				DeniedFeedback?.PlayFeedbacks();
			}
			else
			{
				JumpFeedback?.PlayFeedbacks();
				_lastJumpStartedAt = Time.time;
			}            
		}

		/// <summary>
		/// This method is called by the duck animator on the frame where it makes contact with the ground.
		/// In an actual game context, this may be called when you detect contact with the ground via a physics collision, a downward raycast, etc.
		/// </summary>
		public virtual void Land()
		{
			LandingFeedback?.PlayFeedbacks();
		}
	}
}