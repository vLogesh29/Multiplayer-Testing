using Cinemachine;
using FishNet.Object;
using FishNet.Connection;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace FishnetTest
{
    public class FishnetNetworkManager : NetworkBehaviour
    {

        [Header("Setup")]
        [SerializeField] private CinemachineVirtualCamera followCam;
        [SerializeField] private int BotsCount = 100;
        [SerializeField] private Vector2 minSpawnRange, maxSpawnRange;
        [SerializeField] private GameObject BotPrefab;
        [SerializeField] private List<GameObject> bots = new();
        [SerializeField] private TMP_InputField botCountInput;

        private void Start()
        {
            botCountInput.onValueChanged.AddListener((string val) => SetBotCountInputField(val));
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            //activeConnections = NetworkServer.connections.Count;

            for (int i = 0; i < BotsCount; i++)
            {
                GameObject botInstance = Instantiate(BotPrefab,
                    new Vector3(Random.Range(minSpawnRange.x, maxSpawnRange.x), 0, Random.Range(minSpawnRange.y, maxSpawnRange.y)),
                    Quaternion.identity);
                bots.Add(botInstance);
                SpawnOnServer(botInstance);
            }

            Debug.Log("Network Started!");
        }


        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            bots.Clear();
            Debug.Log("Network Stopped!");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOffline)
                StartCoroutine(OnClientPlayerAdded());
        }

        private IEnumerator OnClientPlayerAdded()
        {
            yield return new WaitUntil(() => base.LocalConnection.FirstObject != null);
            followCam.Priority = 1;
            followCam.Follow = base.LocalConnection.FirstObject.GetComponent<ThirdPersonController>().GetCinemachineCameraTarget();
            Debug.Log("Client Connection cam set.");
        }

        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            var spawnedObjs = connection.Objects.ToArray();
            Debug.Log(spawnedObjs[spawnedObjs.Length-1].gameObject +" spawned, client-ID:" + connection.ClientId + " connected!");
        }


        public override void OnStopClient()
        {
            base.OnStopClient();

            followCam.Priority = -1;
            followCam.Follow = null;
            Debug.Log("Client disconnected!");
        }

        [Server(Logging = FishNet.Managing.Logging.LoggingType.Off)]
        public void SpawnOnServer(GameObject prefabInstance, FishNet.Connection.NetworkConnection conn = null, string name = null)
        {
            if (name == null) name = prefabInstance.name;
            int id = (conn != null) ? conn.ClientId : -1;
            prefabInstance.name = name + $" [ID={id}]"; // if id =-1, mean server only accessable (not belonging to any client)
                                                        // this function just to add name to be specifc on server end for debugging purposes
            base.Spawn(prefabInstance);
        }

        private void SetBotCountInputField(string count)
        {
            if (int.TryParse(count, out var botCount))
                BotsCount = botCount;
        }
    }
}