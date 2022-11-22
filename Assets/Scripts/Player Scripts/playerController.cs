using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Collections;

public class playerController : NetworkBehaviour
{

    //acceleration amount per second added to velocity. de-acceleration is 2:1 proportional to acceleration
    private float acceleration = 1.0f;
    //maximum speed of player ship
    private float maxVelocity = 3.0f;
    private float velocity = 0.0f;
    private Vector3 rotationDieOff = new Vector3(0.0f, 0.0f, 0.0f);
    private int debrisAmount = 3;

    private float money = 10000.0f;
    private float repairAmount = 1.0f; //hull repaired passively per second
    private float rechargeRate = 10.0f;

    public GameObject UILogic;
    private GameObject canvas;
    private GameObject eventSystem;
    private GameObject bulletweaponObject;
    private GameObject bulletUpgradeUI;
    private GameObject spaceStationUI;
    private GameObject shipUpgradeUI;
    private GameObject spaceStationButton;
    private GameObject currencyUI;
    private GameObject energyBar;
    private GameObject levelUI;
    private GameObject escapeUI;

    public List<GameObject> weaponRegistry = new List<GameObject>();
    private GameObject healthBar;

    private GameObject UIplates;
    private GameObject healthPlate;
    private GameObject namePlate;

    // player health for UI

    private const float DEFAULT_MAX_HEALTH = 100;
    private float maxHealth = DEFAULT_MAX_HEALTH;
    // public float health = DEFAULT_MAX_HEALTH;
    public NetworkVariable<float> health = new NetworkVariable<float>(DEFAULT_MAX_HEALTH);
    private float healthFrameValue;
    private bool inCombat = false;
    private bool countDownStaterted = false;
    private float combatTimer = 0.0f;

    // player energy for UI
    private const float DEFAULT_MAX_ENERGY = 20.0f;
    private float maxEnergy = DEFAULT_MAX_ENERGY;
    public float energy = DEFAULT_MAX_ENERGY;

    public Camera playerCamera;
    public static string playerName;
    public NetworkVariable<FixedString64Bytes> networkName = new NetworkVariable<FixedString64Bytes>();

    void Start()
    {
        UIplates = gameObject.transform.parent.transform.Find("UI Plates").gameObject;
        // Find nameplate, give name
        namePlate = UIplates.transform.Find("Nameplate").gameObject;
        if (IsOwner)
            setNameServerRpc(playerName);
        // Find health plate and nameplate
        healthPlate = UIplates.transform.Find("Health bar").gameObject;
        healthPlate.GetComponent<UIBar>().SetValue(maxHealth);

        if (IsOwner)
        {
            UIplates.SetActive(false);

            healthFrameValue = health.Value;
            GameObject parent = gameObject.transform.parent.gameObject;
            playerCamera = parent.GetComponentInChildren<Camera>();
            if (playerCamera == null)
                Debug.Log("Player camera is null!");

            //Find canvas/event system
            canvas = gameObject.transform.Find("Canvas").gameObject;
            eventSystem = gameObject.transform.Find("EventSystem").gameObject;

            canvas.GetComponent<Canvas>().worldCamera = playerCamera;
            canvas.GetComponent<Canvas>().planeDistance = 10;

            //General UI (Health, Currency, ETC.)
            currencyUI = canvas.transform.Find("Currency UI").gameObject;

            //Find space station UI
            spaceStationUI = canvas.transform.Find("Space Station UI").gameObject;
            spaceStationButton = canvas.transform.Find("Space UI Button").gameObject;
            shipUpgradeUI = canvas.transform.Find("ShipUpgradeUI").gameObject;

            //Find health bar, give base health
            //healthBar = GameObject.Find("Health bar");
            healthBar = canvas.transform.Find("Health bar").gameObject;
            healthBar.GetComponent<UIBar>().SetMaxValue(maxHealth);

            energyBar = canvas.transform.Find("Energy Bar").gameObject;
            energyBar.GetComponent<UIBar>().SetMaxValue(maxEnergy);
            //Find weapon objects
            bulletweaponObject = gameObject.transform.Find("BulletWeapon").gameObject;

            //Find weapon object UI
            bulletUpgradeUI = canvas.transform.Find("BulletUpgradeUI").gameObject;

            levelUI = canvas.transform.Find("Level").gameObject;

            escapeUI = canvas.transform.Find("Escape Menu").gameObject;

            bulletweaponObject.GetComponent<bulletWeapon>().setUpgradeUI(bulletUpgradeUI);
            bulletweaponObject.GetComponent<bulletWeapon>().setCanvas(canvas);  //Sets the canvas object to draw debug

            //Find the UILogic object
            UILogic = canvas.transform.Find("UILogic").gameObject;

            //setup and turn off canvas and event system
            canvas.transform.SetParent(null);
            eventSystem.transform.SetParent(null);

            // turn off UI elements by default
            spaceStationUI.SetActive(false);
            spaceStationButton.SetActive(false);
            bulletUpgradeUI.SetActive(false);
            shipUpgradeUI.SetActive(false);
            escapeUI.SetActive(false);
        }
        // Remove other non-client player's UI elements and event system
        else if (!IsServer && !IsOwner)
        {
            gameObject.transform.Find("Canvas").gameObject.SetActive(false);
            gameObject.transform.Find("EventSystem").gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        namePlate.GetComponent<TMP_Text>().text = networkName.Value.ToString();
        // update UI plates to follow camera
        UIplates.transform.position = gameObject.transform.position;
        healthPlate.GetComponent<UIBar>().SetValue(health.Value);

        if (IsOwner)
        {
            //update the camera origin
            Vector3 newPosition = playerCamera.transform.position;
            newPosition.x = gameObject.transform.position.x;
            newPosition.y = gameObject.transform.position.y;
            playerCamera.transform.position = newPosition;

            //Movement is broken down into: X = Movement_Speed * cos(rotation_angle) Y = Movement_Speed * sin(rotation_angle)
            float angle = (float)(transform.eulerAngles.z * (Math.PI / 180));
            transform.position += new Vector3(velocity * (float)Math.Cos(angle), velocity * (float)Math.Sin(angle), 0) * Time.deltaTime;
            transform.Rotate(rotationDieOff * Time.deltaTime);
            rotationDieOff -= rotationDieOff * 0.50f * Time.deltaTime;

            int shipLevel = UILogic.GetComponent<shipUpgrades>().getShipLevel();
            int bulletWeaponLevel = bulletweaponObject.GetComponent<bulletWeapon>().getController().getWeaponLevel();

            int totalLevel = shipLevel + bulletWeaponLevel;

            levelUI.GetComponent<TMP_Text>().text = "Lv. " + totalLevel.ToString();

            if (Input.GetKey(KeyCode.W))
            {
                //adding player speed, until max speed is reached
                if (velocity + acceleration <= maxVelocity)
                {
                    velocity += acceleration * Time.deltaTime;
                }
                else if ((velocity + acceleration) >= maxVelocity && velocity + 0.25f <= maxVelocity)
                {
                    velocity += 0.25f * Time.deltaTime;
                }
                else
                {
                    velocity = maxVelocity;
                }
            }
            //Slow down
            else if (Input.GetKey(KeyCode.S))
            {
                if ((velocity - acceleration) > 0)
                {
                    velocity -= (acceleration) * Time.deltaTime;
                }
                else if ((velocity - acceleration) < 0 && velocity - 0.25f >= 0)
                {
                    velocity -= 0.25f * Time.deltaTime;
                }
                else
                {
                    velocity = 0.0f;
                }
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                escapeUI.SetActive(true);
            }

            currencyUI.GetComponent<TMP_Text>().text = money.ToString();
            if (inCombat && countDownStaterted == false)
            {
                Debug.Log("In Combat!");
                StartCoroutine(combatTimerRoutine());
                countDownStaterted = true;
            }
            else if (!inCombat)
            {
                Debug.Log("Out of Combat!");
                repair();
            }
            

            if (!Input.GetKey(KeyCode.Space))
            {
                recharge();
            }

            healthBar.GetComponent<UIBar>().SetValue(health.Value);

            Debug.Log("Health Frame: " + healthFrameValue);
            if (healthFrameValue != health.Value)
            {
                setHealthServerRpc(healthFrameValue);
            }
            
            checkDeath();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsOwner)
        {
            if (collider.gameObject.name == "Space Station")
            {
                UILogic.GetComponent<UIRegistrar>().enableIndex(0);
                disableWeapons();
            }
        }

        // bullet weapon interaction logic
        if (collider.gameObject.tag == "enemyBullet" && IsOwner)
        {
            GameObject.Destroy(collider.gameObject);
            TakeDamage(collider.gameObject.GetComponent<bulletProjectiles>().getDamage());
            inCombat = true;
        }
        else if 
        (
            collider.gameObject.tag == "enemyBullet" && IsOwner == false 
            && gameObject.transform.parent.gameObject.GetComponent<NetworkObject>().NetworkObjectId != collider.gameObject.GetComponent<bulletProjectiles>().getSpawnerID()
        )
        {
            GameObject.Destroy(collider.gameObject);
        }

        // logic to destroy the bullet that collided with other objects
        if (collider.gameObject.tag == "friendlyBullet" && IsOwner == false)
        {
            GameObject.Destroy(collider.gameObject);
            Debug.Log("Deleting friendly bullet!");
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (IsOwner)
        {
            if (collider.gameObject.name == "Space Station")
            {
                UILogic.GetComponent<UIRegistrar>().disableAll();
                enableWeapons();
            }
        }
    }

    private void TakeDamage(float damage)
    {
        healthFrameValue -= damage;
        inCombat = true;
        countDownStaterted = false;
        combatTimer = 0.0f;

        healthBar.GetComponent<UIBar>().SetValue(healthFrameValue);

        checkDeath();
    }

    private IEnumerator combatTimerRoutine()
    {
        while (combatTimer <= 5.0f)
        {
            combatTimer += Time.deltaTime;
            Debug.Log("Combat Timer: " + combatTimer);
            yield return null;
        }
        inCombat = false;
        countDownStaterted = false;
    }

    private IEnumerator deathTimerRoutine()
    {
        float deathTimer = 0.0f;
        while (deathTimer <= 3.0f)
        {
            deathTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("De-Spawning player");
        //SceneManager.LoadScene("DeathScreen");
        ulong clientID = gameObject.transform.parent.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        ulong objectID = gameObject.transform.parent.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        despawnPlayerServerRpc(clientID, objectID);
    }
    
    private void repair()
    {
        if (healthFrameValue + (repairAmount * Time.deltaTime) <= maxHealth)
        {
            healthFrameValue += (repairAmount * 10.0f *  Time.deltaTime);
        }
        else if (health.Value + (repairAmount * Time.deltaTime) > maxHealth && healthFrameValue != maxHealth)
        {
            healthFrameValue = maxHealth;
        }
    }
    private void recharge()
    {
        if (energy + (rechargeRate * Time.deltaTime) <= maxEnergy)
        {
            energy += rechargeRate * Time.deltaTime;
            setEnergy(energy);
        }
        else if (energy + (rechargeRate * Time.deltaTime) <= maxEnergy)
        {
            energy = maxEnergy;
            setEnergy(energy);
        }
    }
    private void disableWeapons()
    {
        for (int i = 0; i < weaponRegistry.Count; i++)
        {
            weaponRegistry[i].SetActive(false);
        }
    }
    private void enableWeapons()
    {
        for (int i = 0; i < weaponRegistry.Count; i++)
        {
            weaponRegistry[i].SetActive(true);
        }
    }
    private void checkDeath()
    {
        if (healthFrameValue <= 0)
        {
            spawnDebrisServerRpc(gameObject.transform.position);
            gameObject.GetComponent<playerController>().enabled = false;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            canvas.SetActive(false);
            StartCoroutine(deathTimerRoutine());
        }
    }

    public void setHealthFrame(float _frame)
    {
        healthFrameValue = _frame;
    }

    // Setters and Getters
    public float getCurrency()
    {
        return money;
    }
    public void addCurrency(float _money)
    {
        money += _money;
    }
    public void subtractCurrency(float _money)
    {
        money -= _money;
    }
    public float getHealth()
    {
        return health.Value;
    }
    public float getMaxHealth()
    {
        return maxHealth;
    }
    [ServerRpc]
    public void setHealthServerRpc(float _health)
    {
        health.Value = _health;
    }
    public void setMaxHealth(float _maxHealth)
    {
        maxHealth = _maxHealth;
        healthBar.GetComponent<UIBar>().SetMaxValue(maxHealth);
        healthBar.GetComponent<UIBar>().increaseBar();
    }
    public float getMaxVelocity()
    {
        return maxVelocity;
    }
    public void setMaxVelocity(float _maxVelocity)
    {
        maxVelocity = _maxVelocity;
    }
    public float getVelocity()
    {
        return velocity;
    }
    public float getAcceleration()
    {
        return acceleration;
    }
    public void setAcceleration(float _acceleration)
    {
        acceleration = _acceleration;
    }
    public float getRepair()
    {
        return repairAmount;
    }
    public void setRepair(float _repairAmount)
    {
        repairAmount = _repairAmount;
    }
    [ServerRpc]
    public void setNameServerRpc(string name)
    {
        networkName.Value = name;
    }
    public void setEnergy(float _energy)
    {
        energy = _energy;
        energyBar.GetComponent<UIBar>().SetValue(energy);
    }
    public float getEnergy()
    {
        return energy;
    }
    public float getMaxEnergy()
    {
        return maxEnergy;
    }
    public void setMaxEnergy(float _maxEnergy)
    {
        maxEnergy = _maxEnergy;
        energyBar.GetComponent<UIBar>().SetMaxValue(maxEnergy);
        energyBar.GetComponent<UIBar>().increaseBar();
    }
    public void setRechargeRate(float _recharge)
    {
        rechargeRate = _recharge;
    }
    public float getRechargeRate()
    {
        return rechargeRate;
    }

    //handles player despawn sync
    [ServerRpc]
    private void spawnDebrisServerRpc(Vector3 _position)
    {
        spawnDebrisClientRpc(_position);
    }
    [ServerRpc]
    private void despawnPlayerServerRpc(ulong _clientID, ulong _objectID)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { _clientID }
            }
        };
        loadDeathSceneClientRpc(clientRpcParams);
        GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(_objectID);
    }
    [ClientRpc]
    private void spawnDebrisClientRpc(Vector3 _position)
    {
        UnityEngine.Object debris = Resources.Load("prefabs/Player Debris");
        for (int i = 0; i < debrisAmount; i++)
        {
            Instantiate(debris, _position, Quaternion.identity);
        }
    }
    [ClientRpc]
    private void loadDeathSceneClientRpc(ClientRpcParams clientRpcParams = default)
    {
        NetworkManager.Singleton.Shutdown();
        // SceneManager.LoadScene("DeathScreen");
        SceneManager.LoadScene("DeathScreen");
        GameObject.Destroy(GameObject.Find("Network Manager"));
    }
}