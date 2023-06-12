using Cinemachine;
using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorTest
{
    public class MirrorNetworkManager : NetworkManager
    {
        [Header("Setup")]
        [SerializeField] private CinemachineVirtualCamera followCam;
        [SerializeField] private int BotsCount = 100;
        [SerializeField] private Vector2 minSpawnRange, maxSpawnRange;
        [SerializeField] private GameObject BotPrefab;
        [SerializeField] private List<GameObject> bots = new();
        [SerializeField] private TMP_InputField botCountInput;

        public override void Start()
        {
            base.Start();

            botCountInput.onValueChanged.AddListener((string val) => SetBotCountInputField(val));
        }

        public override void OnStartServer()
        {
            //activeConnections = NetworkServer.connections.Count;

            // enable spatial hassing only client only...
            GetComponent<SpatialHashingInterestManagement>().enabled = NetworkClient.active && !NetworkClient.activeHost;

            for (int i = 0; i < BotsCount; i++)
            {
                GameObject botInstance = Instantiate(BotPrefab,
                    new Vector3(Random.Range(minSpawnRange.x, maxSpawnRange.x), 0, Random.Range(minSpawnRange.y, maxSpawnRange.y)),
                    Quaternion.identity);
                bots.Add(botInstance);
                SpawnOnServer(botInstance);
            }

            Debug.Log("Server Started!");
        }

        public override void OnStopServer()
        {
            bots.Clear();
            Debug.Log("Server Stopped!");
        }

        public override void OnStartClient()
        {
            if (isNetworkActive)
                StartCoroutine(OnClientPlayerAdded());
        }

        private IEnumerator OnClientPlayerAdded()
        {
            yield return new WaitUntil(() => NetworkClient.connection != null && NetworkClient.connection.identity != null);
            followCam.Priority = 1;
            followCam.Follow = NetworkClient.connection.identity.GetComponent<ThirdPersonController>().GetCinemachineCameraTarget();
            Debug.Log("Client Connection cam set.");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            Debug.Log("Player ID:"+conn.connectionId+" connected!");
        }

        public override void OnStopClient()
        {

            followCam.Priority = -1;
            followCam.Follow = null;
            Debug.Log("Client disconnected!");
        }


        [Server]
        public static void SpawnOnServer(GameObject prefabInstance, NetworkConnection conn = null, string name = null)
        {
            if (name == null) name = prefabInstance.name;
            int id = (conn != null) ? conn.connectionId : -1;
            prefabInstance.name = name + $" [ID={id}]"; // if id =-1, mean server only accessable (not belonging to any client)
            // this function just to add name to be specifc on server end for debugging purposes
            NetworkServer.Spawn(prefabInstance, conn);
        }

        private void SetBotCountInputField(string count)
        {
            if (int.TryParse(count, out var botCount))
                BotsCount = botCount;
        }
    }
}