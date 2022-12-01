using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class bulletProjectiles : NetworkBehaviour
{
    private float range = 0.0f;
    private NetworkVariable<float> damage = new NetworkVariable<float>(0.0f);
    public NetworkVariable<ulong> spawnerID = new NetworkVariable<ulong>();
    public NetworkVariable<bool> isPlayerBullet = new NetworkVariable<bool>(false);
    private Vector3 startPosition = new Vector3();

    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //calculate distance traveled
        float distance = Mathf.Sqrt(Mathf.Pow((transform.position.x - startPosition.x), 2) + Mathf.Pow((transform.position.y - startPosition.y), 2));
        if (distance > range && IsServer)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Space Station" && IsServer)
        {
            GameObject.Destroy(gameObject);
            //gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }

    public void setStats(float _range, float _damage, ulong _clientID)
    {
        range = _range;
        damage.Value = _damage;
        spawnerID.Value = _clientID;
        isPlayerBullet.Value = true;
    }

    public void setStats(float _range, float _damage)
    {
        range = _range;
        damage.Value = _damage;
    }

    public float getDamage()
    {
        return damage.Value;
    }

    public ulong getSpawnerID()
    {
        return spawnerID.Value;
    }
}
