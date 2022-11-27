using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFlash : MonoBehaviour
{
    private TMP_Text text;
    public Color pulseColor;
    public float pulseTimer;

    private bool isColor = false;
    float sumDeltaTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.color = Color.Lerp(Color.white, pulseColor, Mathf.PingPong(Time.time, pulseTimer));
    }
}
