using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyItem : MonoBehaviour
{
    public float value = 100.0f;
    public float stayAlive = 60.0f; //time to say alive untouched in seconds
    private float aliveFor = 0.0f;

    // Update is called once per frame
    void Update()
    {
        aliveFor += Time.deltaTime;

        if (aliveFor >= stayAlive)
        {
            GameObject.Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.tag == "Player")
        {
            collider.gameObject.GetComponent<playerController>().addCurrency(value);
            GameObject.Destroy(gameObject);
        }
    }
}
