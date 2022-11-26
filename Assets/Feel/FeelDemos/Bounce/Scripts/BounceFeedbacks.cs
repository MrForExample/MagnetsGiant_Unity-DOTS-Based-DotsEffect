using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MoreMountains.Feel
{
	/// <summary>
	/// A simple class used in Feel's Bounce demo scene.
	/// It's meant to be piloted by an animator, that calls animator events at certain points of its "cube jumps" animation 
	/// </summary>
	public class BounceFeedbacks : MonoBehaviour
	{
		/// a feedback to be played when the cube starts "charging"
		public MMFeedbacks ChargeFeedbacks;
		/// a feedback to be played when the jump happens
		public MMFeedbacks JumpFeedbacks;
		/// a feedback to be played when the cube lands
		public MMFeedbacks LandingFeedbacks;

		/// <summary>
		/// Called via an animator event when the charge starts
		/// </summary>
		public virtual void PlayCharge()
		{
			ChargeFeedbacks?.PlayFeedbacks();
		}
        
		/// <summary>
		/// Called via an animator event when the cube jumps
		/// </summary>
		public virtual void PlayJump()
		{
			JumpFeedbacks?.PlayFeedbacks();
		}
        
		/// <summary>
		/// Called via an animator event when the cube lands
		/// </summary>
		public virtual void PlayLanding()
		{
			LandingFeedbacks?.PlayFeedbacks();
		}
	}
}