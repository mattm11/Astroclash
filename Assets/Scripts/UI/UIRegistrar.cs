using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRegistrar : MonoBehaviour
{
    public List<GameObject> UIElements;

    public void disableAll()
    {
        for (int i = 0; i < UIElements.Count; i++)
        {
            UIElements[i].SetActive(false);
        }
    }

    public void disableIndex(int _index)
    {
        UIElements[_index].SetActive(false);
    }

    public void enableAll()
    {
        for (int i = 0; i < UIElements.Count; i++)
        {
            UIElements[i].SetActive(true);
        }
    }

    public void enableIndex(int _index)
    {
        UIElements[_index].SetActive(true);
    }
}
