using UnityEngine;
using System.Collections.Generic;
using Astroclash;

public class bulletWeapon : MonoBehaviour
{
    // Weapon Registrars 
    private List<float> weaponStats = new List<float>()
    {
        3.0f,   // Default rounds per second
        0.2f,   // Default angle of spread
        10.0f,  // Default projectile speed
        1.0f,   // Default damage
        10.0f,  // Default range
        1.0f,   // Default projectile count
        0.33f   // Default time between rounds in burst
    };
    private List<float> weaponStatsIncrement = new List<float>()
    {
        1.0f,
        0.1f,
        1.0f,
        1.0f,
        1.0f,
        1.0f,
        0.05f
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
    private List<string> stateNames = new List<string>()
    {
        "Shotgun",
        "Sniper",
        "Machine Gun"
    };

    public List<GameObject> stateUIObjects = new List<GameObject>();
    
    // State requirements
    StateRequirement sniperReq = new StateRequirement(
        "Sniper",
        new List<string>() {"Projectile Damage", "Projectile Speed", "Projectile Range"},
        new List<float>() {3.0f, 12.0f, 12.0f}
    );

    StateRequirement shotgunReq = new StateRequirement(
        "Shotgun",
        new List<string>() {"Shot Spread", "Projectile Count"},
        new List<float>() {0.4f, 3.0f}
    );

    StateRequirement machineGunReq = new StateRequirement(
        "Machine Gun",
        new List<string>() {"Fire Rate", "Projectile Count"},
        new List<float>() {5.0f, 3.0f}
    );
    

    // Default Weapon Bullet
    public GameObject defaultBulletPref;
    // Weapon upgrade UI object
    public GameObject weaponUI;
    public GameObject weaponUIButton;
    
    // Weapon Controller
    public WeaponController controller;

    
    
    // Weapon Specific Variables
    private bool burst = false;
    private float sumDeltaTime = 0.0f;
    private float burstCount = 0.0f;
    
    void Start()
    {
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

        //set state requirements
        controller.registerStateRequirement("Sniper", sniperReq);
        controller.registerStateRequirement("Shotgun", shotgunReq);
        controller.registerStateRequirement("Machine Gun", machineGunReq);

        controller.registerStateUI(stateUIObjects);
        controller.registerUpdgradeUI(weaponUI);
        controller.registerUpgradeUIButton(weaponUIButton);
    }
    
    void Update()
    {
        burstStep(); //controls burst logic
        controller.debugger(); //calls controller debugger to step each frame

        if (Input.GetMouseButton(0))
            shoot();

        //debug tools block
        if (controller.isDebug())
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            if (controller.getState("Machine Gun") != true && controller.getState("Shotgun") != true)
                Debug.DrawLine(transform.position, mouseRealtivePosition, Color.red);
            drawAngle(controller.getStat("Shot Spread"));
        }
    }
    
    // Does the shooting logic
    void shoot()
    {
        sumDeltaTime += Time.deltaTime;
        float secondsPerRound = 1.0f / controller.getStat("Fire Rate");

        if (sumDeltaTime >= secondsPerRound)
        {
            //machinegun path logic
            if (controller.getState("Machine Gun"))
            {
                //odd streams
                if (controller.getStat("Projectile Count") % 2 == 1 && controller.getStat("Projectile Count") != 1)
                {
                    float temp = controller.getStat("Projectile Count") + 2;
                    float rotationAngle = controller.getStat("Shot Spread") / (temp-1);
                    for (int i = 1; i < (temp - 1)/2; i++)
                    {
                        Vector3 posPoint = rotateVector(transform.right, rotationAngle * (i));
                        Vector3 negPoint = rotateVector(transform.right, -rotationAngle * (i));

                        //TODO: This needs to be replaced with networked objects
                        GameObject bullet1 = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                        GameObject bullet2 = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                        GameObject bullet3 = Instantiate(defaultBulletPref, transform.position, transform.rotation);

                        bullet1.GetComponent<Rigidbody2D>().AddForce(posPoint * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                        bullet1.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));

                        bullet2.GetComponent<Rigidbody2D>().AddForce(negPoint * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                        bullet2.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));

                        bullet3.GetComponent<Rigidbody2D>().AddForce(transform.right * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                        bullet3.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                    }
                }
                //even streams
                else if (controller.getStat("Projectile Count") % 2 == 0)
                {
                    if (controller.getStat("Projectile Count") > 2)
                    {
                        Vector3 point1 = rotateVector(transform.right, controller.getStat("Shot Spread")/2);
                        float rotationAngle = controller.getStat("Shot Spread") / controller.getStat("Projectile Count");
                        rotationAngle -= rotationAngle / controller.getStat("Projectile Count");
                        float temp = controller.getStat("Projectile Count") + 1;
                        for (int i = 1; i < temp; i++)
                        {
                            Vector3 negPoint = rotateVector(point1, -rotationAngle * (i));
                            //TODO: This needs to be replaced with networked objects
                            GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                            bullet.GetComponent<Rigidbody2D>().AddForce(negPoint * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                            bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                        }
                    }
                    else
                    {
                        float rotationAngle = controller.getStat("Shot Spread") / 5;
                        Vector3 posPoint = rotateVector(transform.right, rotationAngle);
                        Vector3 negPoint = rotateVector(transform.right, -rotationAngle);

                        //TODO: This needs to be replaced with networked objects
                        GameObject bullet1 = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                        GameObject bullet2 = Instantiate(defaultBulletPref, transform.position, transform.rotation);

                        bullet1.GetComponent<Rigidbody2D>().AddForce(posPoint * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                        bullet1.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));

                        bullet2.GetComponent<Rigidbody2D>().AddForce(negPoint * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                        bullet2.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                    }
                    
                }
                //single stream
                else
                {
                    //TODO: This needs to be replaced with networked objects
                    GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().AddForce(transform.right * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                    bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                }
            }
            //sniper path logic
            else if (controller.getState("Sniper") && burst == false)
            {
                burst = true;
            }
            //shotgun path logic
            else if (controller.getState("Shotgun"))
            {
                for (int i = 0; i < controller.getStat("Projectile Count"); i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    //TODO: This needs to be replaced with networked objects
                    GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().AddForce(fireVector * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                    bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                }
                for (int i = 0; i < controller.getStat("Projectile Count")/2; i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    //TODO: This needs to be replaced with networked objects
                    GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().AddForce(fireVector * controller.getStat("Projectile Speed") * 0.8f, ForceMode2D.Impulse);
                    bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                }
                for (int i = 0; i < controller.getStat("Projectile Count")/4; i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    //TODO: This needs to be replaced with networked objects
                    GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().AddForce(fireVector * controller.getStat("Projectile Speed") * 0.6f, ForceMode2D.Impulse);
                    bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                }
            }
            //basic gun logic
            else if (burst == false)
            {
                for (int i = 0; i < controller.getStat("Projectile Count"); i++)
                {
                    //randomize vector direction to fire based on spread angle
                    float randomAngle = Random.Range(-controller.getStat("Shot Spread")/2, controller.getStat("Shot Spread")/2);
                    Vector3 fireVector = rotateVector(transform.right, randomAngle);

                    //TODO: This needs to be replaced with networked objects
                    GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().AddForce(fireVector * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                    bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                }
            }

            //set time to fire
            sumDeltaTime = 0;
        }
    }
    
    // Debug functions
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
    void burstStep()
    {
        sumDeltaTime += Time.deltaTime;

        if (burst && controller.getState("Sniper"))
        {
            if (sumDeltaTime >= controller.getStat("Burst Timing") && burstCount < controller.getStat("Projectile Count"))
            {
                Debug.Log("Firing burst");
                //create bullet and send it
                //TODO: This needs to be replaced with networked objects
                GameObject bullet = Instantiate(defaultBulletPref, transform.position, transform.rotation);
                bullet.GetComponent<Rigidbody2D>().AddForce(transform.right * controller.getStat("Projectile Speed"), ForceMode2D.Impulse);
                bullet.GetComponent<bulletProjectiles>().setStats(controller.getStat("Projectile Range"), controller.getStat("Projectile Damage"));
                //reset sum delta
                sumDeltaTime = 0.0f;
                //increment burst count
                burstCount++;
            }
            else if (burstCount >= controller.getStat("Projectile Count"))
            {
                Debug.Log("burst ended");
                //set burst count to zero
                burstCount = 0.0f;
                //set burst to false
                burst = false;
            }
        }
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
    public void downgradeStat(string _statName)
    {
        controller.decreaseState(_statName);
    }
    public void setState(string _stateName)
    {
        controller.setState(_stateName);
    }
    public void toggleOff()
    {
        controller.UIToggleOff();
    }
    public void toggleOn()
    {
        controller.UIToggleOn();
    }
}