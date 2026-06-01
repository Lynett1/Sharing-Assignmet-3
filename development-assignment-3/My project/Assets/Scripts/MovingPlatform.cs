using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 movementDistance = new Vector3(0f, 0f, 0f);
    [SerializeField] private float speed = 1f;
    
    private Vector3 startPosition;
    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float pingPong = Mathf.PingPong(Time.time * speed, 1f);
        float smoothStep = Mathf.SmoothStep(0f, 1f, pingPong);
        Vector3 newPosition = startPosition + (movementDistance * smoothStep);
        rb.MovePosition(newPosition);
    }
}