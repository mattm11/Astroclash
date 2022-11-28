using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class hostileManager : NetworkBehaviour
{

    private int maxAsteroids = 20;
    private int maxMotherships = 8;
    private int maxAsteroidsNearPlayer = 10;
    private int maxMothershipsNearPlayers = 2;
    private float minSpawnRadius = 18f;
    private float maxSpawnRadius = 20f;
    List<GameObject> asteroids;
    List<GameObject> motherships;
    // Start is called before the first frame update
    void Start()
    {
        asteroids = new List<GameObject>();
        motherships = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (checkNearbyAsteroids(players[i].transform.position) < maxAsteroidsNearPlayer && asteroids.Count < maxAsteroids && Random.Range(0f, 1f) < 0.5f)
                {
                    spawnAsteroid(players[i]);
                }
                if (checkNearbyMotherships(players[i].transform.position) < maxMothershipsNearPlayers && motherships.Count < maxMotherships && Random.Range(0f, 1f) < 0.5f)
                {
                    spawnMothership(players[i]);
                }
            }
        }
    }

    void spawnAsteroid(GameObject _player)
    {
        float spawnRange = Random.Range(minSpawnRadius, maxSpawnRadius);
        float spawnAngle = Random.Range(0, 2 * Mathf.PI);
        Vector3 spawnLocation = new Vector3(spawnRange * Mathf.Sin(spawnAngle), spawnRange * Mathf.Cos(spawnAngle), 0);
        asteroids.Add(GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Asteroid", spawnLocation));
    }

    void spawnMothership(GameObject _player)
    {
        float spawnRange = Random.Range(minSpawnRadius, maxSpawnRadius);
        float spawnAngle = Random.Range(0, 2 * Mathf.PI);
        Vector3 spawnLocation = new Vector3(spawnRange * Mathf.Sin(spawnAngle), spawnRange * Mathf.Cos(spawnAngle), 0);
        motherships.Add(GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().spawnEntity("Mothership", spawnLocation));
    }

    void destroyAsteroid(GameObject asteroid)
    {
        asteroids.Remove(asteroid);
        GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(asteroid.GetComponent<NetworkObject>().NetworkObjectId);
    }

    void destroyMothership(GameObject mothership)
    {
        motherships.Remove(mothership);
        GameObject.FindGameObjectWithTag("Spawn Manager").GetComponent<SpawnManager>().despawnEntity(mothership.GetComponent<NetworkObject>().NetworkObjectId);
    }

    int checkNearbyAsteroids(Vector3 position)
    {
        int numOfAsteroids = 0;
        asteroids.ForEach(asteroid => {
            if (Vector3.Distance(position, asteroid.transform.position) < maxSpawnRadius)
            {
                numOfAsteroids++;
            }
        });
        return numOfAsteroids;
    }

    int checkNearbyMotherships(Vector3 position)
    {
        int numOfMothers = 0;
        motherships.ForEach(asteroid => {
            if (Vector3.Distance(position, asteroid.transform.position) < maxSpawnRadius)
            {
                numOfMothers++;
            }
        });
        return numOfMothers;
    }
}
