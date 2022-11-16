using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public float valueLimit = 0.0f;

    private Image fillImage;
    
    public void SetMaxValue(float maxValue)
    {
        fillImage = gameObject.transform.Find("Fill").gameObject.GetComponent<Image>();

        slider.maxValue = maxValue;
        slider.value = maxValue;

        if (fillImage == null)
            Debug.Log("Fill image is null!");

        if (fillImage.color == null)
            Debug.Log("Fill color was not found!");

        fillImage.color = gradient.Evaluate(1f);
    }

    public void SetValue(float value)
    {
        fillImage = gameObject.transform.Find("Fill").gameObject.GetComponent<Image>();
        slider.value = value;

        fillImage.color = gradient.Evaluate(slider.normalizedValue);
    }

    public float getValue()
    {
        return slider.value;
    }

    public void increaseBar()
    {
        if (slider.value <= valueLimit)
        {    
            float width = gameObject.GetComponent<RectTransform>().sizeDelta.x;
            float height = gameObject.GetComponent<RectTransform>().sizeDelta.y;
            Vector2 newWidth = new Vector2(width + 3.0f, height);
            gameObject.GetComponent<RectTransform>().sizeDelta = newWidth;
        }
    }
}
