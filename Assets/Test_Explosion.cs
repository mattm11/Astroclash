using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Explosion : MonoBehaviour
{
    public GameObject debris;
    public int debrisAmount = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            for (int i = 0; i < debrisAmount; i++)
            {
                Instantiate(debris, Vector3.zero, Quaternion.identity);
            }
        }
    }
}
