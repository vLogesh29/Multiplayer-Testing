using Fusion;
using Mono.CSharp;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{

    public struct InputValues : INetworkInput
    {
        public Vector2 move { get; private set; }
        public Vector2 look { get; private set; }
        public bool jump { get; private set; }
        public bool sprint { get; private set; }

        public InputValues(Vector2 move, Vector2 look, bool jump, bool sprint)
        {
            this.move = move;
            this.look = look;
            this.jump = jump;
            this.sprint = sprint;
		}
		public static bool operator == (InputValues left, InputValues right)
		{
			return (left.move == right.move &&
				left.look == right.look &&
				left.jump == right.jump &&
				left.sprint == right.sprint);
		}
        public static bool operator != (InputValues left, InputValues right)
        {
			return !(left == right);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class StarterAssetsInputs : MonoBehaviour
	{
		[field: Header("Character Input Values")]
		[field: SerializeField] public Vector2 move { private get; set; } = Vector2.zero;
        [field: SerializeField] public Vector2 look { private get; set; } = Vector2.zero;
        [field: SerializeField] public bool jump { private get; set; } = false;
        [field: SerializeField] public bool sprint { private get; set; } = false;

        public InputValues GetInputValues() => new InputValues(move, look, jump, sprint);

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;


        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
			if (Cursor.lockState != CursorLockMode.Locked)
				Cursor.lockState = CursorLockMode.Locked;
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
			JumpInput(value.isPressed);
		}

        public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
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

        private void Update()
        {
			if (Keyboard.current.escapeKey.wasReleasedThisFrame && Cursor.lockState != CursorLockMode.None)
				Cursor.lockState = CursorLockMode.None;
        }

        private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.None;
		}
	}
	
}