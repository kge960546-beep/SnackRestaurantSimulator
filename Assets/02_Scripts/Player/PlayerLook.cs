using Unity.Netcode;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [Header("Settings")]
    public float mouseSensitivity = 100.0f;
    public float topClamping = 90.0f;
    public float bottomClamping = -90.0f;

    [Header("References")]
    public Transform cameraHolder;

    private float verticalRotation = 0f;

    private void Start()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, bottomClamping, topClamping);

        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0.0f, 0.0f);
    }
}
