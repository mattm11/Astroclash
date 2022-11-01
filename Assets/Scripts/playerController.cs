using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public class playerController : NetworkBehaviour
{

    //acceleration amount per second added to velocity. de-acceleration is 2:1 proportional to acceleration
    private float acceleration = 1.0f;
    //maximum speed of player ship
    private float maxVelocity = 3.0f;
    private float velocity = 0.0f;
    private Vector3 rotationDieOff = new Vector3(0.0f, 0.0f, 0.0f);

    // player health for UI
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public GameObject healthBar;
    public HealthBar playerHealth;

    public GameObject UILogic;
    private GameObject canvas;
    private GameObject eventSystem;
    private GameObject bulletweaponObject;
    private GameObject bulletUpgradeUI;
    private GameObject spaceStationUI;
    private GameObject spaceStationButton;

    private Camera playerCamera;

    void Start()
    {
        if (IsOwner)
        {
            GameObject parent = gameObject.transform.parent.gameObject;
            playerCamera = parent.GetComponentInChildren<Camera>();

            //Find canvas/even system
            canvas = gameObject.transform.Find("Canvas").gameObject;
            eventSystem = gameObject.transform.Find("EventSystem").gameObject;

            spaceStationUI = canvas.transform.Find("Space Station UI").gameObject;
            spaceStationButton = canvas.transform.Find("Space UI Button").gameObject;

            //Find health bar, initiate base health
            healthBar = GameObject.Find("Health bar");
            playerHealth = healthBar.GetComponent<HealthBar>();
            currentHealth.Value = maxHealth;
            playerHealth.SetMaxHealth(maxHealth);

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
        }
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
            playerCamera.transform.position = newPosition;

            //Movement is broken down into: X = Movement_Speed * cos(rotation_angle) Y = Movement_Speed * sin(rotation_angle)
            float angle = (float)(transform.eulerAngles.z * (Math.PI / 180));
            transform.position += new Vector3(velocity * (float)Math.Cos(angle), velocity * (float)Math.Sin(angle), 0) * Time.deltaTime;
            transform.Rotate(rotationDieOff * Time.deltaTime);
            rotationDieOff -= rotationDieOff * 0.50f * Time.deltaTime;

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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TakeDamage(20);
            }
            playerHealth.SetHealth(currentHealth.Value);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsOwner)
        {
            if (collider.gameObject.name == "Space Station")
                UILogic.GetComponent<UIRegistrar>().enableIndex(0);
        }

    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (IsOwner)
        {
            if (collider.gameObject.name == "Space Station")
                UILogic.GetComponent<UIRegistrar>().disableAll();
        }

    }

    private void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;

        playerHealth.SetHealth(currentHealth.Value);
    }
}