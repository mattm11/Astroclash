using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RotatePlayer : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (IsClient)
        {
            if (Camera.main != null)
            {
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
                Vector3 mouseRealtivePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                Vector3 direction = mouseRealtivePosition - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);
                transform.rotation = rotation;
            }
        }
    }
}
