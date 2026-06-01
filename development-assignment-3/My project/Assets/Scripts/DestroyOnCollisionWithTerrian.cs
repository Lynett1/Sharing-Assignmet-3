using UnityEngine;

public class DestroyOnCollisionWithTerrian : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
