using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
public class StartHost : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<NetworkManager>().StartHost();
    }
}
