using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.SetActive(true);
            if (audioListener != null) audioListener.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            playerCamera.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
        }
    }
}
