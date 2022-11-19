using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class enemyShipController : NetworkBehaviour
{
    private NetworkVariable<float> health = new NetworkVariable<float>(100.0f);

    Vector3 walkPosition;
    Vector3 prevWalkPosition;

    private GameObject targetPlayer = null;
    public GameObject bulletPref = null;

    private Vector3 anchor;

    //random walk
    private float tether = 5.0f;
    private float aggroDistance = 10.0f;
    private float sumDeltaTime = 0.0f;
    private float interpTime = 1.0f;
    private float angle = 0.0f;
    private float prevAngle = 0.0f;

    //weapon data
    private float bulletSpeed = 10.0f;
    private float damage = 10.0f;
    private float bulletRange = 15.0f;
    private float fireRate = 1.0f;  
    private bool hasFired = true; 

    //misc
    private float score = 100.0f;
    private float credits = 1000.0f;

    // Start is called before the first frame update
    void Start()
    {
        anchor = gameObject.transform.position;

        //Generate first walk position
        if (IsServer)
        {
            generateWalk(anchor, tether);
            StartCoroutine(Patrol(1.0f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer != null)
        {
            lookAtPlayer();
        }

        if (health.Value <= 0.0f && IsServer)
        {
            spawnDebrisClientRpc(gameObject.transform.position);
            ulong netID = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
            GameObject credit = GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Credit", gameObject.transform.position);
            credit.GetComponent<CurrencyItem>().setValue(credits);
            gameObject.GetComponent<NetworkObject>().Despawn();
        }

        //Behavior routines
        if (gameObject.transform.position == walkPosition && IsServer && targetPlayer == null)
        {
            generateWalk(anchor, tether);
            StartCoroutine(Patrol(1.0f));
        }
        else if (IsServer && targetPlayer != null && hasFired == true)
        {
            StartCoroutine(shoot());
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer && collider.tag == "friendlyBullet")
        {
            GameObject.Destroy(collider.gameObject);
            float damage = collider.GetComponent<bulletProjectiles>().getDamage();
            dealDamageServerRpc(damage);
        }
        else if (!IsServer && collider.tag == "enemyBullet")
        {
            Debug.Log("Deleted enemy bullet");
            GameObject.Destroy(collider.gameObject);
        }
        
        //kill enemies that enter safe zone
        if (IsServer && collider.gameObject.name == "Space Station")
        {
            health.Value = 0.0f;
        }
    }

    private void generateWalk(Vector3 _anchor, float _tether)
    {
        angle = Random.Range(0.0f, 7.0f);
        float distance = Random.Range(_tether/2.0f, _tether);
        Vector3 rightVector = new Vector3(1.0f, 0.0f ,0.0f);
        rightVector *= distance;

        Vector3 transVector = new Vector3(
            (float)rightVector.x * Mathf.Cos(angle) - (float)rightVector.y * Mathf.Sin(angle),
            (float)rightVector.x * Mathf.Sin(angle) + (float)rightVector.y * Mathf.Cos(angle),
            (float)rightVector.z
        );

        prevWalkPosition = walkPosition;
        walkPosition = transVector;
    }

    private void lookAtPlayer()
    {
        Vector3 direction = targetPlayer.transform.position - gameObject.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);

        gameObject.transform.rotation = rotation;
    }

    //patrol routine
    private IEnumerator Patrol(float _timeWait)
    {
        StartCoroutine(LerpRotation());
        float sumDeltaTime = 0.0f;
        while (sumDeltaTime < _timeWait && targetPlayer == null)
        {
            sumDeltaTime += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(LerpPosition());
    }
    private IEnumerator LerpPosition()
    {
        float distance = Vector3.Distance(prevWalkPosition, walkPosition);
        float speed = 1.0f * distance;
        float ratio = 0.0f;

        while (ratio < 1.0f && targetPlayer == null)
        {
            ratio += Time.deltaTime/speed;
            gameObject.transform.position = Vector3.Lerp(prevWalkPosition, walkPosition, ratio);
            yield return null;
        }

        if (targetPlayer != null)
        {
            walkPosition = gameObject.transform.position;
        }
    }
    private IEnumerator LerpRotation()
    {
        Vector3 direction = walkPosition - prevWalkPosition;
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion newRotation = Quaternion.AngleAxis(angle, transform.forward);
        Quaternion previousRotation = gameObject.transform.rotation;

        float speed = 1.0f;
        float ratio = 0.0f;

        while (ratio < 1.0f && targetPlayer == null)
        {
            ratio += Time.deltaTime/speed;
            gameObject.transform.rotation = Quaternion.Lerp(previousRotation, newRotation, ratio);
            yield return null;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        StartCoroutine(checkDistance(players));
    }

    //looks for players and their distances (Probably should choose one to focus)
    private IEnumerator checkDistance(GameObject[] _player)
    {
        bool foundPlayer = false;

        for (int i = 0; i < _player.Length; i++)
        {
            float distance = Vector3.Distance(gameObject.transform.position, _player[i].transform.position);
            if (distance <= aggroDistance)
            {
                targetPlayer = _player[i];
                foundPlayer = true;
            }
            yield return null;
        }

        if (foundPlayer == false)
        {
            targetPlayer = null;
        }
    }

    private IEnumerator shoot()
    {
        hasFired = false;
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
            createBulletClientRpc(direction, gameObject.transform.position, bulletSpeed, bulletRange, damage);
            
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        StartCoroutine(checkDistance(players));

        hasFired = true;  
    }

    // Network Functions
    [ServerRpc(RequireOwnership = false)]
    void dealDamageServerRpc(float _damage)
    {
        Debug.Log("This object has been dealt damage!");
        health.Value -= _damage;
    }

    [ClientRpc]
    void createBulletClientRpc(Vector3 _direction, Vector3 _position, float _projectileSpeed, float _projectileRange, float _projectileDamage)
    {  
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);

        GameObject bullet = null;
        bullet = Instantiate(bulletPref, _position, rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(_direction * _projectileSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<bulletProjectiles>().setStats(_projectileRange, _projectileDamage);
        
        bullet.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.25f, 0.25f);
        bullet.tag = "enemyBullet";

        //This needs to happen (weird physics issues)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            Physics2D.IgnoreCollision(players[i].GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());
        }

        //enemy's collider too (concerned about how this will interact with other enemy objects)
        Physics2D.IgnoreCollision(gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());  //ignore physics based collision box
        Physics2D.IgnoreCollision(gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<CircleCollider2D>()); //ignore trigger also (avoid it from deleting its own bullets)
    }

    [ClientRpc]
    private void spawnDebrisClientRpc(Vector3 _position)
    {
        UnityEngine.Object debris = Resources.Load("prefabs/Enemy Minion Debris");
        for (int i = 0; i < 3; i++)
        {
            Instantiate(debris, _position, Quaternion.identity);
        }
    }
}