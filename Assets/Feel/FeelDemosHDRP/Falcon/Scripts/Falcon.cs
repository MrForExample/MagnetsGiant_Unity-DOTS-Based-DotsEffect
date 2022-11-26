using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace  MoreMountains.Feel
{
	public class Falcon : MonoBehaviour
	{
		[Header("Input")]
		/// a key to use to jump
		[Tooltip("a key to use to jump")]
		public KeyCode ActionKey = KeyCode.Space;
		/// a secondary key to use to jump
		[Tooltip("a secondary key to use to jump")]
		public KeyCode ActionKeyAlt = KeyCode.Joystick1Button0;

		[Header("Bindings")]
		/// the various wigglers that make the car move
		[Tooltip("the various wigglers that make the car move")]
		public List<MMWiggle> Wigglers;
		/// the wiggler associated to the camera
		[Tooltip("the wiggler associated to the camera")]
		public MMWiggle CameraWiggler;
		/// the ground's panning texture
		[Tooltip("the ground's panning texture")]
		public MMPanningTexture Offsetter;
		/// the particles that are supposed to loop (rocks etc)
		[Tooltip("the particles that are supposed to loop (rocks etc)")]
		public List<ParticleSystem> ParticleLoops;
		/// the on/off emitters (wind, smoke)
		[Tooltip("the on/off emitters (wind, smoke)")]
		public List<ParticleSystem> ParticleEmitters;
		/// the wheels' auto rotators
		[Tooltip("the wheels' auto rotators")]
		public List<MMAutoRotate> AutoRotaters;

		[Header("Settings")] 
		/// the speed at which the wheel should rotate
		[Tooltip("the speed at which the wheel should rotate")]
		public float RotationSpeed = 20f;

		[Header("Feedbacks")]
		/// a feedback to call when the car starts driving
		[Tooltip("a feedback to call when the car starts driving")]
		public MMFeedbacks DriveFeedback;
		/// a feedback to call when the car stops
		[Tooltip("a feedback to call when the car stops")]
		public MMFeedbacks StopFeedback;
        
		protected bool _turning;

		/// <summary>
		/// On Start, we turn the car off
		/// </summary>
		protected virtual void Start()
		{
			SetCar(false);
		}

		/// <summary>
		/// Turns the car on or off depending on the status passed in parameters
		/// </summary>
		/// <param name="status"></param>
		protected virtual void SetCar(bool status)
		{
			foreach (MMWiggle wiggler in Wigglers)
			{
				wiggler.PositionActive = status;
			}
			foreach (ParticleSystem system in ParticleEmitters)
			{
				if (status)
				{
					system.Play();
				}
				else
				{
					system.Stop();
				}
			}
			foreach (ParticleSystem system in ParticleLoops)
			{
				if (status)
				{
					system.Play();
				}
				else
				{
					system.Pause();
				}
			}
			foreach (MMAutoRotate rotater in AutoRotaters)
			{
				rotater.Rotating = status;
			}

			Offsetter.TextureShouldPan = status;

			CameraWiggler.PositionActive = status;
			CameraWiggler.RotationActive = status;
		}

		/// <summary>
		/// On Update we look for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
			HandleCar();
		}

		/// <summary>
		/// Detects input
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressed())
			{
				Drive();
			}
			if (FeelDemosInputHelper.CheckMainActionInputUpThisFrame())
			{
				TurnStop();
			}
		}

		/// <summary>
		/// Every frame, rotates the wheel if needed
		/// </summary>
		protected virtual void HandleCar()
		{
			if (_turning)
			{
				//RotatingPart.transform.Rotate(this.transform.right, RotationSpeed * Time.deltaTime);
			}
		}

		/// <summary>
		/// Makes the car drive, plays a feedback if it's just starting to turn this frame
		/// </summary>
		protected virtual void Drive()
		{
			if (!_turning)
			{
				DriveFeedback?.PlayFeedbacks();
				SetCar(true);
			}
			_turning = true;
		}
        
		/// <summary>
		/// Stops the car
		/// </summary>
		protected virtual void TurnStop()
		{
			DriveFeedback?.StopFeedbacks();
			StopFeedback?.PlayFeedbacks();
			SetCar(false);
			_turning = false;
		}
	}    
}