using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggle : MonoBehaviour
{
    private GameObject UILogic;
    void Start()
    {
        UILogic = GameObject.Find("UILogic");
    }
    public void toggleOn(GameObject targetUI)
    {
        for (int i = 2; i < UILogic.GetComponent<UIRegistrar>().getElementCount(); i++)
        {
            UILogic.GetComponent<UIRegistrar>().disableIndex(i);
        }

        targetUI.SetActive(true);
    }

    public void toggleOff(GameObject targetUI)
    {
        targetUI.SetActive(false);
    }
}
