using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A class used to handle Feel's Snake demo's snake "head", the part controlled by the player
	/// </summary>
	public class Snake : MonoBehaviour
	{
		[Header("Movement")]
		/// the snake's movement speed
		public float Speed = 5f;
		/// the speed multiplier to apply at most times
		public float NormalSpeedMultiplier = 1f;
		/// the rate at which speed should vary
		public float SpeedChangeRate = 0.2f;
		/// the current direction of the snake
		public Vector3 Direction = Vector2.right;

		[Header("Boost")] 
		/// the speed multiplier to apply to the speed when boosting
		public float BoostMultiplier = 2f;
		/// the duration of the boost, in seconds
		public float BoostDuration = 2f;

		[Header("BodyParts")] 
		/// the prefab to use for body parts
		public SnakeBodyPart BodyPartPrefab;
		/// the offset to apply between two parts
		public int BodyPartsOffset = 7;
		/// the maximum amount of body parts for this snake
		public int MaxAmountOfBodyParts = 10;
		/// the minimum duration, in seconds, between 2 allowed parts losses
		public float MinTimeBetweenLostParts = 2f;

		[Header("Bindings")] 
		/// a Text component on which to display our current score
		public Text PointsCounter;

		[Header("Feedbacks")] 
		/// a feedback to play when the snake turns
		public MMFeedbacks TurnFeedback;
		/// a feedback to play when the snake teleports to the other side of the screen
		public MMFeedbacks TeleportFeedback;
		/// a feedback to play when teleporting once
		public MMFeedbacks TeleportOnceFeedback;
		/// a feedback to play when eating snake food
		public MMFeedbacks EatFeedback;
		/// a feedback to play when losing a body part
		public MMFeedbacks LoseFeedback;
        
		[Header("Debug")]
		[MMReadOnly]
		public int SnakePoints = 0;
		[MMReadOnly]
		public float _speed;
		[MMReadOnly]
		public float _speedMultiplier;
		[MMReadOnly]
		public float _lastFoodEatenAt = -100f;
        
		protected Vector3 _newPosition;
		protected MMPositionRecorder _recorder;
		public List<SnakeBodyPart> _snakeBodyParts;
		protected float _lastLostPart = 0f;
        
		/// <summary>
		/// On Awake, we initialize our snake's points, speed, position recorder, and body parts container
		/// </summary>
		protected void Awake()
		{
			_speed = Speed;
			SnakePoints = 0;
			_recorder = this.gameObject.GetComponent<MMPositionRecorder>();
			PointsCounter.text = "0";
			_snakeBodyParts = new List<SnakeBodyPart>();
		}
        
		/// <summary>
		/// Every frame, we check for input and move our snake's head
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();   
			HandleMovement();
		}

		/// <summary>
		/// Every frame, looks for turn input
		/// </summary>
		protected virtual void HandleInput()
		{
			if (FeelDemosInputHelper.CheckMainActionInputPressedThisFrame())
			{
				Turn();
			}    
		}

		/// <summary>
		/// Every frame, moves the snake's head's position
		/// </summary>
		protected virtual void HandleMovement()
		{
			_speedMultiplier = (Time.time - _lastFoodEatenAt < BoostDuration) ? BoostMultiplier : NormalSpeedMultiplier;
			_speed = MMMaths.Lerp(_speed, Speed * _speedMultiplier, SpeedChangeRate, Time.deltaTime);
			_newPosition = (_speed * Time.deltaTime * Direction);
			this.transform.position += _newPosition;
		}
        
		/// <summary>
		/// Called when turning, rotates the snake's head, changes its direction, plays a feedback
		/// </summary>
		public virtual void Turn()
		{
			TurnFeedback?.PlayFeedbacks();
			Direction = MMMaths.RotateVector2(Direction, 90f);
			this.transform.Rotate(new Vector3(0f,0f,90f));
		}

		/// <summary>
		/// Called by the snake head's MMViewportEdgeTeleporter, defines what happens when the snake is teleported to the other side of the screen
		/// </summary>
		public virtual void Teleport()
		{
			StartCoroutine(TeleportCo());
		}

		/// <summary>
		/// A coroutine used to teleport the snake to the other side of the screen
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator TeleportCo()
		{
			TeleportFeedback?.PlayFeedbacks();
            
			TeleportOnceFeedback?.PlayFeedbacks();
            
			yield return MMCoroutine.WaitForFrames(BodyPartsOffset);
            
			int total = _snakeBodyParts.Count;
			float feedbacksIntensity = 0f;
			float part = 1 / (float)total;    
            
			for (int i = 0; i < total; i++)
			{
				yield return MMCoroutine.WaitForFrames(BodyPartsOffset/2);
				feedbacksIntensity = 1 - i * part;
				TeleportFeedback?.PlayFeedbacks(this.transform.position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Called when eating food, triggers visual effects, increases points, plays a feedback
		/// </summary>
		public virtual void Eat()
		{
			EatEffect();
            
			EatFeedback?.PlayFeedbacks();
			SnakePoints++;
			PointsCounter.text = SnakePoints.ToString();
			StartCoroutine(EatCo());
		}

		/// <summary>
		/// A coroutine used to go through every snake body part and trigger its Eat MMFeedbacks
		/// </summary>
		protected virtual IEnumerator EatCo()
		{
			int total = _snakeBodyParts.Count;
			float feedbacksIntensity = 0f;
			float part = 1 / (float)total;    
            
			for (int i = 0; i < total; i++)
			{
				if (i >= _snakeBodyParts.Count)
				{
					yield break;
				}
				yield return MMCoroutine.WaitForFrames(BodyPartsOffset/2);
				feedbacksIntensity = 1 - i * part;
				if (i == total - 1)
				{
					if ((i < MaxAmountOfBodyParts - 1) && (_snakeBodyParts.Count > i))
					{
						if (_snakeBodyParts[i] != null)
						{
							_snakeBodyParts[i].New();    
						}
					}
				}
				else
				{
					_snakeBodyParts[i].Eat(feedbacksIntensity);    
				}
			}
		}

		/// <summary>
		/// When eating food, we instantiate a new body part unless we've reached our limit
		/// </summary>
		public virtual void EatEffect()
		{
			_lastFoodEatenAt = Time.time;
			if (SnakePoints < MaxAmountOfBodyParts - 1)
			{
				SnakeBodyPart part = Instantiate(BodyPartPrefab);
				SceneManager.MoveGameObjectToScene(part.gameObject, this.gameObject.scene);
				part.transform.position = this.transform.position;
				part.TargetRecorder = _recorder;
				part.Offset = ((SnakePoints) * BodyPartsOffset) + BodyPartsOffset + 1;
				part.Index = _snakeBodyParts.Count;
				part.name = "SnakeBodyPart_" + part.Index;
				_snakeBodyParts.Add(part);
			}
		}

		/// <summary>
		/// When we lose a body part, we play a feedback, destroy the last part, lose points, and update our points display 
		/// </summary>
		/// <param name="part"></param>
		public virtual void Lose(SnakeBodyPart part)
		{
			if (Time.time - _lastLostPart < MinTimeBetweenLostParts)
			{
				return;
			}

			_lastLostPart = Time.time;
			LoseFeedback?.PlayFeedbacks(part.transform.position);
			Destroy(_snakeBodyParts[_snakeBodyParts.Count-1].gameObject);
			_snakeBodyParts.RemoveAt(_snakeBodyParts.Count-1);
			SnakePoints--;
			PointsCounter.text = SnakePoints.ToString();
		}
	}
}