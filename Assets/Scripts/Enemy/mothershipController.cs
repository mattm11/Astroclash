using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class mothershipController : NetworkBehaviour
{
    private const float MAX_MOTHERSHIP_HEALTH = 500.0f;
    private NetworkVariable<float> health = new NetworkVariable<float>(MAX_MOTHERSHIP_HEALTH);

    private float patrolArea = 10.0f;
    private float credits = 10f;
    private int maxMinions = 3;
    private float spawnDistance = 2f;
    private float sumDeltaTime = 0.0f;

    private float bulletSpeed = 10.0f;
    private float damage = 10.0f;
    private float bulletRange = 15.0f;
    private float fireRate = 1.0f;
    private bool hasFired = false;

    private List<GameObject> minions;
    private GameObject targetPlayer;
    public GameObject bulletPref = null;

    private GameObject healthBar;
    private GameObject UIPlate;
    // Start is called before the first frame update
    void Start()
    {
        minions = new List<GameObject>();

        //uiplate for enemy
        UIPlate = gameObject.transform.parent.Find("UI Plates").gameObject;
        healthBar = UIPlate.transform.Find("Health bar").gameObject;
        healthBar.GetComponent<UIBar>().SetMaxValue(MAX_MOTHERSHIP_HEALTH);
        healthBar.GetComponent<UIBar>().SetValue(health.Value);
    }

    // Update is called once per frame
    void Update()
    {
        UIPlate.transform.position = gameObject.transform.position;
        if (IsServer)
        {
            if (health.Value <= 0.0f)
            {
                // despawn object and spawn credit
                ulong netID = gameObject.transform.parent.GetComponent<NetworkObject>().NetworkObjectId;
                GameObject credit = GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Credit", gameObject.transform.position);
                credit.GetComponent<CurrencyItem>().setValue(credits);
                GameObject.Find("Hostile Manager").GetComponent<hostileManager>().destroyMothership(gameObject.transform.parent.gameObject);
            }

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            StartCoroutine(checkDistance(players));
            if (targetPlayer != null)
            {
                lookAtPlayer();
                if (hasFired == false)
                {
                    StartCoroutine(shoot());
                }
            }
            // Perhaps spreading spawning of minions over time? :D
            if (minions.Count < maxMinions && Random.Range(0f, 1f) < 0.001)
            {
                spawnMinion();
            }
        }
    }

    void spawnMinion()
    {
        float angle = transform.rotation.z + ((Mathf.PI / 2) * (Random.Range(0, 2) * 2 - 1));
        Debug.Log(transform.rotation.z);
        Vector3 spawnPoint = new Vector3(transform.position.x + spawnDistance * Mathf.Cos(angle), transform.position.y + spawnDistance * Mathf.Sin(angle), transform.position.z);
        GameObject minion = GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Enemy Minion", spawnPoint);
        minion.transform.Find("Enemy").gameObject.tag = "Enemy";
        minion.transform.Find("Enemy").gameObject.layer = 8; //enemy player is 8
        minion.transform.Find("Enemy").GetComponent<enemyShipController>().setAnchor(gameObject.transform.position);
        //ignore the physics collision of the minions and mother ship
        Physics2D.IgnoreCollision(gameObject.GetComponent<PolygonCollider2D>(), minion.GetComponentInChildren<BoxCollider2D>());
        minions.Add(minion);
    }

    private void lookAtPlayer()
    {
        Vector3 direction = targetPlayer.transform.position - gameObject.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);

        gameObject.transform.rotation = rotation;
    }

    private IEnumerator checkDistance(GameObject[] _player)
    {
        bool foundPlayer = false;
        float closestDistance = 0.0f;

        for (int i = 0; i < _player.Length; i++)
        {
            float distance = Vector3.Distance(gameObject.transform.position, _player[i].transform.position);
            if (distance <= patrolArea)
            {
                if (closestDistance == 0.0f || distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPlayer = _player[i];
                    foundPlayer = true;
                }
            }
            yield return null;
        }

        if (foundPlayer == false)
        {
            targetPlayer = null;
        }
    }

    private void destroyMothership()
    {
        GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(this.GetComponent<NetworkObject>().NetworkObjectId);
        GameObject credit = GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Credit", gameObject.transform.position);
        credit.GetComponent<CurrencyItem>().setValue(credits);
    }

    // we really should have had a base enemy class with things like this in it
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsServer)
            Debug.Log("Is Player Bullet? " + collider.GetComponent<bulletProjectiles>().isPlayerBullet.Value);

        if (!IsServer && collider.GetComponent<bulletProjectiles>() != null && collider.GetComponent<bulletProjectiles>().isPlayerBullet.Value)
        {
            float damage = collider.GetComponent<bulletProjectiles>().getDamage();
            ulong bulletID = collider.GetComponent<NetworkObject>().NetworkObjectId;
            dealDamageServerRpc(damage, bulletID);
        }

        //kill enemies that enter safe zone
        if (IsServer && collider.gameObject.name == "Space Station")
        {
            health.Value = 0.0f;
        }
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

    private IEnumerator shoot()
    {
        hasFired = true;
        sumDeltaTime = 0.0f;
        while (sumDeltaTime < fireRate)
        {
            sumDeltaTime += Time.deltaTime;
            yield return null;
        }

        if (targetPlayer != null)
        {
            Vector3 direction = targetPlayer.transform.position - gameObject.transform.position;
            direction = Vector3.Normalize(direction);
            createBullet(direction, gameObject.transform.Find("Left Gun").transform.position, bulletSpeed, bulletRange, damage);
            createBullet(direction, gameObject.transform.Find("Right Gun").transform.position, bulletSpeed, bulletRange, damage);

        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        StartCoroutine(checkDistance(players));

        hasFired = false;
    }

    void createBullet(Vector3 _direction, Vector3 _position, float _projectileSpeed, float _projectileRange, float _projectileDamage)
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);

        GameObject bullet = null;
        bullet = Instantiate(bulletPref, _position, rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(_direction * _projectileSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<bulletProjectiles>().setStats(_projectileRange, _projectileDamage);

        bullet.GetComponent<NetworkObject>().Spawn();
        ulong bulletID = bullet.GetComponent<NetworkObject>().NetworkObjectId;

        //This needs to happen (fix weird physics issues)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            Physics2D.IgnoreCollision(players[i].GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Physics2D.IgnoreCollision(enemies[i].GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());
        }

        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        for (int i = 0; i < asteroids.Length; i++)
        {
            Physics2D.IgnoreCollision(asteroids[i].GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());
        }

        //enemy's collider too (concerned about how this will interact with other enemy objects)
        Physics2D.IgnoreCollision(gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());  //ignore physics based collision box
        Physics2D.IgnoreCollision(gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<CircleCollider2D>()); //ignore trigger also (avoid it from deleting its own bullets)

        createBulletClientRpc(bulletID);
    }

    [ClientRpc]
    void createBulletClientRpc(ulong _bulletID)
    {
        GameObject bullet = NetworkManager.SpawnManager.SpawnedObjects[_bulletID].gameObject;
        bullet.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.25f, 0.25f);
        bullet.tag = "enemyBullet";

        //This needs to happen (weird physics issues)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            Physics2D.IgnoreCollision(players[i].GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Physics2D.IgnoreCollision(enemies[i].GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());
        }

        //enemy's collider too (concerned about how this will interact with other enemy objects)
        Physics2D.IgnoreCollision(gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());  //ignore physics based collision box
        Physics2D.IgnoreCollision(gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<CircleCollider2D>()); //ignore trigger also (avoid it from deleting its own bullets)
    }
}
