using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFade : MonoBehaviour
{
    private TMP_Text text;
    public float fadeTimer; //fades in and out in seconds
    private bool transparent = false;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (text.alpha - (fadeTimer * Time.deltaTime) >= 0.0f && transparent == false)
        {
            text.alpha -= fadeTimer * Time.deltaTime;
        }
        else if (text.alpha - (fadeTimer * Time.deltaTime) < 0.0f && transparent == false)
        {
            text.alpha = 0.0f;
            transparent = true;
        }

        if (text.alpha + (fadeTimer * Time.deltaTime) <= 1.0f && transparent == true)
        {
            text.alpha += fadeTimer * Time.deltaTime;
        }
        else if (text.alpha + (fadeTimer * Time.deltaTime) > 1.0f && transparent == true)
        {
            text.alpha = 1.0f;
            transparent = false;
        }
    }
}
