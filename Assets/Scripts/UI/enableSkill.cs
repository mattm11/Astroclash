using UnityEngine;
using UnityEngine.UI;

public class enableSkill : MonoBehaviour
{
    public GameObject weapon_object;
    public GameObject sniperButton;
    public GameObject shoutgunButton;
    public GameObject machinegunButton;
    private bulletWeapon weapon;
    // Start is called before the first frame update
    void Start()
    {
        weapon = weapon_object.GetComponent<bulletWeapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon.damageUpgrades >= 4)
        {
            if (weapon.isMachinegun == false && weapon.isSniper == false && weapon.isShotgun == false)
            {
                //enable sniper button
                sniperButton.GetComponent<Image>().color = new Color(
                    sniperButton.GetComponent<Image>().color.r,
                    sniperButton.GetComponent<Image>().color.g,
                    sniperButton.GetComponent<Image>().color.b,
                    1.0f
                );
                
                sniperButton.GetComponent<Button>().enabled = true;
            }
            else if (weapon.isSniper)
            {
                sniperButton.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                sniperButton.GetComponent<Button>().enabled = false;
            }
            else
            {
                sniperButton.GetComponent<Image>().color = new Color(
                    sniperButton.GetComponent<Image>().color.r,
                    sniperButton.GetComponent<Image>().color.g,
                    sniperButton.GetComponent<Image>().color.b,
                    0.25f
                );

                sniperButton.GetComponent<Button>().enabled = true;
            }
        }
        
        if (weapon.damageUpgrades >= 2 && weapon.countUpgrades >= 2)
        {
            if (weapon.isMachinegun == false && weapon.isSniper == false && weapon.isShotgun == false)
            {
                //enable sniper button
                machinegunButton.GetComponent<Image>().color = new Color(
                    machinegunButton.GetComponent<Image>().color.r,
                    machinegunButton.GetComponent<Image>().color.g,
                    machinegunButton.GetComponent<Image>().color.b,
                    1.0f
                );
                
                machinegunButton.GetComponent<Button>().enabled = true;
            }
            else if (weapon.isMachinegun)
            {
                machinegunButton.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                machinegunButton.GetComponent<Button>().enabled = false;
            }
            else
            {
                machinegunButton.GetComponent<Image>().color = new Color(
                    machinegunButton.GetComponent<Image>().color.r,
                    machinegunButton.GetComponent<Image>().color.g,
                    machinegunButton.GetComponent<Image>().color.b,
                    0.25f
                );

                machinegunButton.GetComponent<Button>().enabled = true;
            }
        }
        
        if (weapon.countUpgrades >= 2 && weapon.spreadUpgrades >= 2)
        {
            if (weapon.isMachinegun == false && weapon.isSniper == false && weapon.isShotgun == false)
            {
                //enable sniper button
                shoutgunButton.GetComponent<Image>().color = new Color(
                    shoutgunButton.GetComponent<Image>().color.r,
                    shoutgunButton.GetComponent<Image>().color.g,
                    shoutgunButton.GetComponent<Image>().color.b,
                    1.0f
                );
                
                shoutgunButton.GetComponent<Button>().enabled = true;
            }
            else if (weapon.isShotgun)
            {
                shoutgunButton.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                shoutgunButton.GetComponent<Button>().enabled = false;
            }
            else
            {
                shoutgunButton.GetComponent<Image>().color = new Color(
                    shoutgunButton.GetComponent<Image>().color.r,
                    shoutgunButton.GetComponent<Image>().color.g,
                    shoutgunButton.GetComponent<Image>().color.b,
                    0.25f
                );

                shoutgunButton.GetComponent<Button>().enabled = true;
            }
        } 
    }
}
