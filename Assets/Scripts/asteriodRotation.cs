using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class asteriodRotation : MonoBehaviour
{
    public float rotationSpeed = 0.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed) * Time.deltaTime);
    }
}
