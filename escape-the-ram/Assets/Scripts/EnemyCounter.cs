using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bytes;

public class EnemyCounter : MonoBehaviour
{
    private bool isDead;
    [SerializeField]
    private float maxEnemyDistance;
    [SerializeField]
    private SpriteRenderer screen;
    [SerializeField]
    private AllyMovement move;
    [SerializeField]
    int maxEnemyCount = 10;

    private void Update()
    {
        if(isDead) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, maxEnemyDistance).Count(x => x.GetComponent<EnemyEntity>());
        if(hits >= maxEnemyCount) 
        {
            isDead = true;
            gameObject.SetActive(false);
            EventManager.Dispatch("OnDeath", null);
        }
        
        var color = screen.color;
        color.a = ((float)hits) / maxEnemyCount;
        screen.color = color;

        move.Speed = move.MaxSpeed * (1 - (hits / maxEnemyCount));
    }
}
