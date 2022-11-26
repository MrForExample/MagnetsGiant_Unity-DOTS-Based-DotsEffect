using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feel
{
	public class FeelBrass : MonoBehaviour
	{
		[Header("Bindings")]
		/// a reference to the MMAudioAnalyzer in the scene
		public MMAudioAnalyzer TargetAnalyzer;
		/// a light we want to control based on the current level of the music
		public Light TargetLight;

		[Header("Cooldown")]
		/// a duration, in seconds, between two special dance moves, during which moves are prevented
		[Tooltip("a duration, in seconds, between two special dance moves, during which moves are prevented")]
		public float CooldownDuration = 0.1f;

		[Header("Feedbacks")]
		/// a feedback to play when doing a special dance move
		[Tooltip("a feedback to play when doing a special dance move")]
		public MMFeedbacks SpecialDanceMoveFeedbacks;
        
		protected float _lastMoveStartedAt = -100f;
        
		/// <summary>
		/// On Update we look for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
			ControlLightIntensity();
		}

		/// <summary>
		/// Detects input
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame())
			{
				SpecialDanceMove();
			}
		}

		/// <summary>
		/// Updates the light's intensity in real time based on the music's levels
		/// </summary>
		protected virtual void ControlLightIntensity()
		{
			// this simple line lets us grab the current normalized & buffered amplitude of the music, and feed it to the light
			TargetLight.intensity = TargetAnalyzer.NormalizedBufferedAmplitude * 5f;
		}

		/// <summary>
		/// Performs a move if possible, otherwise plays a denied feedback
		/// </summary>
		protected virtual void SpecialDanceMove()
		{
			if (Time.time - _lastMoveStartedAt >= CooldownDuration)
			{
				SpecialDanceMoveFeedbacks?.PlayFeedbacks();
				_lastMoveStartedAt = Time.time;
			}
		}
	}    
}