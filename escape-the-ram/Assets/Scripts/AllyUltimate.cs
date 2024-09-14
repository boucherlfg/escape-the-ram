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

    private List<EnemyEntity> pulledEnemies = new List<EnemyEntity>();
    private Animate _cooldownTimerAnim;
    private Animate _pullTimerAnim;

    private bool _isUltimateActive = false;

    private void Start()
    {
        EventManager.AddEventListener("CastUltimate", HandleCastUltimate);
    }

    private void Update()
    {
        // Debug input
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Dispatch("CastUltimate", null);
        }
    }

    private void FixedUpdate()
    {
        if (!_isUltimateActive)
            return;

        for (int i = 0; i < pulledEnemies.Count; i++) 
        {
            EnemyEntity enemy = pulledEnemies[i];
            if (enemy != null)
            {
                Rigidbody2D rb = enemy.GetRigidbody2D();
                Vector2 dirToPlayer = ((Vector2)transform.position - rb.position).normalized;
                float disToPlayer = Vector2.Distance(rb.position, (Vector2)transform.position);
                rb.velocity = (dirToPlayer * _ultContractionSpeed) * Mathf.Min(disToPlayer/2f, 1f) * Time.fixedDeltaTime;
            }
        }
    }

    private void HandleCastUltimate(BytesData data) 
    {
        // Wait for timer to expire before starting again.
        if (!_isUltimateActive && _cooldownTimerAnim != null)
            return;

        World world = World.Instance;
        pulledEnemies = world.GetAllEnemies();

        // cooldown timer. Null means its done.
        _cooldownTimerAnim = Animate.Delay(_ultCooldown, () => { _cooldownTimerAnim = null; }, true);

        // Pull duration timer. Pull for x seconds then knockback.
        _pullTimerAnim = Animate.Delay(_ultDuration, () => 
        { 
            _pullTimerAnim = null;
            KnockbackEnemies();
            _isUltimateActive = false;
        }, true);

        if (!_isUltimateActive)
        {
            // Cast: Start pulling all.
            PrepareEnemies();
            _isUltimateActive = true;
        }
        else 
        {
            // Recast: Knockback all.
            KnockbackEnemies();
            _isUltimateActive = false;
            // Stop pull timer since we already recast.
            _pullTimerAnim.Stop(false);
        }
    }

    private void PrepareEnemies() 
    {
        for (int i = 0; i < pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = pulledEnemies[i];
            SetupEnemyPull(enemy);
        }
    }

    private void KnockbackEnemies()
    {
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

    private void SetupEnemyPull(EnemyEntity enemy) 
    {
        enemy.SetEnemyMovementEnabled(false);
    }

    private void KnockbackEnemy(EnemyEntity enemy)
    {
        Rigidbody2D rb = enemy.GetRigidbody2D();
        Vector2 dirToPlayer = ((Vector2)transform.position - rb.position).normalized;
        rb.velocity = -dirToPlayer * _ultKnockbackStrength * Random.Range(0.8f, 1.2f);
    }
}
