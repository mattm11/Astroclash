using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class updateNumber : MonoBehaviour
{
    public GameObject weapon_object;
    private bulletWeapon weapon;
    // Start is called before the first frame update
    void Start()
    {
        weapon = weapon_object.GetComponent<bulletWeapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.name == "Damage+")
        {
            gameObject.GetComponent<TMP_Text>().text = weapon.damageUpgrades.ToString();
        }
        else if (gameObject.name == "Spread+")
        {
            gameObject.GetComponent<TMP_Text>().text = weapon.spreadUpgrades.ToString();
        }
        else if (gameObject.name == "BulletCount+")
        {
            gameObject.GetComponent<TMP_Text>().text = weapon.countUpgrades.ToString();
        }
        
    }
}
