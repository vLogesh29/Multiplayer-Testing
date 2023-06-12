using Mirror;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(ThirdPersonController))]
    public class ThirdPersonAnimator : MonoBehaviour
    {
        [SerializeField] private Animator Animator;
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [SerializeField] private bool _playAudio;

        private ThirdPersonController _player;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private bool _hasAnimator;

        private void Start()
        {
            _player = GetComponent<ThirdPersonController>();
            if (Animator == null)
                _hasAnimator = TryGetComponent(out Animator);
            else _hasAnimator = true;
            AssignAnimationIDs();

            _player.OnPlayerGrounded += Animator_OnPlayerGrounded;
            _player.OnPlayerMoved += Animator_OnPlayerMoved;
            _player.OnPlayerJumped += _controller_OnPlayerJumped;
        }

        private void _controller_OnPlayerJumped(object sender, ThirdPersonController.JumpArgs e)
        {
            if (_hasAnimator)
            {
                Animator.SetBool(_animIDJump, e.jump);
                Animator.SetBool(_animIDFreeFall, e.freeFall);
            }
        }

        private void Animator_OnPlayerMoved(object sender, float inpMag)
        {
            if (_hasAnimator)
            {
                Animator.SetFloat(_animIDSpeed, _player.GetAnimationBlend());
                Animator.SetFloat(_animIDMotionSpeed, inpMag);
            }
        }

        private void Animator_OnPlayerGrounded(object sender, System.EventArgs e)
        {
            if (_hasAnimator)
            {
                Animator.SetBool(_animIDGrounded, _player.GetGroundedState());
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        private void Update()
        {
            if (Animator == null)
                _hasAnimator = TryGetComponent(out Animator);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (!_playAudio) return;
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_player.GetCharacterCenter()), FootstepAudioVolume);
                }
            }
        }
        /*
        [Command]
        private void PlayFootstepAudioCMD(int index)
        {
            PlayFootstepAudioClientRPC(index);
        }

        [ClientRpc]
        private void PlayFootstepAudioClientRPC(int index)
        {
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_player.Controller.center), FootstepAudioVolume);
        }
        */

        private void OnLand(AnimationEvent animationEvent)
        {
            if (!_playAudio) return;
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_player.GetCharacterCenter()), FootstepAudioVolume);
            }
        }

    }
}