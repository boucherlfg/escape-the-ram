using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bytes;

public class EnemyCounter : MonoBehaviour
{
    private Camera mainCamera;
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

    private void Start()
    {
        mainCamera = Camera.main;
    }

    

    private void Update()
    {
        if(_isDead || !_canBeKilled) 
            return;

        var hits = (int)Physics2D.OverlapCircleAll(transform.position, maxEnemyDistance).Where(x => x.GetComponent<EnemyEntity>()).Sum(x => x.GetComponent<EnemyEntity>().transform.localScale.magnitude);
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

    }

    public void SetCanBeKilled(bool canBeKilled) 
    {
        //_canBeKilled = canBeKilled;
    }
}
