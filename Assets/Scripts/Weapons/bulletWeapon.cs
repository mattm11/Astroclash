using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Astroclash;

public class bulletWeapon : MonoBehaviour
{
    //Bullet aspects that can change (defaults)
    private float fireRate = 10.0f;               //Rounds per second
    private float fireSpread = 1.0f;        //Angle of spread in radians
    private float projectileSpeed = 10.0f;   //Projectile speed in units/second
    private float projectileDamage = 1.0f;       //Damage each projectile deals
    private float projectileRange = 20.0f;   //Projectile range until destroyed
    private float projectileCount = 1.0f;        //Count of projectiles fired
    public GameObject bulletPref;
    private List<float> weaponStats = new List<float>();
    private List<string> statNames = new List<string>()
    {
        "Fire Rate",
        "Shot Spead",
        "Projectile Speed",
        "Projectile Damage",
        "Projectile Range",
        "Projectile Count"
    };
    private weaponDebugUI debugUI;
    private bool debug = false;
    private float sumDeltaTime = 0.0f;

    void Start()
    {
        weaponStats.Add(fireRate);
        weaponStats.Add(fireSpread);
        weaponStats.Add(projectileSpeed);
        weaponStats.Add(projectileDamage);
        weaponStats.Add(projectileRange);
        weaponStats.Add(projectileCount);

        debugUI = new weaponDebugUI(weaponStats, statNames, this.GetComponent<bulletWeapon>());    
    }

    // Update is called once per frame
    void Update()
    {
        //just simulates the rotation of player characters (not needed for the actual weapon) 
        rotatePlayer();

        if (Input.GetMouseButton(0))
        {
            Debug.Log("Shooting!");
            shoot();
        }

        //debug tools block
        if (debug)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.DrawLine(transform.position, mouseRealtivePosition, Color.red);
            drawAngle(fireSpread);
            setWeaponStats();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (debug)
            {
                Debug.Log("Debug Mode Deactivated");
                debug = false;
            }
            else
            {
                Debug.Log("Debug Mode Activated");
                debug = true;
            }

            debugUI.toggle();
        }
    }

    void rotatePlayer()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 direction = mouseRealtivePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);
        transform.rotation = rotation;
    }
    void shoot()
    {
        sumDeltaTime += Time.deltaTime;
        float secondsPerRound = 1.0f / fireRate;
        Debug.Log(secondsPerRound);

        if (sumDeltaTime >= secondsPerRound)
        {
            sumDeltaTime = 0;
            GameObject bullet = Instantiate(bulletPref, transform.position, transform.rotation);
            bullet.GetComponent<Rigidbody2D>().AddForce(transform.right * projectileSpeed, ForceMode2D.Impulse);

        }
    }
    // Debug functions
    void drawAngle(float _angle)
    {
        float halfSpread = _angle / 2;
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector3 point1 = new Vector3(
            (Mathf.Cos(halfSpread) * mouseRealtivePosition.x) - (Mathf.Sin(halfSpread) * mouseRealtivePosition.y),
            (Mathf.Sin(halfSpread) * mouseRealtivePosition.x) + (Mathf.Cos(halfSpread) * mouseRealtivePosition.y),
            mouseRealtivePosition.z
        );

        Vector3 point2 = new Vector3(
            (Mathf.Cos(-halfSpread) * mouseRealtivePosition.x) - (Mathf.Sin(-halfSpread) * mouseRealtivePosition.y),
            (Mathf.Sin(-halfSpread) * mouseRealtivePosition.x) + (Mathf.Cos(-halfSpread) * mouseRealtivePosition.y),
            mouseRealtivePosition.z
        );

        Debug.DrawLine(transform.position, point1, Color.yellow);
        Debug.DrawLine(transform.position, point2, Color.yellow);
    }
    public void setWeaponStats()
    {
        List<string> input = debugUI.getInput();
        fireRate = float.Parse(input[0]);               
        fireSpread = float.Parse(input[1]);        
        projectileSpeed = float.Parse(input[2]);   
        projectileDamage = float.Parse(input[3]);       
        projectileRange = float.Parse(input[4]);   
        projectileCount = float.Parse(input[5]);        
    }
}
