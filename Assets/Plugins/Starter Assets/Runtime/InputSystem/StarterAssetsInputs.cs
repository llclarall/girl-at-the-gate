using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Fallback Inputs")]
		public bool enableJumpKeyboardFallback = true;
		public bool enableJumpControllerFallback = true;
		public bool enableOscMovement = true;
		public bool enableOscJump = true;
		public float oscPressedThreshold = 0.5f;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		private Vector2 _moveFromInputAction;
		private Vector2 _moveFromOsc;
		private bool _jumpFromInputAction;
		private bool _jumpFromOsc;
		private bool _wasOscJumpPressed;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			_jumpFromInputAction = value.isPressed;
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			_moveFromInputAction = newMoveDirection;
			UpdateResolvedMove();
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			_jumpFromInputAction = newJumpState;
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		private void Update()
		{
			UpdateResolvedMove();
			UpdateJumpFallback();
		}

		private void UpdateResolvedMove()
		{
			move = _moveFromInputAction;

			if (enableOscMovement && _moveFromOsc != Vector2.zero)
			{
				move = _moveFromOsc;
			}
		}

		private void UpdateJumpFallback()
		{
			bool keyboardPressed = false;
			bool controllerPressed = false;

#if ENABLE_INPUT_SYSTEM
			if (enableJumpKeyboardFallback)
			{
				Keyboard keyboard = Keyboard.current;
				keyboardPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
			}

			if (enableJumpControllerFallback)
			{
				Gamepad gamepad = Gamepad.current;
				controllerPressed = gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;

				Joystick joystick = Joystick.current;
				controllerPressed = controllerPressed || (joystick != null && joystick.trigger != null && joystick.trigger.wasPressedThisFrame);
			}
#else
			keyboardPressed = enableJumpKeyboardFallback && Input.GetKeyDown(KeyCode.Space);
			controllerPressed = enableJumpControllerFallback && Input.GetKeyDown(KeyCode.JoystickButton0);
#endif

			bool fallbackJumpPressed = keyboardPressed || controllerPressed;
			if (fallbackJumpPressed || _jumpFromOsc)
				jump = true;

			// Keep OSC jump as a one-shot press, similar to button down behavior.
			_jumpFromOsc = false;
		}

		public void OnOscJump(float value)
		{
			if (!enableOscJump)
			{
				return;
			}

			bool isPressed = value >= oscPressedThreshold;
			if (isPressed && !_wasOscJumpPressed)
			{
				_jumpFromOsc = true;
			}

			_wasOscJumpPressed = isPressed;
		}

		public void OnOscJumpInt(int value)
		{
			OnOscJump(value);
		}

		public void OnOscJumpBang()
		{
			if (!enableOscJump)
			{
				return;
			}

			_jumpFromOsc = true;
		}

		// French aliases for easier direct mapping from Chataigne labels.
		public void OnOscSauter(float value)
		{
			OnOscJump(value);
		}

		public void OnOscSauterInt(int value)
		{
			OnOscJumpInt(value);
		}

		public void OnOscSauterBang()
		{
			OnOscJumpBang();
		}

		public void OnOscMoveHorizontal(float value)
		{
			SetOscHorizontal(Mathf.Clamp(value, -1f, 1f));
		}

		public void OnOscMoveVertical(float value)
		{
			SetOscVertical(Mathf.Clamp(value, -1f, 1f));
		}

		public void OnOscMoveHorizontalInt(int value)
		{
			SetOscHorizontal(Mathf.Clamp(value, -1, 1));
		}

		public void OnOscMoveVerticalInt(int value)
		{
			SetOscVertical(Mathf.Clamp(value, -1, 1));
		}

		public void OnOscMoveLeft()
		{
			SetOscHorizontal(-1f);
		}

		public void OnOscMoveRight()
		{
			SetOscHorizontal(1f);
		}

		public void OnOscMoveForward()
		{
			SetOscVertical(1f);
		}

		public void OnOscMoveBackward()
		{
			SetOscVertical(-1f);
		}

		public void OnOscMoveStop()
		{
			_moveFromOsc = Vector2.zero;
			UpdateResolvedMove();
		}

		private void SetOscHorizontal(float value)
		{
			_moveFromOsc.x = value;
			UpdateResolvedMove();
		}

		private void SetOscVertical(float value)
		{
			_moveFromOsc.y = value;
			UpdateResolvedMove();
		}
	}

}