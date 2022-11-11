using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Astroclash;
using TMPro;

using UnityEngine.Networking;
using UnityEngine.Events;
using System;

public class ScoreBoard : MonoBehaviour
{
    public List<GameObject> positions;
    // Start is called before the first frame update
    void Start()
    {
        //TODO: replace with database interface function to get struct
        StartCoroutine(databaseQuery());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //update function to call when scoreboard needs to dynamically be changed
    public void updateScores(QueryResult _results)
    {
        List<string> names = _results.getUserNames();
        List<int> scores = _results.getPlayerScores();
        for (int i = 0; i < 3; i++)
        {
            positions[i].GetComponent<TMP_Text>().text = names[i];
            positions[i].transform.Find("Score").GetComponent<TMP_Text>().text = scores[i].ToString();
        }
    }

    //called from player controller when player dies
    public void postScore(string _userName, int _score)
    {
        //TODO: call backend interface with username and score as input
    }

    private void getScores()
    {
        //TODO: interface of the score board called from player controller to get new scores from database
        /*QueryResult result = databaseQuery();
        updateScores(result);*/
    }

    //Dummy function for now should be replaced with the real backend API call
    public IEnumerator databaseQuery()
    {
        Debug.Log("getRequest has been called");

        var uwr = new UnityWebRequest("https://lhmtbss7qi.execute-api.us-east-1.amazonaws.com/default/addUser", "GET");
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            string response = uwr.downloadHandler.text;

            // gets the data of the top three players
            char[] stringSeparators1 = { '[', ']' };
            var res1 = response.Split(stringSeparators1);

            // splits up the individual attributes of each entry
            char[] stringSeparators2 = { ',' };
            var res2 = res1[1].Split(stringSeparators2);

            // throws away useless tokens like quotes and colons
            char[] stringSeparators3 = { '"', ':' };
            var score1 = res2[1].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
            var user1 = res2[2].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
            var score2 = res2[5].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
            var user2 = res2[6].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
            var score3 = res2[9].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
            var user3 = res2[10].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);

            List<string> userList = new List<string>();
            userList.Add(user1[1]);
            userList.Add(user2[1]);
            userList.Add(user3[1]);

            List<int> scoreList = new List<int>();
            scoreList.Add(Int32.Parse(score1[1]));
            scoreList.Add(Int32.Parse(score2[1]));
            scoreList.Add(Int32.Parse(score3[1]));

            QueryResult result = new QueryResult(userList, scoreList);
            List<string> usernames = result.getUserNames();
            List<int> scores = result.getPlayerScores();
            Debug.Log("Usernames: " + usernames[0] + usernames[1] + usernames[2]);
            for (int i = 0; i < 3; i++)
            {
                positions[i].GetComponent<TMP_Text>().text = usernames[i];
                positions[i].transform.Find("Score").GetComponent<TMP_Text>().text = scores[i].ToString();
            }

            Debug.Log("Usernames: " + userList[0] + " and " + userList[1] + " and " + userList[2]);
            Debug.Log("Scores: " + scoreList[0] + " and " + scoreList[1] + " and " + scoreList[2]);
        }
    }
}
