using Mirror;
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    public interface ThirdPersonController
    {
        public event EventHandler OnPlayerGrounded;
        public event EventHandler<float> OnPlayerMoved;
        public class JumpArgs : EventArgs { public bool jump; public bool freeFall; }
        public event EventHandler<JumpArgs> OnPlayerJumped;

        public float GetAnimationBlend();
        public bool GetGroundedState();
        public Vector3 GetCharacterCenter();
        public Transform GetCinemachineCameraTarget();
    }
}