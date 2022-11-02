using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shipUpgrades : MonoBehaviour
{
    private GameObject player;
    private float hullUpgradeCost = 100.0f;
    private float speedUpgradeCost = 100.0f;
    private float repairUpgradeCost = 100.0f;


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

    public void upgradeSpeed(float _speedIncrement)
    {
        if (player.GetComponent<playerController>().getCurrency() >= speedUpgradeCost)
        {
            float newMax = player.GetComponent<playerController>().getMaxVelocity() + _speedIncrement;
            player.GetComponent<playerController>().setMaxVelocity(newMax);

            newMax = player.GetComponent<playerController>().getAcceleration() + (_speedIncrement * 0.33f);
            player.GetComponent<playerController>().setAcceleration(newMax);

            player.GetComponent<playerController>().subtractCurrency(speedUpgradeCost);
        }
    }

    public void upgradeRepair(float _repairIncrement)
    {
        if (player.GetComponent<playerController>().getCurrency() >= repairUpgradeCost)
        {
            float newRepair = player.GetComponent<playerController>().getRepair() + _repairIncrement;
            player.GetComponent<playerController>().setRepair(newRepair);

            player.GetComponent<playerController>().subtractCurrency(repairUpgradeCost);
        }
    }
}
