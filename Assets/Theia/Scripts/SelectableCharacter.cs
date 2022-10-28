// small helper script that is added to character selection previews at runtime
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerIndicator))]
[DisallowMultipleComponent]
public class SelectableCharacter : MonoBehaviour
{
    // index will be set by networkmanager when creating this script
    public int index = -1;

    void OnMouseDown()
    {
        // set selection index
        ((NetworkManagerMMO)NetworkManager.singleton).selection = index;

        // show selection indicator for better feedback
        GetComponent<PlayerIndicator>().SetViaParent(transform);
    }

    void Update()
    {
        // remove indicator if not selected anymore
        if (((NetworkManagerMMO)NetworkManager.singleton).selection != index)
        {
            GetComponent<PlayerIndicator>().Clear();
        }
    }
}
