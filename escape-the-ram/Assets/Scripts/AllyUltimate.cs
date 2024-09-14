using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Bytes;

public class AllyUltimate : MonoBehaviour
{
    [SerializeField] private float _ultDuration = 3f;
    [SerializeField] private float _ultCooldown = 10f;
    [SerializeField] private float _ultContractionSpeed = 2f;
    [SerializeField] private float _ultKnockbackStrength = 9f;

    private World _world;
    private AllyMovement _allyMovement;
    private EnemyCounter _enemyCounter;

    private List<EnemyEntity> pulledEnemies = new List<EnemyEntity>();
    private Vector2[] _enemiesStartingPositions; // Cached Positions of enemies when starting the ult.
    private Animate _cooldownTimerAnim;
    private Animate _pullTimerAnim;

    private bool _isUltimateActive = false;

    private void Awake()
    {
        _allyMovement = GetComponent<AllyMovement>();
        _enemyCounter = GetComponent<EnemyCounter>();
    }

    private void Start()
    {
        _world = World.Instance;

        EventManager.AddEventListener("OnCastUltimate", HandleCastUltimate);
    }

    private void Update()
    {
        // Debug input
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Dispatch("OnCastUltimate", null);
        }
    }

    private void HandleCastUltimate(BytesData data) 
    {
        // Wait for timer to expire before starting again.
        if (_cooldownTimerAnim != null)
            return;

        if (!_isUltimateActive)
        {
            FirstUltimateCast();
        }
        else 
        {
            SecondCastKnockbackEnemies();
        }
    }

    private void FirstUltimateCast() 
    {
        _allyMovement.SetIsMoving(false);
        _enemyCounter.SetCanBeKilled(false);
        _isUltimateActive = true;

        pulledEnemies = _world.GetAllEnemies();

        // Get all initial positions of enemies.
        _enemiesStartingPositions = new Vector2[pulledEnemies.Count];
        for (int i = 0; i < pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = pulledEnemies[i];
            if (enemy != null)
            {
                _enemiesStartingPositions[i] = enemy.transform.position;
            }
        }

        // Pull duration timer. Pull for x seconds then knockback.
        _pullTimerAnim = Animate.LerpSomething(_ultDuration, (progress)=> 
        {
            EventManager.Dispatch("OnUltimatePullUpdate", new FloatDataBytes(progress));

            // Don't go too close to player.
            progress = Mathf.Min(progress, 0.9f);

            for (int i = 0; i < pulledEnemies.Count; i++)
            {
                EnemyEntity enemy = pulledEnemies[i];
                if (enemy != null)
                {
                    Rigidbody2D rb = enemy.GetRigidbody2D();
                    Vector2 dirToPlayer = ((Vector2)transform.position - rb.position).normalized;
                    enemy.transform.position = Vector3.Lerp(_enemiesStartingPositions[i], transform.position, progress);
                }
            }
        },
        () =>
        {
            EventManager.Dispatch("OnUltimatePullUpdate", new FloatDataBytes(1f));
            _isUltimateActive = false;
            _pullTimerAnim = null;
            SecondCastKnockbackEnemies();
        }, true);

        for (int i = 0; i < pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = pulledEnemies[i];
            SetupEnemyPull(enemy);
        }
    }

    private void SetupEnemyPull(EnemyEntity enemy)
    {
        enemy.SetEnemyMovementEnabled(false);
    }

    private void SecondCastKnockbackEnemies()
    {
        _allyMovement.SetIsMoving(true);
        // Wait a bit for enemies to be sent away before starting to detect death again.
        Animate.Delay(0.1f, () => { _enemyCounter.SetCanBeKilled(true); });

        _isUltimateActive = false;

        // Stop pull timer since we already recast.
        _pullTimerAnim?.Stop(false);
        // cooldown timer. Null means its done.
        _cooldownTimerAnim = Animate.Delay(_ultCooldown, () => { _cooldownTimerAnim = null; }, true);

        for (int i = 0; i < pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = pulledEnemies[i];
            KnockbackEnemy(enemy);
        }

        // After a bit of knockback, we re-enable the enemy movement.
        Animate.Delay(1f, () => 
        {
            for (int i = 0; i < pulledEnemies.Count; i++)
            {
                EnemyEntity enemy = pulledEnemies[i];
                enemy.SetEnemyMovementEnabled(true);
            }
        }, true);
    }

    private void KnockbackEnemy(EnemyEntity enemy)
    {
        Rigidbody2D rb = enemy.GetRigidbody2D();
        Vector2 dirToPlayer = ((Vector2)transform.position - rb.position).normalized;
        rb.velocity = -dirToPlayer * _ultKnockbackStrength * Random.Range(0.8f, 1.2f);
    }
}
