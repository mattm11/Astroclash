using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

namespace Astroclash
{
    public struct StateRequirement
    {
        public string stateName;
        public List<string> statName;
        public List<float> amountRequired;

        public StateRequirement(string _stateName, List<string> _statName, List<float> _amountRequired)
        {
            stateName = _stateName;
            statName = _statName;
            amountRequired = _amountRequired;
        }
    };
    public class WeaponController : NetworkBehaviour
    {
        private class WeaponDebugUI : MonoBehaviour
        {
            private List<GameObject> modules = new List<GameObject>();

            public bool toggleFlag = false;

            public WeaponDebugUI(List<float> _stats, List<string> names, GameObject canvas)
            {
                Object prefab = Resources.Load("prefabs/UIModule");

                for (int i = 0; i < _stats.Count; i++)
                {
                    GameObject module = (GameObject)Instantiate(prefab, new Vector3(30, i * -30, 0), Quaternion.identity);
                    module.SetActive(false);
                    module.transform.parent = canvas.gameObject.transform;
                    module.tag = "UIModule";
                    module.gameObject.GetComponent<TMP_InputField>().text = _stats[i].ToString();
                    module.gameObject.GetComponentsInChildren<Transform>()[5].gameObject.GetComponent<TMP_Text>().text = names[i];
                    modules.Add(module);
                }
            }
            public void toggle()
            {
                if (toggleFlag)
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        modules[i].SetActive(false);
                    }
                    toggleFlag = false;
                    Debug.Log("Debug mode off");
                }
                else 
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        modules[i].SetActive(true);
                    }
                    toggleFlag = true;
                    Debug.Log("Debug mode on");
                }
            }
            public List<float> getInput()
            {
                List<float> inputs = new List<float>();
                for (int i = 0; i < modules.Count; i++)
                {
                    inputs.Add(float.Parse(modules[i].GetComponent<TMP_InputField>().text));
                }

                return inputs;
            }
            public void setDebugValues(List<float> _currentStats)
            {
                for (int i = 0; i < modules.Count; i++)
                {
                    modules[i].GetComponent<TMP_InputField>().text = _currentStats[i].ToString();
                } 
            }
        }

        private List<float> stats = new List<float>();
        private List<float> weaponStatsIncrement = new List<float>();
        private List<string> weaponStatsNames = new List<string>();
        private List<string> stateNames = new List<string>();
        private List<bool> states = new List<bool>();
        private List<GameObject> stateUIObjects = new List<GameObject>();
        private List<GameObject> statUIObjects = new List<GameObject>();

        private static Dictionary<string, float> weaponIncrements = new Dictionary<string, float>();
        private Dictionary<string, float> weaponStats = new Dictionary<string, float>();
        private Dictionary<string, float> weaponStatCosts = new Dictionary<string, float>();
        private Dictionary<string, float> weaponStateCosts = new Dictionary<string, float>();
        private Dictionary<string, bool> weaponStates = new Dictionary<string, bool>();
        private Dictionary<string, StateRequirement> stateRequirements = new Dictionary<string, StateRequirement>();
        

        private WeaponDebugUI debugUI = null;

        private GameObject bullet = null;
        private GameObject upgradeUI = null;
        private GameObject canvas = null;
        private GameObject player = null;

        private string stateSet = null;

        public WeaponController (List<float> _stats, List<float> _increments, List<string> _statsName, List<bool> _states, List<string> _stateNames)
        {
            stats = _stats;
            weaponStatsIncrement = _increments;
            weaponStatsNames = _statsName;
            stateNames = _stateNames;
            states = _states;

            //clear in case of previous game memory conflicts
            weaponIncrements.Clear();
            weaponStats.Clear();
            weaponStatCosts.Clear();
            weaponStateCosts.Clear();
            weaponStates.Clear();
            stateRequirements.Clear();

            for (int i = 0; i < stats.Count; i++)
            {
                weaponIncrements.Add(weaponStatsNames[i], weaponStatsIncrement[i]);
            }
        }

        public void registerPlayer(GameObject _player)
        {
            player = _player;
        }
        
        //stat manipulation
        public void registerStatCosts(List<float> _costs)
        {
            for(int i = 0; i < weaponStatsNames.Count; i++)
            {
                weaponStatCosts.Add(weaponStatsNames[i], _costs[i]);
            }
        }
        public void increaseStat(string _statName)
        {
            if (player == null)
            {
                Debug.Log("Player not found!");
            }
            else 
            {
                Debug.Log("Object Name: " + player.name);
            }

            if (player.GetComponent<playerController>().getCurrency() >= weaponStatCosts[_statName])
            {
                weaponStats[_statName] += weaponIncrements[_statName];
                player.GetComponent<playerController>().subtractCurrency(weaponStatCosts[_statName]);

                 //update debug block
                List<float> currStats = new List<float>();
                for (int i = 0; i < weaponStatsNames.Count; i++)
                {
                    currStats.Add(weaponStats[weaponStatsNames[i]]);
                }
                debugUI.setDebugValues(currStats);
                checkStates();
            }
            else 
            {
                Debug.Log("Not enough money!");
            }
        }
        public void decreaseState(string _statName)
        {
            weaponStats[_statName] -= weaponIncrements[_statName];

            //update debug block
            List<float> currStats = new List<float>();
            for (int i = 0; i < weaponStatsNames.Count; i++)
            {
                currStats.Add(weaponStats[weaponStatsNames[i]]);
            }
            debugUI.setDebugValues(currStats);
            checkStates();
        }

        //state requirement functions
        public void registerStateCosts(List<float> _costs)
        {
            for(int i = 0; i < stateNames.Count; i++)
            {
                weaponStateCosts.Add(stateNames[i], _costs[i]);
            }
        }
        public bool checkStateRequirementLessThan(string _stateName)
        {
            StateRequirement temp = stateRequirements[_stateName];
            for (int i = 0; i < temp.amountRequired.Count; i++)
            {
                if (weaponStats[temp.statName[i]] >= temp.amountRequired[i])
                {
                    return false;
                }
            }

            return true;
        }
        public bool checkStateRequirementGreaterThan(string _stateName)
        {
            StateRequirement temp = stateRequirements[_stateName];
            for (int i = 0; i < temp.amountRequired.Count; i++)
            {
                if (weaponStats[temp.statName[i]] <= temp.amountRequired[i])
                {
                    return false;
                }
            }

            return true;
        }
        public void registerStateRequirement(string _stateName, StateRequirement _requirement)
        {
            stateRequirements.Add(_stateName, _requirement);
        }
        public void lockStateButton(GameObject _button)
        {
            Image image = _button.GetComponent<Image>();
            image.color = new Color( 0.0f, 1.0f, 0.0f, 1.0f);

            _button.GetComponent<Button>().enabled = false;  
        }
        private void checkStates()
        {
            for (int i = 0; i < stateUIObjects.Count; i++)
            {
                string tempName = stateUIObjects[i].name;
                bool reqCheck = checkStateRequirementGreaterThan(tempName);

                if (reqCheck && stateSet == null)
                {
                    enableButton(stateUIObjects[i]);
                }
                else if (reqCheck == false && stateSet == null)
                {
                    disableButton(stateUIObjects[i]);
                }
                else if (stateSet != null)
                {
                    if (stateUIObjects[i].name == stateSet)
                    {
                        lockStateButton(stateUIObjects[i]);
                    }
                    else
                    {
                        disableButton(stateUIObjects[i]);
                    }
                }
            }
        }
    
        //function to toggle weapons debug mode
        public void step()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                debugUI.toggle();
            }

            if (debugUI.toggleFlag)
            {
                setStatsDebug();
            }

            checkStates();
        }
        public void setStatsDebug()
        {
            List<float> input = debugUI.getInput();
            for (int i = 0; i < weaponStatsNames.Count; i++)
            {
                setStat(weaponStatsNames[i], input[i]);
            }
        }
        public bool isDebug()
        {
            return debugUI.toggleFlag;
        }
        
        // UI functions
        public void registerUpdgradeUI(GameObject _ui)
        {
            upgradeUI = _ui;
        }
        public void registerCanvas(GameObject _canvas)
        {
            canvas = _canvas;
        }
        public void enableButton(GameObject _button)
        {
            Image image = _button.GetComponent<Image>();
            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                1.0f
            );

            _button.GetComponent<Button>().enabled = true;
        }
        public void disableButton(GameObject _button)
        {
            Image image = _button.GetComponent<Image>();
            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                0.25f
            );

            _button.GetComponent<Button>().enabled = false;
        }
        
        // Getter & Setters
        public void setState(string _stateName)
        {
            if (player.GetComponent<playerController>().getCurrency() >= weaponStateCosts[_stateName])
            {
                Debug.Log("Setting state: " + _stateName);
                weaponStates[_stateName] = true;
                stateSet = _stateName;
                player.GetComponent<playerController>().subtractCurrency(weaponStateCosts[_stateName]);
                checkStates();
            }
            
        }
        public void setStat(string _statName, float _value)
        {
            weaponStats[_statName] = _value;
        }
        public void setBulletPrefab(GameObject _prefab)
        {
            bullet = _prefab;
        }
        public float getStat(string _statName)
        {
            return weaponStats[_statName];
        }
        public void setStatIncrement(string _statName, float _increment)
        {
            weaponIncrements[_statName] = _increment;
        }
        public bool getState(string _stateName)
        {
            return weaponStates[_stateName];
        }
        public List<string> getStatNames()
        {
            return weaponStatsNames;
        }
        public List<string> getStateNames()
        {
            return stateNames;
        }
        public StateRequirement getStateRequirement(string _stateName)
        {
            return stateRequirements[_stateName];
        }

        //debug functions
        public void instantiateUI()
        {
            debugUI = new WeaponDebugUI(stats, weaponStatsNames, canvas);

            for (int i = 0; i < stats.Count; i++)
            {
                weaponStats.Add(weaponStatsNames[i], stats[i]);
                if (upgradeUI.transform.Find(weaponStatsNames[i]) == null)
                {
                    Debug.Log("Weapon stat " + weaponStatsNames[i] + " does not have a UI element.");
                }
                else 
                {
                    GameObject statUIElement = upgradeUI.transform.Find(weaponStatsNames[i]).gameObject;
                    statUIElement.AddComponent<Button>();

                    ColorBlock buttonColor = new ColorBlock();
                    buttonColor.normalColor = Color.white;
                    buttonColor.highlightedColor = new Color(0.5f, 1.0f, 0.5f);
                    buttonColor.pressedColor = Color.green;
                    buttonColor.selectedColor = Color.white;
                    buttonColor.disabledColor = Color.grey;
                    buttonColor.colorMultiplier = 1.0f;
                    buttonColor.fadeDuration = 0.1f;

                    statUIElement.GetComponent<Button>().colors = buttonColor;
                    string name = weaponStatsNames[i];
                    statUIElement.GetComponent<Button>().onClick.AddListener(() => increaseStat(name));
                    statUIObjects.Add(statUIElement);
                }
                
            }

            for (int i = 0; i < states.Count; i++)
            {
                weaponStates.Add(stateNames[i], states[i]);
                GameObject stateUIElement = upgradeUI.transform.Find(stateNames[i]).gameObject;

                if (stateUIElement == null)
                    Debug.Log("Canvas object not found!");

                stateUIElement.AddComponent<Button>();
                string state = stateNames[i];
                stateUIElement.GetComponent<Button>().onClick.AddListener(() => setState(state));
                stateUIElement.GetComponent<Button>().enabled = false;

                ColorBlock buttonColor = new ColorBlock();
                buttonColor.normalColor = Color.white;
                buttonColor.highlightedColor = new Color(0.5f, 1.0f, 0.5f);
                buttonColor.pressedColor = Color.green;
                buttonColor.selectedColor = Color.white;
                buttonColor.disabledColor = Color.grey;
                buttonColor.colorMultiplier = 1.0f;
                buttonColor.fadeDuration = 0.1f;

                stateUIElement.GetComponent<Button>().colors = buttonColor;

                stateUIObjects.Add(stateUIElement);
            }
        }
    }
}