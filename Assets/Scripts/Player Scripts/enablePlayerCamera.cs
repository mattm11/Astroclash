using Unity.Netcode;
using UnityEngine;

public class enablePlayerCamera : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            gameObject.GetComponent<Camera>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<Camera>().enabled = false;
        }
    }
}
