using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<playerController>().IsOwner)
            {
                player = players[i];
                break;
            }
        }
    }

    public void selfDestruct()
    {
        player.GetComponent<playerController>().setHealthFrame(-10.0f);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
