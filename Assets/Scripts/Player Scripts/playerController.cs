using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public class playerController : NetworkBehaviour
{

    //acceleration amount per second added to velocity. de-acceleration is 2:1 proportional to acceleration
    public float acceleration = 1.0f;
    //maximum speed of player ship
    public float maxVelocity = 3.0f;
    public float velocity = 0.0f;
    private Vector3 rotationDieOff = new Vector3(0.0f, 0.0f, 0.0f);

    public float money = 0.0f;
    public float health = 100.0f;
    private float maxHealth = 100.0f;
    public float repairAmount = 1.0f; //hull repaired passively per second

    public GameObject UILogic;
    private GameObject canvas;
    private GameObject eventSystem;
    private GameObject bulletweaponObject;
    private GameObject bulletUpgradeUI;
    private GameObject spaceStationUI;
    private GameObject shipUpgradeUI;
    private GameObject spaceStationButton;
    private GameObject currencyUI;

    public List<GameObject> weaponRegistry = new List<GameObject>();

    private Camera playerCamera;

    void Start()
    {
        Physics2D.IgnoreLayerCollision(6,7);

        if (IsOwner)
        {
            GameObject parent = gameObject.transform.parent.gameObject;
            playerCamera = parent.GetComponentInChildren<Camera>();

            //Find canvas/even system
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

            //Find weapon objects
            bulletweaponObject = gameObject.transform.Find("BulletWeapon").gameObject;

            //Find weapon object UI
            bulletUpgradeUI = canvas.transform.Find("BulletUpgradeUI").gameObject;

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
        }
        // Remove other non-client player's UI elements and event system
        else
        {
            GameObject.Destroy(gameObject.transform.Find("Canvas").gameObject);
            GameObject.Destroy(gameObject.transform.Find("EventSystem").gameObject);
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (IsOwner) 
        {
            //update the camera origin
            Vector3 newPosition = playerCamera.transform.position;
            newPosition.x = gameObject.transform.position.x;
            newPosition.y = gameObject.transform.position.y;
            playerCamera.transform.position= newPosition;

            //Movement is broken down into: X = Movement_Speed * cos(rotation_angle) Y = Movement_Speed * sin(rotation_angle)
            float angle = (float)(transform.eulerAngles.z * (Math.PI / 180));
            transform.position += new Vector3(velocity * (float)Math.Cos(angle), velocity * (float)Math.Sin(angle), 0) * Time.deltaTime;
            transform.Rotate(rotationDieOff * Time.deltaTime);
            rotationDieOff -= rotationDieOff * 0.50f * Time.deltaTime;

            if (Input.GetKey(KeyCode.W))
            {
                //adding player speed, until max speed is reached
                if(velocity + acceleration <= maxVelocity)
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
                if((velocity - acceleration) > 0)
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

            currencyUI.GetComponent<TMP_Text>().text = money.ToString();
            repair();
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
            takeDamage(collider.gameObject.GetComponent<bulletProjectiles>().getDamage());
        }

        // logic to destroy the bullet that collided with other objects
        if (collider.gameObject.tag == "friendlyBullet" && IsOwner == false)
        {
            GameObject.Destroy(collider.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (IsOwner)
        {
            if (collider.gameObject.name == "Space Station")
            UILogic.GetComponent<UIRegistrar>().disableAll();
            enableWeapons();
        }
        
    }

    private void takeDamage(float damage)
    {
        if (health - damage <= 0.0f)
        {
            health = 0.0f;
            //Death logic
        }
        else
        {
            health -= damage;
        }
    }

    private void repair()
    {
        if (health + (repairAmount * Time.deltaTime) <= maxHealth)
        {
            health += repairAmount * Time.deltaTime;
        }
        else if (health + (repairAmount * Time.deltaTime) > maxHealth)
        {
            health = maxHealth;
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
        return health;
    }
    public float getMaxHealth()
    {
        return maxHealth;
    }
    public void setHealth(float _health)
    {
        health = _health;
    }
    public void setMaxHealth(float _maxHealth)
    {
        maxHealth = _maxHealth;
    }
    public float getMaxVelocity()
    {
        return maxVelocity;
    }
    public void setMaxVelocity(float _maxVelocity)
    {
        maxVelocity = _maxVelocity;
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
}