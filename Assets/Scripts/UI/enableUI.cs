using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enableUI : MonoBehaviour
{
    public GameObject TargetUI;
    public void toggleOn()
    {
        TargetUI.SetActive(true);
    }

    public void toggleOff()
    {
        TargetUI.SetActive(false);
    }
}
