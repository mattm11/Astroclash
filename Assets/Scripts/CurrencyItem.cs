using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Astroclash;

public class CurrencyItem : NetworkBehaviour
{
    public float value = 100.0f;
    public float stayAlive = 5.0f; //time to say alive untouched in seconds
    private float aliveFor = 0.0f;

    // Update is called once per frame
    void Update()
    {
        aliveFor += Time.deltaTime;

        if (aliveFor >= stayAlive)
        {
            ulong netID = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
            despawnEntityServerRpc(netID);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.tag == "Player")
        {
            collider.gameObject.GetComponent<playerController>().addCurrency(value);
            ulong netID = gameObject.GetComponent<NetworkObject>().NetworkObjectId;

            despawnEntityServerRpc(netID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void despawnEntityServerRpc(ulong _netID)
    {
        GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(_netID);
    }
}
