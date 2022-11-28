using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class asteroidBehavior : NetworkBehaviour
{
    private const float MAX_ASTEROID_HEALTH = 100.0f;
    private NetworkVariable<float> health = new NetworkVariable<float>(MAX_ASTEROID_HEALTH);

    public float rotationSpeed = 15f;
    public Vector2 direction;
    public float velocity;
    private float credits = 100f;

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

        UIPlate = gameObject.transform.parent.transform.Find("UI Plates").gameObject;
        healthBar = UIPlate.transform.Find("Health bar").gameObject;
        healthBar.GetComponent<UIBar>().SetMaxValue(MAX_ASTEROID_HEALTH);
        healthBar.GetComponent<UIBar>().SetValue(health.Value);
    }

    private void Update()
    {
        UIPlate.transform.position = gameObject.transform.position;
        if (health.Value <= 0.0f && IsServer)
        {
            // despawn object and spawn credit
            ulong netID = gameObject.transform.parent.GetComponent<NetworkObject>().NetworkObjectId;
            GameObject credit = GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Credit", gameObject.transform.position);
            credit.GetComponent<CurrencyItem>().setValue(credits);
            GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(netID);
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
        if (!IsServer && collision.collider.GetComponent<bulletProjectiles>().isPlayerBullet.Value)
        {
            float damage = collision.collider.GetComponent<bulletProjectiles>().getDamage();
            ulong bulletID = collision.collider.GetComponent<NetworkObject>().NetworkObjectId;
            dealDamageServerRpc(damage, bulletID);
        }

        //kill enemies that enter safe zone
        if (IsServer && collision.collider.gameObject.name == "Space Station")
        {
            health.Value = 0.0f;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void dealDamageServerRpc(float _damage, ulong _bulletID)
    {
        health.Value -= _damage;
        NetworkManager.SpawnManager.SpawnedObjects[_bulletID].gameObject.GetComponent<NetworkObject>().Despawn();
        updateHealthBarClientRpc();
    }

    [ClientRpc]
    private void updateHealthBarClientRpc()
    {
        Debug.Log("Changing healthbar of enemy");
        healthBar.GetComponent<UIBar>().SetValue(health.Value);
    }
}
