using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Astroclash
{
    public class Player : MonoBehaviour
    {
        private static Player _instance;
        public string username;
        public int score;
        private string jsonString;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        public static Player Instance
        {
            get
            {
                return _instance;
            }
        }

        public void saveJsonString()
        {
            jsonString = JsonUtility.ToJson(_instance);
        }

        public string getJsonString()
        {
            return jsonString;
        }

        public string getUsername()
        {
            return username;
        }

        public int getScore()
        {
            return score;
        }

        public void setUsername(string _username)
        {
            username = _username;
        }

        public void setScore(int _score)
        {
            score = _score;
        }
    }
}

