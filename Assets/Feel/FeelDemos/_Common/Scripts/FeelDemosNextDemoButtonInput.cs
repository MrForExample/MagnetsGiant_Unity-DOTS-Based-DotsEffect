using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// This component checks whether the user pressed Enter and plays the associated feedback if that's the case
	/// </summary>
	public class FeelDemosNextDemoButtonInput : MonoBehaviour
	{
		public MMFeedbacks OnInputFeedback;

		protected virtual void Update()
		{
			if (FeelDemosInputHelper.CheckEnterPressedThisFrame())
			{
				OnInputFeedback?.PlayFeedbacks();
			}
		}
	}	
}