using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using System.Collections;
using System.Collections.Generic;
using Astroclash;
using System;


public class StartHost : MonoBehaviour
{
    int prevConnectionCount = -1;

    void Start()
    {
        Debug.Log("script");
        gameObject.GetComponent<NetworkManager>().StartHost();
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
