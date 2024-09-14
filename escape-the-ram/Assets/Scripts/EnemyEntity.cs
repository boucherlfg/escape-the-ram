using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    private Rigidbody2D _rb;
    private EnemyMovement _enemyMovement;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemyMovement = GetComponent<EnemyMovement>();
    }

    public Rigidbody2D GetRigidbody2D() 
    {
        return _rb;
    }

    public void SetEnemyMovementEnabled(bool enabled) 
    {
        _enemyMovement.enabled = enabled;
    }
}
