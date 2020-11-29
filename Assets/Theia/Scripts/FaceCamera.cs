// Useful for Text Meshes that should face the camera.
//
// In some cases there seems to be a Unity bug where the text meshes end up in
// weird positions if it's not positioned at (0,0,0). In that case simply put it
// into an empty GameObject and use that empty GameObject for positioning.
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Camera.main calls FindObjectWithTag each time. cache it!
    Transform cam;

    void Awake()
    {
        // find main camera
        cam = Camera.main.transform;

        // disable by default until visible
        enabled = false;
    }

    // LateUpdate so that all camera updates are finished.
    void LateUpdate()
    {
        transform.forward = cam.forward;
    }

    // copying transform.forward is relatively expensive and slows things down
    // for large amounts of entities, so we only want to do it while the mesh
    // is actually visible
    void OnBecameVisible() { enabled = true; }
    void OnBecameInvisible() { enabled = false; }
}
