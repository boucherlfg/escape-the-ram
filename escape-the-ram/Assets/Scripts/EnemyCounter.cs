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
    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddEventListener("OnDeath", HandleDeath);
    }

    void HandleDeath(BytesData data){
        isDead = true;
        EventManager.RemoveEventListener("OnDeath", HandleDeath);
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead) return;
        var hits = Physics2D.OverlapCircleAll(transform.position, maxEnemyDistance).Count(x => x.GetComponent<EnemyEntity>());
        if(hits >= maxEnemyCount) 
        {
            EventManager.Dispatch("OnDeath", null);
            gameObject.SetActive(false);
        }
        
        var color = screen.color;
        color.a = ((float)hits) / maxEnemyCount;
        screen.color = color;

        move.Speed = move.MaxSpeed * (1 - (hits / maxEnemyCount));
    }
}
