using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerUI : NetworkBehaviour
{
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> credits = new NetworkVariable<int>();

    //public HealthBar healthBar;
    public GameObject healthBar;
    public HealthBar playerHealth;

    void Start()
    {
        credits.Value = 0;
        healthBar = GameObject.Find("Health bar");
        playerHealth = healthBar.GetComponent<HealthBar>();
        currentHealth.Value = maxHealth;
        playerHealth.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(20);
        }
        playerHealth.SetHealth(currentHealth.Value);
    }

    void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;

        playerHealth.SetHealth(currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            SceneManager.LoadScene("DeathScreen");
        }
    }

    void PickupCredits(int amount)
    {
        credits.Value += amount;
    }
}
