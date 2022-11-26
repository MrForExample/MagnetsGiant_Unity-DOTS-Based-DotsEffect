using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using MoreMountains.Feel;

/// <summary>
/// A very simple class used to make a character jump, designed to be used in Feel's Getting Started tutorial
/// Yes the name is different from the one in the tutorial, it's to avoid conflicts if you were to name it exactly the same.
/// </summary>
public class GettingStartedTutorialHeroReference : MonoBehaviour
{
	[Header("Hero Settings")]
	/// a key the Player has to press to make our Hero jump
	public KeyCode ActionKey = KeyCode.Space;
	/// the force to apply vertically to the Hero's rigidbody to make it jump up
	public float JumpForce = 8f;

	[Header("Feedbacks")]
	/// a MMFeedbacks to play when the Hero starts jumping
	public MMFeedbacks JumpFeedback;
	/// a MMFeedbacks to play when the Hero lands after a jump
	public MMFeedbacks LandingFeedback;

	[Header("Events")]
	/// a UnityEvent to fire when jumping
	public UnityEvent OnJump;
	/// a UnityEvent to fire when landing
	public UnityEvent OnLand;

	private const float _lowVelocity = 0.1f;
	private Rigidbody _rigidbody;
	private float _velocityLastFrame;
	private bool _jumping = false;
    
	/// <summary>
	/// On Awake we store our Rigidbody and force gravity to -30 on the y axis so that jumps feel better
	/// </summary>
	private void Awake()
	{
		_rigidbody = this.gameObject.GetComponent<Rigidbody>();
		Physics.gravity = Vector3.down * 30;
	}
    
	/// <summary>
	/// Every frame
	/// </summary>
	private void Update()
	{
		// we check if the Player has pressed our action key, and trigger a jump if that's the case
		if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame() && !_jumping)
		{
			Jump();
		}

		// if we're jumping, were going down last frame, and have now reached an almost null velocity
		if (_jumping && (_velocityLastFrame < 0) && (Mathf.Abs(_rigidbody.velocity.y) < _lowVelocity))
		{
			// then we just landed, we reset our state
			_jumping = false;
			LandingFeedback?.PlayFeedbacks();

			if (OnLand != null)
			{
				OnLand.Invoke();
			}
		}
        
		// we store our velocity
		_velocityLastFrame = _rigidbody.velocity.y;
	}

	/// <summary>
	/// Makes our hero jump in the air
	/// </summary>
	private void Jump()
	{
		_rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
		_jumping = true;
		JumpFeedback?.PlayFeedbacks();

		if (OnJump != null)
		{
			OnJump.Invoke();
		}
	}
}