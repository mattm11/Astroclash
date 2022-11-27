using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Astroclash;
using System;

public class StartServer : MonoBehaviour
{
    int prevConnectionCount = -1;

    void Start()
    {
        Debug.Log("script");
        GetComponent<NetworkManager>().StartServer();
        Debug.Log("Server IP: " + GetComponent<UnityTransport>().ConnectionData.Address);
        Debug.Log("Server Port: " + GetComponent<UnityTransport>().ConnectionData.Port);
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
}
