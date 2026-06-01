using System.Collections;
using UnityEngine;

public class SpecialAttackManager : MonoBehaviour
{
    [SerializeField] private GameObject gravityFieldPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float cooldownBetweenAttacks = 2f;
    private GameObject currentField;
    private Coroutine groupAttackCoroutine;
    private int lockedEnemiesCount = 0;

    private void OnEnable()
    {
        //Listen to the specific events
        Enemy2.OnTargetLockAcquired += EnemyAcquiredLock;
        Enemy2.OnTargetLockLost += EnemyLostLock;
    }

    private void OnDisable()
    {
        Enemy2.OnTargetLockAcquired -= EnemyAcquiredLock;
        Enemy2.OnTargetLockLost -= EnemyLostLock; 
    }

    private void EnemyAcquiredLock()
    {
        lockedEnemiesCount++;
        if (lockedEnemiesCount >= 3 && groupAttackCoroutine == null)
        {
            groupAttackCoroutine = StartCoroutine(GroupAttackRoutine());
        }
    }

    private void EnemyLostLock()
    {
        lockedEnemiesCount--;
    }

    private IEnumerator GroupAttackRoutine()
    {
        while (lockedEnemiesCount >= 3)
        {
            if (currentField == null)
            {
                Debug.Log("3+ Enemies locked on! Group attack triggered!");
                currentField = Instantiate(gravityFieldPrefab, player.transform.position, Quaternion.identity);
                
                Destroy(currentField, 6f); 
                
                yield return new WaitForSeconds(6f + cooldownBetweenAttacks); 
            }
            else
            {
                yield return null;
            }
        }
        
        groupAttackCoroutine = null;
    }
}
