using System;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feel
{
	public struct StrikePin
	{
		public Rigidbody Rb;
		public Vector3 InitialPosition;
		public Quaternion InitialRotation;

		public void ResetPin()
		{
			Rb.transform.position = InitialPosition;
			Rb.transform.rotation = InitialRotation;
			Rb.velocity = Vector3.zero;
			Rb.angularVelocity = Vector3.zero;
		}
	}

	/// <summary>
	/// An example class part of the Feel demos
	/// This class handles the strike demo, detecting input and applying force to the ball
	/// </summary>
	public class Strike : MonoBehaviour
	{
		[Header("Input")]
		/// a key to use to throw the ball
		[Tooltip("a key to use to throw the ball")]
		public KeyCode ActionKey = KeyCode.Space;

		/// a secondary key to use to throw the ball
		[Tooltip("a secondary key to use to throw the ball")]
		public KeyCode ActionKeyAlt = KeyCode.Joystick1Button0;

		[Header("Bindings")]
		/// the rigidbody of the bowling ball
		[Tooltip("the rigidbody of the bowling ball")]
		public Rigidbody BowlingBallRb;
		/// a collider used to count points (still standing pins will overlap with it)
		[Tooltip("a collider used to count points (still standing pins will overlap with it)")]
		public Collider PointsCollider;
		/// the rigidbody of the pins
		[Tooltip("the rigidbody of the pins")] public List<Rigidbody> Pins;
		/// the wiggler that makes the launcher rotate
		[Tooltip("the wiggler that makes the launcher rotate")]
		public MMWiggle BowlingBallLauncherWiggler;
		/// the text component used to display the current last score
		[Tooltip("the text component used to display the current last score")]
		public Text LastScoreText;
		/// the text component used to display the total score
		[Tooltip("the text component used to display the total score")]
		public Text TotalScoreText;
		/// the text component used to display the number of consecutive strikes
		[Tooltip("the text component used to display the number of consecutive strikes")]
		public Text ConsecutiveStrikesText;
		/// a list of elements to turn on/off in case of strike
		[Tooltip("a list of elements to turn on/off in case of strike")]
		public List<GameObject> StrikeElements;
        
		[Header("Settings")]
		/// the force to apply when throwing the ball
		[Tooltip("the force to apply when throwing the ball")]
		public Vector3 ThrowingForce = new Vector3(0, 0, 10f);
		/// the gravity to apply
		[Tooltip("the gravity to apply")] public Vector3 Gravity = new Vector3(0f, -9.81f, 0f);
		/// the max duration before a reset
		[Tooltip("the max duration before a reset")]
		public float MaxDurationBeforeReset = 4f;
		/// the delay to wait for (in seconds) before resetting the scene
		[Tooltip("the delay to wait for (in seconds) before resetting the scene")]
		public float DelayBeforeReset = 1f;
		/// the delay to wait for (in seconds) while counting/displaying points
		[Tooltip("the delay to wait for (in seconds) while counting/displaying points")]
		public float DelayForPoints = 1f;

		[Header("Feedbacks")]
		/// a feedback to call when throwing the ball
		[Tooltip("a feedback to call when throwing the ball")]
		public MMFeedbacks ThrowBallFeedback;
		/// a feedback to call when resetting the scene
		[Tooltip("a feedback to call when resetting the scene")]
		public MMFeedbacks ResetFeedback;
		/// a feedback played when hitting a strike
		[Tooltip("a feedback played when hitting a strike")]
		public MMFeedbacks StrikeFeedback;
		/// a feedback played when missing a strike
		[Tooltip("a feedback played when missing a strike")]
		public MMFeedbacks NoStrikeFeedback;

		[Header("Scores")]
		/// the last score you hit
		[Tooltip("the last score you hit")]
		[MMReadOnly]
		public int LastScore = 0;
		/// The total amount of points since the start
		[Tooltip("The total amount of points since the start")]
		[MMReadOnly]
		public int TotalPoints = 0;
		/// the amount of consecutive strikes
		[Tooltip("the amount of consecutive strikes")]
		[MMReadOnly]
		public int ConsecutiveStrikes = 0;

        
		protected bool _ballThrown = false;
		protected Vector3 _initialBallPosition;
		protected Quaternion _initialBallRotation;
		protected List<StrikePin> _strikePins;
		protected List<Collider> _pinColliders;
        
		protected Coroutine _resetCoroutine;

		/// <summary>
		/// On Start we initialize our scene
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initializes physics settings, stores initial positions
		/// </summary>
		protected virtual void Initialization()
		{
			// we initialize our physics settings
			Physics.gravity = Gravity;
			Physics.bounceThreshold = 2;
			Physics.sleepThreshold = 0.005f;
			Physics.defaultContactOffset = 0.01f;
			Physics.defaultSolverIterations = 6;
			Physics.defaultSolverVelocityIterations = 1;
			Physics.queriesHitTriggers = true;

			// we initialize our point counters
			ConsecutiveStrikes = 0;
			LastScore = 0;
			TotalPoints = 0;
			ConsecutiveStrikesText.text = "0";
			LastScoreText.text = "0";
			TotalScoreText.text = "0";
			SetStrikeElements(false);
            
			// we store our ball's position & rotation
			_initialBallPosition = BowlingBallRb.transform.position;
			_initialBallRotation = BowlingBallRb.transform.localRotation;

			// we store our pins' positions & rotations
			_strikePins = new List<StrikePin>();
			_pinColliders = new List<Collider>();
			foreach (Rigidbody rb in Pins)
			{
				StrikePin pin = new StrikePin();
				pin.Rb = rb;
				pin.InitialPosition = rb.transform.position;
				pin.InitialRotation = rb.transform.rotation;
				_strikePins.Add(pin);
				_pinColliders.Add(pin.Rb.gameObject.GetComponent<Collider>());
			}
		}

		protected virtual void SetStrikeElements(bool status)
		{
			foreach (GameObject element in StrikeElements)
			{
				element.SetActive(status);
			}
		}

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
				StartBall();
			}
		}

		/// <summary>
		/// Performs a jump if possible, otherwise plays a denied feedback
		/// </summary>
		protected virtual void StartBall()
		{
			if (!_ballThrown)
			{
				ThrowBallFeedback?.PlayFeedbacks();
				BowlingBallLauncherWiggler.RotationActive = false;
				_ballThrown = true;
			}
		}

		/// <summary>
		/// This method, meant to be called by a feedback once it's ready to throw, will apply force to the ball.
		/// In a "normal" game this probably would be directly called by this class, but here we want to make sure all
		/// the previous feedbacks have played, and that's handled by the ThrowBallFeedback.
		/// </summary>
		public virtual void ThrowBall()
		{
			if (BowlingBallRb != null)
			{
				BowlingBallRb.AddRelativeForce(ThrowingForce, ForceMode.Impulse);
				BowlingBallRb.AddTorque(ThrowingForce, ForceMode.Impulse);
				_resetCoroutine = StartCoroutine(ResetCountdown());
			}
		}

		/// <summary>
		/// When colliding with the catcher, we reset
		/// </summary>
		/// <param name="other"></param>
		protected void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.MMGetComponentNoAlloc<StrikeBall>() != null)
			{
				StartCoroutine(ResetSceneCo());
				if (_resetCoroutine != null)
				{
					StopCoroutine(_resetCoroutine);
				}
			}
		}

		/// <summary>
		/// A countdown used to reset the scene after a max delay
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ResetCountdown()
		{
			yield return MMCoroutine.WaitFor(MaxDurationBeforeReset);
			StartCoroutine(ResetSceneCo());
		}

		/// <summary>
		/// Resets the whole scene
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ResetSceneCo()
		{
			yield return MMCoroutine.WaitFor(DelayBeforeReset);
            
            
			CountPoints();
            
			yield return MMCoroutine.WaitFor(DelayForPoints);
            
			ResetFeedback?.PlayFeedbacks();

			yield return MMCoroutine.WaitFor(0.1f);

			// we reset the ball's position and forces
			BowlingBallRb.MovePosition(_initialBallPosition);
			BowlingBallRb.transform.localRotation = _initialBallRotation;
			BowlingBallRb.velocity = Vector3.zero;
			BowlingBallRb.angularVelocity = Vector3.zero;

			yield return MMCoroutine.WaitForFrames(1);

			BowlingBallRb.transform.position = _initialBallPosition;
            
			// we make our launcher rotate again
			BowlingBallLauncherWiggler.RotationActive = true;

			foreach (StrikePin pin in _strikePins)
			{
				pin.ResetPin();
			}

			_ballThrown = false;
		}

		protected virtual void CountPoints()
		{
			int points = 10;
			foreach (Collider col in _pinColliders)
			{
				if (col.bounds.Intersects(PointsCollider.bounds))
				{
					points--;
				}
			}

			LastScore = points;
			ConsecutiveStrikes = (points == 10) ? ConsecutiveStrikes + 1 : 0;

			if (points == 10)
			{
				StrikeFeedback?.PlayFeedbacks();
				SetStrikeElements(true);
			}
			else
			{
				NoStrikeFeedback?.PlayFeedbacks();
				SetStrikeElements(false);
			}

			TotalPoints += points;

			ConsecutiveStrikesText.text = ConsecutiveStrikes.ToString();
			LastScoreText.text = LastScore.ToString();
			TotalScoreText.text = TotalPoints.ToString();
		}
	}
}