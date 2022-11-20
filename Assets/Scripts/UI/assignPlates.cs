using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class assignPlates : NetworkBehaviour
{
    GameObject UIplates;
    GameObject owner;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<playerController>().IsOwner)
            {
                owner = players[i];
            }
        }

        if (!IsOwner)
            gameObject.GetComponent<Canvas>().worldCamera = owner.GetComponent<Camera>();

        if (IsOwner)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
