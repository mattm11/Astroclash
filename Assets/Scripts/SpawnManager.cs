using UnityEngine;
using Unity.Netcode;

public class SpawnManager : MonoBehaviour
{
    public void spawnEntity(string _gameObjectName, Vector3 _position)
    {
        Debug.Log("Spawning \"" + _gameObjectName + "\" at position: " + _position.ToString());
        UnityEngine.Object prefab = Resources.Load("prefabs/Entities/" + _gameObjectName);

        if (prefab == null)
        {
            Debug.Log( "\"" + _gameObjectName + "\" is not registered in Network Manager or does not exist");
        }

        GameObject entity = (GameObject)Instantiate(prefab, _position, Quaternion.identity);
        entity.GetComponent<NetworkObject>().Spawn();
        entity.transform.position = _position;
        entity.transform.localPosition = _position;
    }

    public void despawnEntity(ulong _networkID)
    {
        GameObject networkManger = GameObject.Find("Network Manager");
        NetworkObject entity = networkManger.GetComponent<NetworkManager>().SpawnManager.SpawnedObjects[_networkID];
        string objectName = entity.gameObject.name;
        Debug.Log("Despawning \"" + objectName + "\" with netID: " + _networkID);
        entity.Despawn();
    }
}
