using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class playerController : NetworkBehaviour
{

    //acceleration amount per second added to velocity. de-acceleration is 2:1 proportional to acceleration
    private float acceleration = 1.0f;
    //maximum speed of player ship
    private float maxVelocity = 3.0f;
    private float velocity = 0.0f;
    //speed at which they can rotate (45 degrees per second)
    private Vector3 rotationAcceleration = new Vector3(0.0f, 0.0f, 90.0f);
    private Vector3 rotationDieOff = new Vector3(0.0f, 0.0f, 0.0f);

    private Camera playerCamera;

    void Start()
    {
        GameObject parent = gameObject.transform.parent.gameObject;
        playerCamera = parent.GetComponentInChildren<Camera>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (IsOwner) 
        {
            //update the camera origin
            Vector3 newPosition = playerCamera.transform.position;
            newPosition.x = gameObject.transform.position.x;
            newPosition.y = gameObject.transform.position.y;
            playerCamera.transform.position= newPosition;

            Debug.Log("Current Rotation: " + transform.eulerAngles.z);
            Debug.Log("Current X: " + velocity * (float)Math.Cos(transform.eulerAngles.z));
            Debug.Log("Current Y: " + velocity * (float)Math.Sin(transform.eulerAngles.z));

            //Movement is broken down into: X = Movement_Speed * cos(rotation_angle) Y = Movement_Speed * sin(rotation_angle)
            float angle = (float)(transform.eulerAngles.z * (Math.PI / 180));
            transform.position += new Vector3(velocity * (float)Math.Cos(angle), velocity * (float)Math.Sin(angle), 0) * Time.deltaTime;
            transform.Rotate(rotationDieOff * Time.deltaTime);
            rotationDieOff -= rotationDieOff * 0.50f * Time.deltaTime;

            if (Input.GetKey(KeyCode.W))
            {
                //adding player speed, until max speed is reached
                if(velocity + acceleration <= maxVelocity)
                {
                    velocity += acceleration * Time.deltaTime;
                }
                else if ((velocity + acceleration) >= maxVelocity && velocity + 0.25f <= maxVelocity)
                {
                    velocity += 0.25f * Time.deltaTime;
                }
                else 
                {
                    velocity = maxVelocity;
                }
            }
            //Slow down
            else if (Input.GetKey(KeyCode.S))
            {
                if((velocity - acceleration) > 0)
                {
                    velocity -= (acceleration) * Time.deltaTime;
                }
                else if ((velocity - acceleration) < 0 && velocity - 0.25f >= 0)
                {
                    velocity -= 0.25f * Time.deltaTime;
                }
                else 
                {
                    velocity = 0.0f;
                }
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(rotationAcceleration * Time.deltaTime);
                
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(-rotationAcceleration * Time.deltaTime);
            }
        }
    }
}
