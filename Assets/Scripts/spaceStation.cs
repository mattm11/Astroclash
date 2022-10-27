using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spaceStation : MonoBehaviour
{
    public GameObject spaceStationButton;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entering Space Station");
        spaceStationButton.SetActive(true);
    }

    // private void OnTriggerExit(Collider collider)
    // {
    //     Debug.Log("Exiting Space Station");
    //     spaceStationButton.SetActive(false);
    // }
}
