using UnityEngine;
using Mirror;

public class CameraRide : MonoBehaviour
{
    public float speed = 0.1f;

    void Update()
    {
        // only while not in character selection or world
        if (((NetworkManagerMMO)NetworkManager.singleton).state != NetworkState.Offline)
            Destroy(this);

        // move backwards
        transform.position -= transform.forward * speed * Time.deltaTime;
    }
}
