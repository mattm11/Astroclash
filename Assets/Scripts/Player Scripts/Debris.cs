using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    private Vector3 direction = Vector3.zero;
    private float speed = 0.0f;
    private float rotationSpeed = 0.0f;
    private float timeLived = 0.0f;

    public float speedMin = 0.0f;
    public float speedMax = 0.0f;

    public float rotationMin = 0.0f;
    public float rotationMax = 0.0f;

    public float timeToLive = 60.0f; // in seconds

    public List<Sprite> sprites = new List<Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        //Randomize the direction and speed based on the param (direction is any direction)
        direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
        speed = Random.Range(speedMin, speedMax);
        rotationSpeed = Random.Range(rotationMin, rotationMax);

        //randomize sprite
        int index = Random.Range(0, sprites.Count);
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[index];

        //Add the force
        gameObject.GetComponent<Rigidbody2D>().AddForce(direction * speed, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLived >= timeToLive)
        {
            GameObject.Destroy(gameObject.transform.parent.gameObject);
        }

        transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed) * Time.deltaTime);
        timeLived += Time.deltaTime;
    }
}
