using Astroclash;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class CreateUser : MonoBehaviour
{
    public GameObject InputFieldGameObject;

    public void Start()
    {
        Debug.Log("Start: creating player!!!");
        createPlayer();
    }

    public void createPlayer()
    {
        Debug.Log("createPlayer has been called");

        // This changes between scenes, to make the string stay the same value must make the string static...????
        string text = InputFieldGameObject.GetComponent<TMP_InputField>().text;
        Debug.Log("Text from input field: " + text);
        int score = 0;

        Player.Instance.setUsername(text);
        Player.Instance.setScore(score);
        Player.Instance.saveJsonString();
        Debug.Log(Player.Instance.getUsername());
        Debug.Log(Player.Instance.getScore());
        Debug.Log(Player.Instance.getJsonString());

    }
}
