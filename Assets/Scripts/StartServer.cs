using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class StartServer : MonoBehaviour
{
    int prevConnectionCount = -1;

    void Start()
    {
        GetComponent<NetworkManager>().StartServer();
        Debug.Log("Server IP: " + GetComponent<UnityTransport>().ConnectionData.Address);
        Debug.Log("Server Port: " + GetComponent<UnityTransport>().ConnectionData.Port);

        createSpawnManagerServerRpc();
        createEnemyServerRpc();
    }

    // Update is called once per frame
    void Update()
    {
        var tempCount = GetComponent<NetworkManager>().ConnectedClients.Count;
        if (tempCount != prevConnectionCount)
        {
            Debug.Log("Connections: " + tempCount);
            prevConnectionCount = tempCount;
        }
    }

    [ServerRpc]
    private void createSpawnManagerServerRpc()
    {
        //create the spawn manager
        Object prefab = Resources.Load("prefabs/Spawn Manager");
        GameObject SpawnManager = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SpawnManager.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void createEnemyServerRpc()
    {   
        Object prefab = Resources.Load("prefabs/Entities/Enemy Minion");
        // GameObject enemy;

        // for (int i = 0; i < 1; i++)
        // {
        //     enemy = (GameObject)Instantiate(prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        //     enemy.GetComponent<NetworkObject>().Spawn();
        // }
    }
}
