using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;                       // Takip edilecek oyuncu
    public Vector3 offset = new Vector3(0f, 2f, -5f); // Kamera pozisyon farký

    [Header("Camera Settings")]
    public float rotationSpeed = 100f;             // Fare hassasiyeti
    public float smoothTime = 0.1f;                // Kameranýn yumuþaklýðý
    public float minYAngle = -30f;                 // Minimum yukarý-aþaðý bakýþ açýsý
    public float maxYAngle = 70f;                  // Maksimum yukarý-aþaðý bakýþ açýsý
    public float playerTurnSmooth = 10f;           // Karakterin dönüþ hýzý

    private float yaw;                             // Saða-sola dönüþ (Y ekseni)
    private float pitch;                           // Yukarý-aþaðý bakýþ (X ekseni)
    private Vector3 currentVelocity;

    private PlayerInput playerInput;
    private InputAction lookAction;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player Transform is missing!");
            return;
        }

        // PlayerInput bileþenini karakterden al
        playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found on Player!");
            return;
        }

        // Look action'u al
        lookAction = playerInput.actions["Look"];
        if (lookAction == null)
        {
            Debug.LogError("Look action not found in InputActions!");
            return;
        }

        // Baþlangýç açýlarýný al
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void LateUpdate()
    {
        if (player == null || lookAction == null) return;

        HandleCameraRotation();
        FollowPlayer();
        RotatePlayerWithCamera();
    }

    void HandleCameraRotation()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
    }

    void FollowPlayer()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = player.position + rotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.rotation = rotation;
    }

    void RotatePlayerWithCamera()
    {
        // Kameranýn yönüne göre karakteri yumuþakça döndür
        Vector3 cameraForward = transform.forward;
        cameraForward.y = 0f; // Yukarý bileþeni sýfýrla (sadece yatay dönüþ)
        if (cameraForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * playerTurnSmooth);
        }
    }
}