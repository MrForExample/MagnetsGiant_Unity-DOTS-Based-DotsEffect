using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace  MoreMountains.Feel
{
	public class Wheel : MonoBehaviour
	{
		[Header("Binding")]
		/// the part of the wheel that rotates
		[Tooltip("the part of the wheel that rotates")]
		public Transform RotatingPart;

		[Header("Settings")] 
		/// the speed at which the wheel should rotate
		[Tooltip("the speed at which the wheel should rotate")]
		public float RotationSpeed = 20f;

		[Header("Feedbacks")]
		/// a feedback to call when the wheel starts turning
		[Tooltip("a feedback to call when the wheel starts turning")]
		public MMFeedbacks TurnFeedback;
		/// a feedback to call when the wheel stops turning
		[Tooltip("a feedback to call when the wheel stops turning")]
		public MMFeedbacks TurnStopFeedback;
        
		protected bool _turning;

		/// <summary>
		/// On Update we look for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
			HandleWheel();
		}

		/// <summary>
		/// Detects input
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame())
			{
				Turn();
			}
			if (FeelDemosInputHelper.CheckMainActionInputUpThisFrame())
			{
				TurnStop();
			}
		}

		/// <summary>
		/// Every frame, rotates the wheel if needed
		/// </summary>
		protected virtual void HandleWheel()
		{
			if (_turning)
			{
				RotatingPart.transform.Rotate(this.transform.right, RotationSpeed * Time.deltaTime);
			}
		}

		/// <summary>
		/// Makes the wheel turn, plays a feedback if it's just starting to turn this frame
		/// </summary>
		protected virtual void Turn()
		{
			if (!_turning)
			{
				TurnFeedback?.PlayFeedbacks();    
			}
			_turning = true;
		}
        
		/// <summary>
		/// Stops the wheel from turning
		/// </summary>
		protected virtual void TurnStop()
		{
			TurnFeedback?.StopFeedbacks();
			TurnStopFeedback?.PlayFeedbacks();
			_turning = false;
		}
	}    
}