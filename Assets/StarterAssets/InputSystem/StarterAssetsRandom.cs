using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Random = UnityEngine.Random;

namespace StarterAssets
{
    public class StarterAssetsRandom : StarterAssetsInputs
    {
        [Header("Input Delays")]
        public float moveInputDelay = 5f;
        public float lookInputDelay = 5f;
        public float jumpInputDelay = 3f;
        public float sprintInputDelay = 2f;

        //private float delayTimer;
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (Time.time % moveInputDelay < Time.deltaTime)
                RandomizeMoveInput();

            if (Time.time % lookInputDelay < Time.deltaTime)
                RandomizeLookInput();

            if (Time.time % jumpInputDelay < Time.deltaTime)
                RandomizeJumpInput();

            if (Time.time % sprintInputDelay < Time.deltaTime)
                RandomizeSprintInput();
        }

        private void RandomizeMoveInput()
        {
            move = Random.insideUnitSphere;
        }
        private void RandomizeLookInput()
        {
            look = Random.insideUnitSphere;
        }
        private void RandomizeJumpInput()
        {
            // max exclusive
            jump = Random.Range(0, 2) == 1;
        }
        private void RandomizeSprintInput()
        {
            sprint = Random.Range(0, 2) == 1;
        }
        private void OnApplicationFocus(bool hasFocus) { } 
    }
}