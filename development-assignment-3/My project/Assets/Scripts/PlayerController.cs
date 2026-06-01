using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    private Vector2 inputVector;
    [SerializeField] private float speed = 2f;
    private int speedModifier = 1;
    private bool isTorchEquipped;
    
    [SerializeField] private GameObject torch;
    
    // Hash
    private int velocityXHash;
    private int velocityYHash;
    private int torchHash;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        velocityXHash = Animator.StringToHash("VelocityX");
        velocityYHash = Animator.StringToHash("VelocityY");
        torchHash = Animator.StringToHash("IsTorchEquipped");
    }
    
    void Update()
    {
        Vector3 movement = transform.forward * inputVector.y + transform.right * inputVector.x;
        characterController.Move(speed * speedModifier * movement * Time.deltaTime);
        characterController.Move(Physics.gravity * Time.deltaTime);
        
        animator.SetFloat(velocityXHash, inputVector.x * speedModifier);
        animator.SetFloat(velocityYHash, inputVector.y * speedModifier);
    }

    void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>().normalized;
    }
    
    void OnSprint(InputValue value)
    {
        speedModifier = value.isPressed ? 2 : 1;
    }

    void OnAttack(InputValue value)
    {
        isTorchEquipped = !isTorchEquipped;
        animator.SetBool(torchHash, isTorchEquipped);
        
    }
    public void ToggleTorch()
    {
       torch.SetActive(!torch.activeSelf);
    }
}
