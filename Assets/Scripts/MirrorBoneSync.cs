using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorTest
{
    public class MirrorBoneSync : NetworkBehaviour
    {
        [SerializeField] private Animator animator; // Animator component of the original rig

        [SyncVar(hook = nameof(OnPoseSynced))]
        private HumanPose syncedPose; // Synchronized HumanPose

        private HumanPoseHandler poseHandler;

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            poseHandler = new HumanPoseHandler(animator.avatar, transform.root);

            if (!isLocalPlayer)
                return;

            CmdRequestSyncedPose(); // Request synced HumanPose from the server
        }

        private void Update()
        {
            if (!isLocalPlayer)
                return;

            // Update the pose on the server
            CmdUpdatePose(GetCurrentPose());
        }

        private HumanPose GetCurrentPose()
        {
            // Get the current HumanPose from the original rig's Animator component
            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);
            return pose;
        }

        private void OnPoseSynced(HumanPose oldPose, HumanPose newPose)
        {
            // Apply the synced HumanPose to the mirror rig's Animator component
            poseHandler.SetHumanPose(ref newPose);
        }

        [Command]
        private void CmdRequestSyncedPose()
        {
            // Send the synced HumanPose to the client
            RpcSyncPose(GetCurrentPose());
        }

        [Command]
        private void CmdUpdatePose(HumanPose pose)
        {
            // Update the synced HumanPose on the server
            syncedPose = pose;
        }

        [ClientRpc]
        private void RpcSyncPose(HumanPose pose)
        {
            // Sync the HumanPose across all clients
            syncedPose = pose;
        }
    }
}