using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class bulletProjectiles : NetworkBehaviour
{
    private float range = 0.0f;
    private float damage = 0.0f;
    private Vector3 startPosition = new Vector3();

    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //calculate distance traveled
        float distance = Mathf.Sqrt(Mathf.Pow((transform.position.x - startPosition.x), 2) + Mathf.Pow((transform.position.y - startPosition.y), 2));
        if (distance > range)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void setStats(float _range, float _damage)
    {
        range = _range;
        damage = _damage;
    }
}
