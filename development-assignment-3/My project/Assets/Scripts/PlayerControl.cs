using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f; 

    [Header("References")]
    [SerializeField] private GameObject spotlight; 
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerCamera; 
    
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isHoldingFlashlight = false;
    private bool isSprinting = false;
    private float verticalVelocity;
    
    [Header("Top-Down Camera")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 15f, -7f);

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        Cursor.lockState = CursorLockMode.Locked;
        
        if (spotlight != null)
        {
            spotlight.SetActive(isHoldingFlashlight);
        }

        if (playerCamera != null)
        {
            // Unparent the camera so it doesn't spin wildly when the player rotates
            playerCamera.parent = null; 
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnAttack(InputValue value) 
    {
        if (value.isPressed)
        {
            isHoldingFlashlight = !isHoldingFlashlight;
            
            if (animator != null)
            {
                animator.SetBool("IsHoldingFlashlight", isHoldingFlashlight);
            }

            if (!isHoldingFlashlight)
            {
                if (spotlight != null)
                {
                    spotlight.SetActive(false);
                }
            }
        }
    }

    public void ToggleTorch()
    {
        if (isHoldingFlashlight && spotlight != null)
        {
            spotlight.SetActive(true);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateAnimations();
        HandleFlashlightRaycast();
    }

    private void LateUpdate()
    {
        if (playerCamera != null)
        {
            playerCamera.position = Vector3.Lerp(playerCamera.position, transform.position + cameraOffset, Time.deltaTime * 5f);
            
            playerCamera.LookAt(transform.position);
        }
    }

    private void HandleFlashlightRaycast()
    {
        if (isHoldingFlashlight && spotlight != null && spotlight.activeSelf)
        {
            float flashlightRange = 15f;
            float beamRadius = 1.5f;
            
            // Blendes the players forward with the spotlights forward direction.
            Vector3 aimDirection = Vector3.Lerp(transform.forward, spotlight.transform.forward, 0.5f).normalized;

            if (Physics.SphereCast(spotlight.transform.position, beamRadius, aimDirection, out RaycastHit hit, flashlightRange))
            {
                if (hit.collider.TryGetComponent(out Enemy enemy))
                {
                    enemy.SetBlinded(true);
                }
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        
        Vector3 horizontalMovement = new Vector3(move.x, 0f, move.z);
        if (horizontalMovement.magnitude > 1f)
        {
            horizontalMovement.Normalize();
            move.x = horizontalMovement.x;
            move.z = horizontalMovement.z;
        }

        // Gravity (it was very wierd with character controller)
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f; 
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        float currentSpeed = moveSpeed * (isSprinting ? 2f : 1f);
        Vector3 finalMove = move * currentSpeed;
        finalMove.y = verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float horizontalRotationAmount = lookInput.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, horizontalRotationAmount, 0f);
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            float animMultiplier = isSprinting ? 2f : 1f;
            animator.SetFloat("VelocityX", moveInput.x * animMultiplier);
            animator.SetFloat("VelocityY", moveInput.y * animMultiplier);
        }
    }
}
