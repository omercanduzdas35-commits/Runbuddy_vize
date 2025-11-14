using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSmoothTime = 0.1f;

    [Header("References")]
    public Transform cameraTransform; // Kameranýn yönüne göre hareket
    public Transform groundCheck;     // Ayak hizasýnda boþ obje
    public LayerMask groundLayer;     // Zemin için layer maskesi

    private Animator animator;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;

    private float rotationSmoothVelocity;
    private bool isGrounded = true;
    private bool isJumping = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found!");
            return;
        }

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (playerInput == null) return;

        HandleJump();
        HandleAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
        CheckGround();
    }

    void HandleMovement()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed;

            // Y eksenini koruyarak hareket
            Vector3 horizontalVel = new Vector3(moveDir.x * speed, 0, moveDir.z * speed);
            rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void HandleJump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            isJumping = true;
            animator.SetBool("IsJumping", true);
        }
    }

    void CheckGround()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 0.3f, groundLayer))
        {
            if (!isGrounded)
            {
                isGrounded = true;
                isJumping = false;
                animator.SetBool("IsJumping", false);
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    void HandleAnimation()
    {
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        float normalizedSpeed = Mathf.InverseLerp(0f, runSpeed, horizontalSpeed);
        animator.SetFloat("Speed", normalizedSpeed);

        animator.SetBool("IsJumping", isJumping);
    }
}