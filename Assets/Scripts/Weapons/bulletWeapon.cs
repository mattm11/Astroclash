using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using Astroclash;

public class bulletWeapon : NetworkBehaviour
{
    // Weapon Registrars 
    private List<float> weaponStats = new List<float>()
    {
        2.0f,   // Default rounds per second
        0.2f,   // Default angle of spread
        5.0f,  // Default projectile speed
        5.0f,   // Default damage
        10.0f,  // Default range
        1.0f,   // Default projectile count
        0.15f   // Default time between rounds in burst
    };
    private List<float> weaponStatsIncrement = new List<float>()
    {
        0.1f,
        0.1f,
        0.5f,
        1.0f,
        1.0f,
        0.25f,
        0.00f
    };
    private List<float> weaponStatCosts = new List<float>()
    {
        100.0f,
        100.0f,
        100.0f,
        100.0f,
        100.0f,
        100.0f,
        100.0f
    };
    private List<string> statNames = new List<string>()
    {
        "Fire Rate",
        "Shot Spread",
        "Projectile Speed",
        "Projectile Damage",
        "Projectile Range",
        "Projectile Count",
        "Burst Timing"
    };
    private List<bool> states = new List<bool>()
    {
        false,
        false,
        false
    };
    private List<float> stateCosts = new List<float>()
    {
        1000.0f,
        1000.0f,
        1000.0f
    };
    private List<string> stateNames = new List<string>()
    {
        "Shotgun",
        "Sniper",
        "Machine Gun"
    };

    //UI element list for states
    public List<GameObject> stateUIObjects = new List<GameObject>();
    private GameObject upgradeUI = null;
    private GameObject canvas = null;
    
    // State requirements
    StateRequirement sniperReq = new StateRequirement(
        "Sniper",
        new List<string>() {"Projectile Damage", "Projectile Speed", "Projectile Range"},
        new List<float>() {10.0f, 7.5f, 15.0f}
    );

    StateRequirement shotgunReq = new StateRequirement(
        "Shotgun",
        new List<string>() {"Shot Spread", "Projectile Count"},
        new List<float>() {0.7f, 2.0f}
    );

    StateRequirement machineGunReq = new StateRequirement(
        "Machine Gun",
        new List<string>() {"Fire Rate", "Projectile Count"},
        new List<float>() {2.5f, 2.0f}
    );
    
    // Default Weapon Bullet
    public GameObject defaultBulletPref;
    public List<GameObject> otherBullets = new List<GameObject>();
    private GameObject player;
    
    // Weapon Controller
    private WeaponController controller = null;
    
    // Weapon Specific Variables
    private float sumDeltaTime = 0.0f;
    private float burstSumDeltaTime = 0.0f;
    private bool stateChange = false;

    void Start()
    {
        if (IsOwner)
        {     
            player = gameObject.transform.parent.gameObject;
            GameObject.Destroy(gameObject.GetComponent<NetworkObject>());

            //setup weapon controller
            controller = new WeaponController(
                weaponStats,
                weaponStatsIncrement,
                statNames,
                states,
                stateNames
            );

            //setting default bullet prefab
            controller.setBulletPrefab(defaultBulletPref);

            //set state requirements (state requirements need to have UI elements but don't need to be registered)
            controller.registerStateRequirement("Sniper", sniperReq);
            controller.registerStateRequirement("Shotgun", shotgunReq);
            controller.registerStateRequirement("Machine Gun", machineGunReq);

            //register UI elements and objects
            controller.registerCanvas(canvas);
            controller.registerUpdgradeUI(upgradeUI);

            controller.registerPlayer(gameObject.transform.parent.gameObject);
            controller.registerStatCosts(weaponStatCosts);
            controller.registerStateCosts(stateCosts);

            controller.instantiateUI();

            controller.registerUpgradeCounters();
        }
        
    }

    // Gun Specific Code
    void Update()
    {
        if (IsOwner && IsServer == false)
        {
            controller.step(); //calls controller debugger to step each frame as well as state management
            
            if (Input.GetKey(KeyCode.Space) && controller.getState("Sniper") != true)
            {
                shoot();
            }    
            else if (Input.GetKeyUp(KeyCode.Space) && controller.getState("Sniper") == true && player.GetComponent<playerController>().getEnergy() > 0.0f)
            {
                burst();
            }

            //debug tools block
            if (controller.isDebug())
            {
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
                Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                if (controller.getState("Machine Gun") != true && controller.getState("Shotgun") != true)
                    Debug.DrawLine(transform.position, mouseRealtivePosition, Color.red);
                drawAngle(controller.getStat("Shot Spread"));
            }

            // Upgrade changes and stat changes when selecting a new state
            if ((controller.getState("Shotgun") || controller.getState("Machine Gun") || controller.getState("Sniper")) && stateChange == false)
            {
                stateChange = true;
                
                //change stat-increments current stats
                if (controller.getState("Shotgun"))
                {
                    controller.setStat("Fire Rate", controller.getStat("Fire Rate") / 1.5f);
                    controller.setStat("Shot Spread", controller.getStat("Shot Spread") * 1.5f);
                    controller.setStat("Projectile Damage", controller.getStat("Projectile Damage") / 2.0f);

                    controller.setStatIncrement("Fire Rate", 0.05f);
                    controller.setStatIncrement("Shot Spread", 0.2f);
                    controller.setStatIncrement("Projectile Damage", 0.5f);
                    controller.setStatIncrement("Projectile Range", 0.5f);
                    controller.setStatIncrement("Projectile Count", 1.0f);
                }
                else if (controller.getState("Machine Gun"))
                {
                    controller.setStat("Fire Rate", controller.getStat("Fire Rate") * 1.1f);
                    controller.setStat("Shot Spread", controller.getStat("Shot Spread") * 3.0f);

                    controller.setStatIncrement("Fire Rate", 0.2f);
                    controller.setStatIncrement("Shot Spread", 0.2f);
                    controller.setStatIncrement("Projectile Damage", 0.25f);
                    controller.setStatIncrement("Projectile Range", 1.0f);
                    controller.setStatIncrement("Projectile Count", 0.15f);
                }
                else if (controller.getState("Sniper"))
                {
                    controller.setStat("Fire Rate", controller.getStat("Fire Rate") / 1.5f);
                    controller.setStat("Projectile Damage", controller.getStat("Projectile Damage") * 1.5f);
                    controller.setStat("Projectile Count", controller.getStat("Projectile Count") / 3.0f);

                    controller.setStatIncrement("Fire Rate", 0.05f);
                    controller.setStatIncrement("Shot Spread", 0.0f);
                    controller.setStatIncrement("Projectile Damage", 1.5f);
                    controller.setStatIncrement("Projectile Range", 2.0f);
                    controller.setStatIncrement("Projectile Count", 0.15f);
                }
            }
        }
    }    
    // shooting logic
    void shoot()
    {
        sumDeltaTime += Time.deltaTime;
        float secondsPerRound = 1.0f / controller.getStat("Fire Rate");
        float energyCost = (controller.getStat("Projectile Damage") * controller.getStat("Projectile Count") / 10.0f);
        float currEnergy = player.GetComponent<playerController>().getEnergy();

        float relativeBulletSpeed = controller.getStat("Projectile Speed") + player.GetComponent<playerController>().getVelocity();

        //this is so gross, maybe have the playerController hold this information
        ulong clientID = gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<NetworkObject>().NetworkObjectId;

        if (sumDeltaTime >= secondsPerRound && currEnergy > 0.0f)
        {
            if(currEnergy - energyCost < 0.0f)
            {
                //penality for over use of energy (3 seconds)
                float energyPenality = -3.0f * player.GetComponent<playerController>().getRechargeRate();
                player.GetComponent<playerController>().setEnergy(energyPenality);
            }
            else
            {
                player.GetComponent<playerController>().setEnergy(currEnergy - energyCost);
            }
            
            //machinegun path logic
            if (controller.getState("Machine Gun"))
            {
                int projectileCount = (int)controller.getStat("Projectile Count");

                //odd streams
                if (projectileCount % 2 == 1 && projectileCount != 1)
                {
                    float temp = projectileCount + 2;
                    float rotationAngle = controller.getStat("Shot Spread") / (temp-1);
                    for (int i = 1; i < (temp - 1)/2; i++)
                    {
                        Vector3 posPoint = rotateVector(transform.right, rotationAngle * (i));
                        Vector3 negPoint = rotateVector(transform.right, -rotationAngle * (i));

                        fireBulletServerRpc(
                            posPoint, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                        );

                        fireBulletServerRpc(
                            negPoint, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                        );

                        fireBulletServerRpc(
                            transform.right, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                        );
                    }
                }
                //even streams
                else if (projectileCount % 2 == 0)
                {
                    if (projectileCount > 2)
                    {
                        Vector3 point1 = rotateVector(transform.right, (controller.getStat("Shot Spread"))/2);
                        float rotationAngle = (controller.getStat("Shot Spread")) / projectileCount;
                        rotationAngle -= rotationAngle / projectileCount;
                        float temp = projectileCount + 1;
                        for (int i = 1; i < temp; i++)
                        {
                            Vector3 negPoint = rotateVector(point1, -rotationAngle * i);

                            fireBulletServerRpc(
                                negPoint, 
                                gameObject.transform.position, 
                                relativeBulletSpeed,
                                controller.getStat("Projectile Range"),
                                controller.getStat("Projectile Damage"),
                                clientID
                            );
                        }
                    }
                    else
                    {
                        float rotationAngle = (controller.getStat("Shot Spread") + (projectileCount * 0.1f)) / 5;
                        Vector3 posPoint = rotateVector(transform.right, rotationAngle);
                        Vector3 negPoint = rotateVector(transform.right, -rotationAngle);

                        fireBulletServerRpc(
                            posPoint, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                        );

                        fireBulletServerRpc(
                            negPoint, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                        );
                    }
                    
                }
                //single stream
                else
                {
                    fireBulletServerRpc(
                            transform.right, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                    );
                }
            }
            //shotgun path logic
            else if (controller.getState("Shotgun"))
            {
                for (int i = 0; i < controller.getStat("Projectile Count"); i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    fireBulletServerRpc(
                            fireVector, 
                            gameObject.transform.position, 
                            relativeBulletSpeed,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                    );
                }
                for (int i = 0; i < controller.getStat("Projectile Count")/2; i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    fireBulletServerRpc(
                            fireVector, 
                            gameObject.transform.position, 
                            relativeBulletSpeed * 0.80f,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                    );
                }
                for (int i = 0; i < controller.getStat("Projectile Count")/4; i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    fireBulletServerRpc(
                            fireVector, 
                            gameObject.transform.position, 
                            relativeBulletSpeed * 0.60f,
                            controller.getStat("Projectile Range"),
                            controller.getStat("Projectile Damage"),
                            clientID
                    );
                }
            }
            //basic gun logic
            else if (controller.getState("Shotgun") == false && controller.getState("Machine Gun") == false && controller.getState("Sniper") == false)
            {
                for (int i = 0; i < controller.getStat("Projectile Count"); i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    fireBulletServerRpc(
                        fireVector, 
                        gameObject.transform.position, 
                        relativeBulletSpeed,
                        controller.getStat("Projectile Range"),
                        controller.getStat("Projectile Damage"),
                        clientID
                    );
                }
            }

            //set time to fire
            sumDeltaTime = 0;
        }
    }   

    // Network Functions
    [ServerRpc]
    void fireBulletServerRpc(Vector3 _direction, Vector3 _position, float _projectileSpeed, float _projectileRange, float _projectileDamage, ulong _clientID)
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);

        GameObject bullet = null;
        bullet = Instantiate(defaultBulletPref, _position, rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(_direction * _projectileSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<bulletProjectiles>().setStats(_projectileRange, _projectileDamage);
        bullet.GetComponent<bulletProjectiles>().spawnerID.Value = _clientID;
        bullet.GetComponent<bulletProjectiles>().isPlayerBullet.Value = true;
        bullet.GetComponent<NetworkObject>().Spawn();

        ulong bulletID = bullet.GetComponent<NetworkObject>().NetworkObjectId;
        createBulletClientRpc(bulletID);

        //This needs to happen (server side ignoring collision)
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
    }

    [ClientRpc]
    void createBulletClientRpc(ulong _bulletID)
    {   
        //Should a client have the authority over this?
        GameObject bullet = NetworkManager.SpawnManager.SpawnedObjects[_bulletID].gameObject;
        ulong thisClientID = gameObject.transform.parent.transform.parent.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        ulong spawnerID = bullet.GetComponent<bulletProjectiles>().spawnerID.Value;
        
        if (IsOwner)
        {
            Debug.Log("Creating friendly bullet");
            bullet.tag = "friendlyBullet";
            Physics2D.IgnoreCollision(gameObject.transform.parent.gameObject.GetComponent<BoxCollider2D>(), bullet.GetComponent<CircleCollider2D>());
        }
        else
        {
            Debug.Log("Creating enemy bullet");
            bullet.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.25f, 0.25f);
            bullet.tag = "enemyBullet";
        }

        //This needs to happen (client side ignore collision)
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
    }

    // draws debug lines
    void drawAngle(float _angle)
    {
        float halfSpread = _angle / 2;
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector3 point1 = rotateVector(mouseRealtivePosition, halfSpread);
        Vector3 point2 = rotateVector(mouseRealtivePosition, -halfSpread);

        if (controller.getState("Sniper") == false)
        {
            Debug.DrawLine(transform.position, point1, Color.yellow);
            Debug.DrawLine(transform.position, point2, Color.yellow);
        }

        //debug mode
        if (controller.getState("Machine Gun"))
        {
            //odd streams
            if (controller.getStat("Projectile Count") % 2 == 1 && controller.getStat("Projectile Count") != 1)
            {
                float rotationAngle = controller.getStat("Shot Spread") / (controller.getStat("Projectile Count")-1);
                for (int i = 1; i < (controller.getStat("Projectile Count") + 1)/2; i++)
                {
                    Vector3 posPoint = rotateVector(mouseRealtivePosition, rotationAngle * (i));
                    Vector3 negPoint = rotateVector(mouseRealtivePosition, -rotationAngle * (i));
                    Debug.DrawLine(transform.position, posPoint, Color.red);
                    Debug.DrawLine(transform.position, negPoint, Color.red);
                    Debug.DrawLine(transform.position, mouseRealtivePosition, Color.red);
                }
            }
            //even streams
            else if (controller.getStat("Projectile Count") % 2 == 0)
            {
                if (controller.getStat("Projectile Count") > 2)
                {
                    float rotationAngle = controller.getStat("Shot Spread") / controller.getStat("Projectile Count");
                    rotationAngle -= rotationAngle / controller.getStat("Projectile Count");
                    for (int i = 1; i < controller.getStat("Projectile Count") + 1; i++)
                    {
                        Vector3 negPoint = rotateVector(point1, -rotationAngle * (i));
                        Debug.DrawLine(transform.position, negPoint, Color.red);
                    }
                }
                else
                {
                    float rotationAngle = controller.getStat("Shot Spread") / 5;
                    Vector3 posPoint = rotateVector(mouseRealtivePosition, rotationAngle);
                    Vector3 negPoint = rotateVector(mouseRealtivePosition, -rotationAngle);

                    Debug.DrawLine(transform.position, negPoint, Color.red);
                    Debug.DrawLine(transform.position, posPoint, Color.red);
                }
                
            }
            //single stream
            else
            {
                Debug.DrawLine(transform.position, mouseRealtivePosition, Color.red);
            }
        }

    }    
    //handles burst logic per frame
    void burst()
    {
        float energyCost = (controller.getStat("Projectile Damage") * controller.getStat("Projectile Count")) * (1.0f / controller.getStat("Fire Rate"));
        float currEnergy = player.GetComponent<playerController>().getEnergy();

        ulong clientID = gameObject.transform.parent.transform.parent.gameObject.GetComponent<NetworkObject>().NetworkObjectId;

        if(currEnergy - energyCost < 0.0f)
        {
            //penality for over use of energy (3 seconds)
            float energyPenality = -3.0f * player.GetComponent<playerController>().getRechargeRate();
            player.GetComponent<playerController>().setEnergy(energyPenality);
        }
        else
        {
            player.GetComponent<playerController>().setEnergy(currEnergy - energyCost);
        }

        float relativeBulletSpeed = controller.getStat("Projectile Speed") + player.GetComponent<playerController>().getVelocity();

        //ratio of damage
        float ratio = currEnergy / energyCost;
        float damage;
        if (ratio <= 1.0f)
        {
            damage = ratio * controller.getStat("Projectile Damage") * controller.getStat("Projectile Count");
        }
        else 
        {
            damage = controller.getStat("Projectile Damage") * controller.getStat("Projectile Count");
        }

        Debug.Log("Sniper Damage: " + damage);

        fireBulletServerRpc(
                transform.right, 
                gameObject.transform.position, 
                relativeBulletSpeed,
                controller.getStat("Projectile Range"),
                damage,
                clientID
        );
    }
    private Vector3 rotateVector(Vector3 vector, float angle)
    {
        Vector3 transVector = new Vector3(
            vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle),
            vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle),
            vector.z
        );

        return transVector;
    }

    //UI interface functions
    public void upgradeStat(string _statName)
    {
        controller.increaseStat(_statName);
    }
    public void setState(string _stateName)
    {
        controller.setState(_stateName);
    }

    // helper functions
    public WeaponController getController()
    {
        return controller;
    }
    public void setUpgradeUI(GameObject _ui)
    {  
        upgradeUI = _ui;
    }
    public void setCanvas(GameObject _canvas)
    {
        canvas = _canvas;
    }
}