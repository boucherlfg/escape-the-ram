using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bytes;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField]
    private float maxEnemyDistance;
    [SerializeField]
    private SpriteRenderer screen;
    [SerializeField]
    private AllyMovement move;
    [SerializeField]
    int maxEnemyCount = 10;

    int currentCount = 0;

    private bool _isDead;
    private bool _canBeKilled = true;

    private void Update()
    {
        if(_isDead || !_canBeKilled) 
            return;

        var hits = Physics2D.OverlapCircleAll(transform.position, maxEnemyDistance).Count(x => x.GetComponent<EnemyEntity>());
        if (currentCount != hits)
        {
            currentCount = hits;
            EventManager.Dispatch("OnDamageChanged", new FloatDataBytes(currentCount / (float)maxEnemyCount));
        }
        if(hits >= maxEnemyCount) 
        {
            _isDead = true;
            gameObject.SetActive(false);
            EventManager.Dispatch("OnDeath", null);
        }
        
        var color = screen.color;
        color.a = ((float)hits) / maxEnemyCount;
        screen.color = color;

        move.Speed = move.MaxSpeed * (1 - (hits / maxEnemyCount));
    }

    public void SetCanBeKilled(bool canBeKilled) 
    {
        //_canBeKilled = canBeKilled;
    }
}
