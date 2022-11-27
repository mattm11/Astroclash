using Astroclash;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class PostUserData : MonoBehaviour
{
    private int score;

    public void Start()
    {
        score = Player.Instance.getScore();
        Debug.Log("Start: begin coroutine");
        StartCoroutine(postRequest());
    }

    /*void Update()
    {
        if (Player.Instance.getScore() >= score+100 || Player.Instance.getScore() <= score + 100)
        {
            score = Player.Instance.getScore();
            StartCoroutine(postRequest());
        }
    }*/

    IEnumerator postRequest()
    {
        while (true)
        {
            Debug.Log("postRequest has been called");
            //string json = Player.Instance.saveToString();
            //Debug.Log(Player.Instance.saveToString());
            Debug.Log(Player.Instance.getUsername());
            Debug.Log(Player.Instance.getScore());
            Debug.Log(Player.Instance.getJsonString());
            //string JSONresult = JsonConvert.SerializeObject(Player.Instance);
            //string Json = JSON.stringify(Player.Instance);
            //string json = @"{""body"": {""username"":""" + Player.Instance.getUsername() + @""", ""score"":""0""} }";
            //Debug.Log(JSONresult);
            //Debug.Log(json);
            Player.Instance.saveJsonString();
            Debug.Log(Player.Instance.getJsonString());
            //string json = Player.Instance.getJsonString();
            string json = @"{""body"":" + Player.Instance.getJsonString() + @"}";
            Debug.Log(json);

            var uwr = new UnityWebRequest("https://lhmtbss7qi.execute-api.us-east-1.amazonaws.com/default/addUser", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Debug.Log("Error while sending: " + uwr.error);
            }
            else
            {
                Debug.Log("User has successfully been added!");
            }

            yield return new WaitForSeconds(10.0f);
            /*if (Player.Instance.getScore() >= score + 100 || Player.Instance.getScore() <= score + 100)
            {
                score = Player.Instance.getScore();
                StartCoroutine(postRequest());
            }*/
        }
    }
}
