using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
public class StartClient : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<NetworkManager>().StartClient();
    }
}
