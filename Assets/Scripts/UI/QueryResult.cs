using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Astroclash
{
    public struct QueryResult
    {
        private List<string> userNames;
        private List<int> playerScores;

        QueryResult(List<string> _userNames, List<int> _playerScores)
        {
            if (_userNames.Count == _playerScores.Count)
            {
                userNames = _userNames;
                playerScores = _playerScores;
            }
            else
            {
                throw new AstroClashDatabaseError("Username and player score arrays have different lengths");
            }
        }

        public List<string> getUserNames(int _numberOfUsers)
        {
            List<string> newList = new List<string>();
            for (int i = 0; i < userNames.Count && i < _numberOfUsers; i++)
            {
                newList.Add(userNames[i]);
            }

            return newList;
        }

        public List<string> getUserNames()
        {
            return userNames;
        }

        public List<int> getPlayerScores(int _numberOfUsers)
        {
            List<int> newList = new List<int>();
            for (int i = 0; i < playerScores.Count && i < _numberOfUsers; i++)
            {
                newList.Add(playerScores[i]);
            }

            return newList;
        }

        public List<int> getPlayerScores()
        {
            return playerScores;
        }

        public class AstroClashDatabaseError : Exception
        {
            public AstroClashDatabaseError(string _message) : base(_message) {}
            public AstroClashDatabaseError() {}
        }
    }
}

