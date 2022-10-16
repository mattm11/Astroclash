using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Astroclash
{
    public class weaponDebugUI : MonoBehaviour
    {

        private List<GameObject> modules = new List<GameObject>();
        private bool toggled = false;
        private Component weapon;
        public weaponDebugUI(List<float> stats, List<string> names, Component _weapon)
        {
            GameObject canvas = GameObject.Find("Canvas");
            Object prefab = Resources.Load("prefabs/UIModule");
            weapon = _weapon;

            for (int i = 0; i < stats.Count; i++)
            {
                GameObject module = (GameObject)Instantiate(prefab, new Vector3(30, i * -30 + -30 + 500, 0), Quaternion.identity);
                module.SetActive(false);
                module.transform.parent = canvas.gameObject.transform;
                module.tag = "UIModule";
                module.gameObject.GetComponent<TMP_InputField>().text = stats[i].ToString();
                module.gameObject.GetComponentsInChildren<Transform>()[5].gameObject.GetComponent<TMP_Text>().text = names[i];
                modules.Add(module);
            }
        }

        //when called toggled the debug UI
        public void toggle()
        {
            if (toggled)
            {
                for (int i = 0; i < modules.Count; i++)
                {
                    modules[i].SetActive(false);
                }
                toggled = false;
            }
            else
            {
                for (int i = 0; i < modules.Count; i++)
                {
                    modules[i].SetActive(true);
                }
                toggled = true;
            }
        }

        //get tool's input
        public List<string> getInput()
        {
            List<string> inputs = new List<string>();
            for (int i = 0; i < modules.Count; i++)
            {
                inputs.Add(modules[i].GetComponent<TMP_InputField>().text);
            }

            return inputs;
        }
    }
}
