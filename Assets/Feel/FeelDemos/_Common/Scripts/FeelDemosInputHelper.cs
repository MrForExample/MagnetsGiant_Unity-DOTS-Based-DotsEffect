using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Feel
{
	/// <summary>
	/// This class contains a number of helper methods that will check for input in both the old and the new input system.
	/// </summary>
	public static class FeelDemosInputHelper 
	{
		private const string _horizontalAxis = "Horizontal";
		private const string _verticalAxis = "Vertical";
		
		public static bool CheckMainActionInputPressedThisFrame()
		{
			bool input = false;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.spaceKey.wasPressedThisFrame
			        || Mouse.current.leftButton.wasPressedThisFrame;
			#else
			input = (Input.GetKeyDown(KeyCode.Space) 
			         || Input.GetKeyDown(KeyCode.Joystick1Button0) 
			         || Input.GetMouseButtonDown(0));
			#endif

			return input;
		}
		public static bool CheckMainActionInputPressed()
		{
			bool input = false;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.spaceKey.isPressed
			        || Mouse.current.leftButton.isPressed;
			#else
			input = (Input.GetKey(KeyCode.Space) 
			         || Input.GetKey(KeyCode.Joystick1Button0) 
			         || Input.GetMouseButton(0));
			#endif

			return input;
		}
		
		public static bool CheckMainActionInputUpThisFrame()
		{
			bool input = false;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.spaceKey.wasReleasedThisFrame
			        || Mouse.current.leftButton.wasReleasedThisFrame;
			#else
			input = (Input.GetKeyUp(KeyCode.Space) 
			         || Input.GetKeyUp(KeyCode.Joystick1Button0) 
			         || Input.GetMouseButtonUp(0));
			#endif

			return input;
		}
		
		public static bool CheckEnterPressedThisFrame()
		{
			bool input = false;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.enterKey.wasPressedThisFrame;
			#else
			input = (Input.GetKeyDown(KeyCode.Return));
			#endif

			return input;
		}

		public static bool CheckMouseDown()
		{
			bool input = false;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				input = Mouse.current.leftButton.wasReleasedThisFrame;
			#else
			input = Input.GetMouseButtonUp(0);
			#endif

			return input;
		}

		public static Vector2 MousePosition()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				return Mouse.current.position.ReadValue();
			#else
			return Input.mousePosition;
			#endif
		}

		public static Vector2 GetDirectionAxis(ref Vector2 direction)
		{
			direction.x = 0f;
			direction.y = 0f;
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				if (Keyboard.current.leftArrowKey.isPressed)
				{
					direction.x = -1f;
				}
				else if (Keyboard.current.rightArrowKey.isPressed)
				{
					direction.x = 1f;
				}
				if (Keyboard.current.downArrowKey.isPressed)
				{
					direction.y = -1f;
				}
				else if (Keyboard.current.upArrowKey.isPressed)
				{
					direction.y = 1f;
				}
			#else
			direction.x = Input.GetAxis(_horizontalAxis);
			direction.y = Input.GetAxis(_verticalAxis);
			#endif

			return direction;
		}
		
		public static bool CheckAlphaInputPressedThisFrame(int alpha)
		{
			bool input = false;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				switch(alpha)
				{
					case 1:
						input = Keyboard.current.digit1Key.wasPressedThisFrame;
						break;
					case 2:
						input = Keyboard.current.digit2Key.wasPressedThisFrame;
						break;
					case 3:
						input = Keyboard.current.digit3Key.wasPressedThisFrame;
						break;
					case 4:
						input = Keyboard.current.digit4Key.wasPressedThisFrame;
						break;
				}
			#else
			switch(alpha)
			{
				case 1:
					input = Input.GetKeyDown(KeyCode.Alpha1);
					break;
				case 2:
					input = Input.GetKeyDown(KeyCode.Alpha2);
					break;
				case 3:
					input = Input.GetKeyDown(KeyCode.Alpha3);
					break;
				case 4:
					input = Input.GetKeyDown(KeyCode.Alpha4);
					break;
			}
			#endif

			return input;
		}
	}
}