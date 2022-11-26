using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// This class handles Feel's MMSequencer demo scene, detecting input and starting/stopping its target sequencer 
	/// </summary>
	public class MMSequencerDemoManager : MonoBehaviour
	{
		[Header("Sequence")]
		/// the feedback sequencer to pilot when pressing the ActionKey
		public MMFeedbacksSequencer TargetSequencer;
        
		/// <summary>
		/// On Update we detect input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// Every frame we check if input was pressed
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame())
			{
				Toggle();
			}
		}

		/// <summary>
		/// Turns the sequencer on or off based on its current state
		/// </summary>
		protected virtual void Toggle()
		{
			if (TargetSequencer.Playing)
			{
				TargetSequencer.StopSequence();
			}
			else
			{
				TargetSequencer.PlaySequence();
			}
		}
	}
}