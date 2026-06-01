using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemySettings", menuName = "Enemy/Enemy Settings")]
public class EnemySettings : ScriptableObject
{
    [field: SerializeField] public float targetDistance { get; private set; } = 2f; 
    [field: SerializeField] public float HP { get; private set; } = 3f;
    [field: SerializeField] public float AttackDuration { get; private set; } = 3f;
}
