using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Bytes;

public class AllyUltimate : MonoBehaviour
{
    [SerializeField] private AudioClip charging;
    [SerializeField] private AudioClip explosion;
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
    private float progress;
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
        pulledEnemies.RemoveAll(x => !x);
        // Debug input
        /*if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Dispatch("OnCastUltimate", null);
        }*/
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
        EventManager.Dispatch("OnUltimatePull", null);
        var audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = charging;
        audio.Play();
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
            this.progress = progress;

            for (int i = 0; i < pulledEnemies.Count; i++)
            {
                EnemyEntity enemy = pulledEnemies[i];
                if (enemy != null)
                {
                    Rigidbody2D rb = enemy.GetRigidbody2D();
                    // If enemy is bigger, progress is slower and reaches less close to the player.
                    float usedProgress = Mathf.Min(progress, progress / rb.transform.localScale.x * 1.5f);
                    Vector2 dirToPlayer = ((Vector2)transform.position - rb.position).normalized;
                    enemy.transform.position = Vector3.Lerp(_enemiesStartingPositions[i], transform.position, usedProgress);
                }
            }
        },
        () =>
        {
            _isUltimateActive = false;
            _pullTimerAnim = null;
            SecondCastKnockbackEnemies();
        }, true);

        for (int i = 0; i < pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = pulledEnemies[i];
            if (enemy != null)
                enemy.SetEnemyIsBeingKnocbacked(false);
        }
    }

    private void SecondCastKnockbackEnemies()
    {
        EventManager.Dispatch("OnUltimatePush", null);
        EventManager.Dispatch("OnUltimatePullUpdate", new FloatDataBytes(0f));
        var audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = explosion;
        audio.Play();
        _allyMovement.SetIsMoving(true);
        // Wait a bit for enemies to be sent away before starting to detect death again.
        Animate.Delay(0.2f, () => { _enemyCounter.SetCanBeKilled(true); });

        _isUltimateActive = false;

        // Stop pull timer since we already recast.
        _pullTimerAnim?.Stop(false);
        // cooldown timer. Null means its done.
        _cooldownTimerAnim = Animate.Delay(_ultCooldown, () => { _cooldownTimerAnim = null; }, true);

        for (int i = 0; i < pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = pulledEnemies[i];
            Vector2 dirToPlayer = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;
            enemy.UltKnockback(dirToPlayer, _ultKnockbackStrength * progress);
        }

        // After a bit of knockback, we re-enable the enemy movement.
        Animate.Delay(1f, () => 
        {
            for (int i = 0; i < pulledEnemies.Count; i++)
            {
                EnemyEntity enemy = pulledEnemies[i];
                if(enemy != null)
                    enemy.SetEnemyIsBeingKnocbacked(true);
            }
        }, true);
    }
}
