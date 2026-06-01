using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lookSensitivity = 1.5f;
    [SerializeField] private float jumpForce = 5f;
    private float movementX;
    private float movementY;
    private float mouseX;
    private bool isGrounded;
    RaycastHit hit;
    [SerializeField] private Rigidbody rb;
    
    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void OnLook(InputValue lookValue)
    {
        mouseX = lookValue.Get<Vector2>().x;
    }

    void OnClick(InputValue value)
    {

    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {   
        // 1. Calculate Ground Truth Directions
        // Since we are strictly using the Rigidbody, we use rb.rotation for directions
        Vector3 forward = rb.rotation * Vector3.forward;
        Vector3 right = rb.rotation * Vector3.right;

        // 2. Move Forward/Backward (W/S) and Strafe Left/Right (A/D)
        Vector3 moveDir = forward * movementY + right * movementX;
        
        if (moveDir.magnitude > 1f) moveDir.Normalize();
        Vector3 movement = moveDir * speed * Time.fixedDeltaTime;
        
        // Pass to the Rigidbody
        rb.MovePosition(rb.position + movement);

        // 3. Camera/Player Rotation using Mouse (NO Quaternion required)
        // Set angular velocity to rotate entirely through physics
        rb.angularVelocity = new Vector3(0f, mouseX * lookSensitivity, 0f);
        
        // 4. Ground check
        isGrounded = false;
        // TransformDirection is okay here since it's just a check
        Vector3 down = transform.TransformDirection(Vector3.down); 
        
        // Raycast down
        if (Physics.Raycast(transform.position, down, out hit, 0.75f))
        {
            Debug.DrawRay(transform.position, down * hit.distance, Color.red);
            if (hit.collider.CompareTag("Ground"))
            {
                isGrounded = true;
                Debug.Log("Player is grounded");
            }
        }
    }

}



