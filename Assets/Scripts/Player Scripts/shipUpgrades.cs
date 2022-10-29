using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shipUpgrades : MonoBehaviour
{
    private GameObject player;
    private float hullUpgradeCost = 100.0f;
    private float speedUpgradeCost = 100.0f;
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<playerController>().IsOwner)
            {
                player = players[i];
                Debug.Log("Owned player object was found!");
                break;
            }
        }

        if (player == null)
        {
            Debug.Log("Owned player object was not found!");
        }
    }

    public void upgradeHealth(float _healthIncrease)
    {
        if (player.GetComponent<playerController>().getCurrency() >= hullUpgradeCost)
        {
            float newMax = player.GetComponent<playerController>().getMaxHealth() + _healthIncrease;
            player.GetComponent<playerController>().setMaxHealth(newMax);
            player.GetComponent<playerController>().setHealth(newMax);

            player.GetComponent<playerController>().subtractCurrency(hullUpgradeCost);
        }
        
    }

    public void upgradeSpeed()
    {
        if (player.GetComponent<playerController>().getCurrency() >= speedUpgradeCost)
        {
            float newMax = player.GetComponent<playerController>().getMaxVelocity() + 0.25f;
            player.GetComponent<playerController>().setMaxVelocity(newMax);

            newMax = player.GetComponent<playerController>().getAcceleration() + 0.0825f;
            player.GetComponent<playerController>().setAcceleration(newMax);

            player.GetComponent<playerController>().subtractCurrency(speedUpgradeCost);
        }
    }
}
