using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 0.1f;

    [Header("References")]
    public Transform playerBody;

    float xRotation;
    Vector2 lookInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float lookX = lookInput.x;
        float lookY = lookInput.y;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * lookX);
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>() * mouseSensitivity;
    }
}
