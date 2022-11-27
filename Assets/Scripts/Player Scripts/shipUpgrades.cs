using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class shipUpgrades : MonoBehaviour
{
    private GameObject player;
    public GameObject levelUI;
    private float hullUpgradeCost = 100.0f;
    private float speedUpgradeCost = 100.0f;
    private float repairUpgradeCost = 100.0f;
    private float capactiyUpgradeCost = 100.0f;
    private float rechargeUpgradeCost = 100.0f;

    private int shipLevel = 0;


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
            player.GetComponent<playerController>().setHealthServerRpc(newMax);

            player.GetComponent<playerController>().subtractCurrency(hullUpgradeCost);

            increaseLevel();
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

            increaseLevel();
        }
    }

    public void upgradeRepair(float _repairIncrement)
    {
        if (player.GetComponent<playerController>().getCurrency() >= repairUpgradeCost)
        {
            float newRepair = player.GetComponent<playerController>().getRepair() + _repairIncrement;
            player.GetComponent<playerController>().setRepair(newRepair);

            player.GetComponent<playerController>().subtractCurrency(repairUpgradeCost);

            increaseLevel();
        }
    }

    public void upgradeEnergyCapacity(float _energyCapacityIncrement)
    {
        if (player.GetComponent<playerController>().getCurrency() >= capactiyUpgradeCost)
        {
            float newCap = player.GetComponent<playerController>().getMaxEnergy() + _energyCapacityIncrement;
            player.GetComponent<playerController>().setEnergy(newCap);
            player.GetComponent<playerController>().setMaxEnergy(newCap);

            player.GetComponent<playerController>().subtractCurrency(capactiyUpgradeCost);

            increaseLevel();
        }
    }

    public void upgradeRecharge(float _rechargeIncrement)
    {
        if (player.GetComponent<playerController>().getCurrency() >= rechargeUpgradeCost)
        {
            float newRecharge = player.GetComponent<playerController>().getRechargeRate() + _rechargeIncrement;
            player.GetComponent<playerController>().setRechargeRate(newRecharge);

            player.GetComponent<playerController>().subtractCurrency(rechargeUpgradeCost);
            increaseLevel();
        }
    }

    public int getShipLevel()
    {
        return shipLevel;
    }

    private void increaseLevel()
    {
        shipLevel++;
        levelUI.GetComponent<TMP_Text>().text = "Lv. " + shipLevel.ToString();
    }
}
