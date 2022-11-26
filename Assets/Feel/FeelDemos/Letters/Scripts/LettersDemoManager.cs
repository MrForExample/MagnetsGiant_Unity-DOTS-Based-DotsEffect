using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A manager used to pilot Feel's Letters demo scene
	/// It detects input, and plays corresponding feedbacks when needed
	/// </summary>
	public class LettersDemoManager : MonoBehaviour
	{
		[Header("Feedbacks")]
		/// a feedback to play when the F letter gets activated
		public MMFeedbacks FeedbackF;
		/// a feedback to play when the first E letter gets activated
		public MMFeedbacks FeedbackE1;
		/// a feedback to play when the second E letter gets activated
		public MMFeedbacks FeedbackE2;
		/// a feedback to play when the L letter gets activated
		public MMFeedbacks FeedbackL;

		protected Vector3 _mousePosition;

		/// <summary>
		/// On Update we look for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// Every frame, looks for input, and activates the corresponding letter if needed
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckAlphaInputPressedThisFrame(1))
			{
				PlayF();
			}
			if (FeelDemosInputHelper.CheckAlphaInputPressedThisFrame(2))
			{
				PlayE1();
			}
			if (FeelDemosInputHelper.CheckAlphaInputPressedThisFrame(3))
			{
				PlayE2();
			}
			if (FeelDemosInputHelper.CheckAlphaInputPressedThisFrame(4))
			{
				PlayL();
			}
            
			if (FeelDemosInputHelper.CheckMouseDown())
			{ 
				RaycastHit hit; 
				Ray ray = Camera.main.ScreenPointToRay(FeelDemosInputHelper.MousePosition()); 
				if ( Physics.Raycast (ray,out hit,100.0f)) 
				{
					switch (hit.transform.name)
					{
						case "ColliderF":
							PlayF();
							break;
						case "ColliderE1":
							PlayE1();
							break;
						case "ColliderE2":
							PlayE2();
							break;
						case "ColliderL":
							PlayL();
							break;
					}
				}
			}
		}

		/// <summary>
		/// Activates the letter F
		/// </summary>
		protected virtual void PlayF()
		{
			FeedbackF?.PlayFeedbacks();
		}

		/// <summary>
		/// Activates the first E letter
		/// </summary>
		protected virtual void PlayE1()
		{
			FeedbackE1?.PlayFeedbacks();
		}
        
		/// <summary>
		/// Activates the second E letter
		/// </summary>
		protected virtual void PlayE2()
		{
			FeedbackE2?.PlayFeedbacks();
		}
        
		/// <summary>
		/// Activates the letter L
		/// </summary>
		protected virtual void PlayL()
		{
			FeedbackL?.PlayFeedbacks();
		}
	}
}