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
    private QueryResult result;
    private float update;
    public List<GameObject> positions;
    // Start is called before the first frame update
    void Start()
    {
        //TODO: replace with database interface function to get struct
        StartCoroutine(databaseQuery());
    }

    //update function to call when scoreboard needs to dynamically be changed
    public void updateScores()
    {
        List<string> names = result.getUserNames();
        List<int> scores = result.getPlayerScores();
        if (names.Count == scores.Count && names.Count == positions.Count)
        {
            for (int i = 0; i < 10; i++)
            {
                positions[i].GetComponent<TMP_Text>().text = names[i];
                positions[i].transform.Find("Score").GetComponent<TMP_Text>().text = scores[i].ToString();
            }
        }
    }

    // TODO: 2 compiler errors => [1] body is an iterator and cannot return a value, [2] actual line where value is returned
    // Can't run a coroutine more than once, also cannot return anything with a method thast inherits from IEnumerator
    public IEnumerator databaseQuery()
    {
        while (true)
        {
            //Debug.Log("getRequest has been called");
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

                Debug.Log(res2[0] + " and " + res2[1] + " and " + res2[2] + " and " + res2[3]);

                // throws away useless tokens like quotes and colons
                char[] stringSeparators3 = { '"', ':' };

                List<string> userList = new List<string>();
                List<int> scoreList = new List<int>();

                int index = 1;
                for (int i = 0; i < 10; i++)
                {
                    var score = res2[index].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
                    var user = res2[index + 1].Split(stringSeparators3, StringSplitOptions.RemoveEmptyEntries);
                    userList.Add(user[1]);
                    scoreList.Add(Int32.Parse(score[1]));
                    index = index + 4;
                }

                result = new QueryResult(userList, scoreList);
                updateScores();

                Debug.Log("Usernames: " + userList[0] + " and " + userList[1] + " and " + userList[2]);
                Debug.Log("Scores: " + scoreList[0] + " and " + scoreList[1] + " and " + scoreList[2]);
            }

            // Coroutine will repeat every 5 seconds
            yield return new WaitForSeconds(5.0f);
        }
    }
}
