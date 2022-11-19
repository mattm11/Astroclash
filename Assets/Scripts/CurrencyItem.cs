using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Astroclash;

public class CurrencyItem : NetworkBehaviour
{
    private NetworkVariable<float> value = new NetworkVariable<float>(0.0f);
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
            collider.gameObject.GetComponent<playerController>().addCurrency(value.Value);
            ulong netID = gameObject.GetComponent<NetworkObject>().NetworkObjectId;

            despawnEntityServerRpc(netID);
        }
    }

    public void setValue(float _value)
    {
        value.Value = _value;
    }

    [ServerRpc(RequireOwnership = false)]
    void despawnEntityServerRpc(ulong _netID)
    {
        GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(_netID);
    }
}
