using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Collections;

public class asteroidBehavior : NetworkBehaviour
{
    public float startingHealth = 0.0f;
    public string asteriodToSpawn = "";
    private NetworkVariable<float> health = new NetworkVariable<float>();

    public float rotationSpeed = 15f;
    public Vector2 direction;
    public float velocity;
    public float credits = 100f;

    private GameObject healthBar;
    private GameObject UIPlate;

    private void Awake()
    {
        this.direction = Random.insideUnitCircle;
        this.direction.Normalize();
        this.velocity = 0.5f;
    }

    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(this.direction * this.velocity, ForceMode2D.Impulse);
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        if (IsServer)
        {
            health.Value = startingHealth;
        }
        UIPlate = gameObject.transform.parent.transform.Find("UI Plates").gameObject;
        healthBar = UIPlate.transform.Find("Health bar").gameObject;
        healthBar.GetComponent<UIBar>().SetMaxValue(startingHealth);
        healthBar.GetComponent<UIBar>().SetValue(startingHealth);

        StartCoroutine(collisionTimer());
    }

    private void Update()
    {
        UIPlate.transform.position = gameObject.transform.position;
        if (health.Value <= 0.0f && IsServer)
        {
            // despawn object and spawn credit
            ulong netID = gameObject.transform.parent.GetComponent<NetworkObject>().NetworkObjectId;
            createDebrisClientRpc(gameObject.transform.position);
            GameObject credit = GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Credit", gameObject.transform.position);
            credit.GetComponent<CurrencyItem>().setValue(credits);

            //spawn the new asteriods
            if (asteriodToSpawn != "")
            {
                GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity(asteriodToSpawn, gameObject.transform.position);
                GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity(asteriodToSpawn, gameObject.transform.position);
            }

            GameObject.Find("Hostile Manager").GetComponent<hostileManager>().destroyAsteroid(gameObject.transform.parent.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Asteroid")
        {
            Vector2 direction = collision.contacts[0].point - new Vector2(transform.position.x, transform.position.y);
            direction = -direction.normalized;
            GetComponent<Rigidbody2D>().AddForce(direction * this.velocity);
        }

        //kill enemies that enter safe zone
        if (IsServer && collision.collider.gameObject.name == "Space Station")
        {
            health.Value = 0.0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer && collider.GetComponent<bulletProjectiles>() != null && collider.GetComponent<bulletProjectiles>().isPlayerBullet.Value)
        {
            float damage = collider.GetComponent<bulletProjectiles>().getDamage();
            ulong bulletID = collider.GetComponent<NetworkObject>().NetworkObjectId;
            dealDamageServerRpc(damage, bulletID);
        }
    }

    private IEnumerator collisionTimer()
    {
        float sumTimeDelta = 0.0f;
        while (sumTimeDelta < 1.0f)
        {
            sumTimeDelta += Time.deltaTime;
            yield return null;
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void dealDamageServerRpc(float _damage, ulong _bulletID)
    {
        health.Value -= _damage;
        try
        {
            NetworkManager.SpawnManager.SpawnedObjects[_bulletID].gameObject.GetComponent<NetworkObject>().Despawn();
        }
        catch (KeyNotFoundException e)
        {
            Debug.Log(e.Message);
        }
        
        updateHealthBarClientRpc();
    }

    [ClientRpc]
    private void updateHealthBarClientRpc()
    {
        Debug.Log("Changing healthbar of enemy");
        healthBar.GetComponent<UIBar>().SetValue(health.Value);
    }
    [ClientRpc]
    private void createDebrisClientRpc(Vector3 _position)
    {
        UnityEngine.Object debris = Resources.Load("prefabs/Asteroid Debris");
        for (int i = 0; i < 3; i++)
        {
            Instantiate(debris, _position, Quaternion.identity);
        }
    }
}
