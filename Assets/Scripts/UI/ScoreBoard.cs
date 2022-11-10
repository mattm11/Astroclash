using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Astroclash;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public List<GameObject> positions;
    // Start is called before the first frame update
    void Start()
    {
        //TODO: replace with database interface function to get struct
        QueryResult result = databaseQuery();
        List<string> usernames = result.getUserNames();
        List<int> scores = result.getPlayerScores();
        for (int i = 0; i < 10; i++)
        {
            positions[i].GetComponent<TMP_Text>().text = usernames[i];
            positions[i].transform.Find("Score").GetComponent<TMP_Text>().text = scores[i].ToString();
        }
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
        for (int i = 0; i < 10; i++)
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
        QueryResult result = databaseQuery();
        updateScores(result);
    }

    //Dummy function for now should be replaced with the real backend API call
    private QueryResult databaseQuery()
    {
        List<string> userNames = new List<string>()
        {
            "TheLegend27",
            "ZelliexHD",
            "Paxmeeble",
            "FlyFire",
            "seenkay",
            "SugarPeas",
            "Joker",
            "Ashman9999",
            "TheLongestNameOfAllTime",
            "Drater",
            "SomeOtherPlayer"
        };

        List<int> playerScores = new List<int>()
        {
            1000000,
            900000,
            800000,
            700000,
            600000,
            500000,
            42069,
            300000,
            200000,
            -6,
            -1000000
        };

        QueryResult result = new QueryResult(userNames, playerScores);
        return result;
    }
}
