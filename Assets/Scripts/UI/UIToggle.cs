using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggle : MonoBehaviour
{
    public void toggleOn(GameObject targetUI)
    {
        targetUI.SetActive(true);
    }

    public void toggleOff(GameObject targetUI)
    {
        targetUI.SetActive(false);
    }
}
