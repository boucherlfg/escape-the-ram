using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : SingletonBehaviour<World>
{
    private List<EnemyEntity> _allEnemies;

    protected override void Awake()
    {
        base.Awake();

        _allEnemies = new List<EnemyEntity>(gameObject.GetComponentsInChildren<EnemyEntity>());
    }

    public List<EnemyEntity> GetAllEnemies() 
    {
        // Fetch enemies everytime
        _allEnemies = new List<EnemyEntity>(gameObject.GetComponentsInChildren<EnemyEntity>());
        return _allEnemies;
    }
}
