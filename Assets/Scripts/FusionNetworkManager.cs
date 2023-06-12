using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace FusionTest
{
    public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static FusionNetworkManager Instance { get; private set; } = null;

        [Header("Setup")]
        [SerializeField] private CinemachineVirtualCamera followCam;
        [SerializeField] private int BotsCount = 100;
        [SerializeField] private Vector2 minSpawnRange, maxSpawnRange;
        [SerializeField] private GameObject BotPrefab;
        [SerializeField] private List<NetworkObject> bots = new();
        [SerializeField] private TMP_InputField botCountInput;

        public NetworkRunner Runner { get; private set; } = null;
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            botCountInput.onValueChanged.AddListener((string val) => SetBotCountInputField(val));
        }

        private IEnumerator OnClientPlayerAdded(NetworkRunner runner)
        {
            yield return new WaitUntil(() => _spawnedCharacters.TryGetValue(runner.LocalPlayer, out _));
            followCam.Priority = 1;
            followCam.Follow = _spawnedCharacters[runner.LocalPlayer].GetComponent<ThirdPersonController>().GetCinemachineCameraTarget();
            Debug.Log("Client Connection cam set.");
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
        public void SpawnOnServer(NetworkRunner runner, GameObject prefab, PlayerRef playerRef,
            Vector3 position, Quaternion rotation,
            List<NetworkObject> instanceList = null, string name = null)
        {
            if (!runner.IsServer) return;

            if (name == null) name = prefab.name;
            int id = (playerRef.IsNone) ? playerRef.PlayerId : -1;

            NetworkObject networkPrefabInstance = runner.Spawn(prefab, position, rotation, playerRef);

            networkPrefabInstance.name = name + $" [ID={id}]"; // if id =-1, mean server only accessable (not belonging to any client)
                                                               // this function just to add name to be specifc on server end for debugging purposes

            if (instanceList != null)
                instanceList.Add(networkPrefabInstance);
        }


        private void SetBotCountInputField(string count)
        {
            if (int.TryParse(count, out var botCount))
                BotsCount = botCount;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Create a unique position for the player
            int noOfPlayers = runner.Config.Simulation.DefaultPlayers;
            Vector3 spawnPosition = new Vector3(((player.RawEncoded % noOfPlayers) - (noOfPlayers / 2)) * 3, 0, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedCharacters.Add(player, networkPlayerObject);
            Debug.Log(_spawnedCharacters[player].gameObject + " spawned, client-ID:" + player.PlayerId + " connected!");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
            Debug.Log(_spawnedCharacters[player].gameObject + " spawned, client-ID:" + player.PlayerId + " connected!");
        }


        public InputValues NetworkInputValues;
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(NetworkInputValues);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            if (runner.IsServer)
            {
                for (int i = 0; i < BotsCount; i++)
                {
                    Vector3 position = new Vector3(Random.Range(minSpawnRange.x, maxSpawnRange.x), 0, Random.Range(minSpawnRange.y, maxSpawnRange.y));
                    Quaternion rotation = Quaternion.identity;
                    SpawnOnServer(runner, BotPrefab, PlayerRef.None, position, rotation, bots);
                }
            }
            Debug.Log("Network Started!");
            
            if (runner.IsConnectedToServer)
                StartCoroutine(OnClientPlayerAdded(runner));
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            bots.Clear();
            Debug.Log("Network Stopped!");

            followCam.Priority = -1;
            followCam.Follow = null;
            Debug.Log("Client disconnected!");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {

        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        private void OnGUI()
        {
            if (Runner == null)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                {
                    StartGame(GameMode.Host);
                }
                if (GUI.Button(new Rect(0, 40, 200, 40), "Client"))
                {
                    StartGame(GameMode.Client);
                }
                if (GUI.Button(new Rect(0, 80, 200, 40), "Server"))
                {
                    StartGame(GameMode.Server);
                }
            }
            else
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Exit"))
                {
                    Runner.Shutdown();
                }
            }
        }
        async void StartGame(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            Runner = gameObject.AddComponent<NetworkRunner>();
            Runner.ProvideInput = true;

            // Start or join (depends on gamemode) a session with a specific name
            await Runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "MultiplayerTest",
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }
    }
}