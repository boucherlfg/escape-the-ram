using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Bytes;

public class AllyUltimate : MonoBehaviour
{
    [SerializeField] private AudioClip _charging;
    [SerializeField] private AudioClip _explosion;
    [SerializeField] private float _ultDuration = 3f;
    [SerializeField] private float _ultCooldown = 10f;
    [SerializeField] private float _ultContractionSpeed = 2f;
    [SerializeField] private float _ultKnockbackStrength = 9f;
    [SerializeField] private float _maximumCameraShakeIntensity = 0.05f;
    [SerializeField] private float _fullChargeBonus = 2;

    private World _world;
    private Camera _mainCamera;
    private AllyMovement _allyMovement;
    private EnemyCounter _enemyCounter;

    private List<EnemyEntity> _pulledEnemies = new List<EnemyEntity>();
    private Vector2[] _enemiesStartingPositions; // Cached Positions of enemies when starting the ult.
    private Animate _cooldownTimerAnim;
    private Animate _pullTimerAnim;

    private float _currentCameraShakeIntensity = 0;
    private bool _isUltimateActive = false;
    private float _progress;
    bool isFull;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _allyMovement = GetComponent<AllyMovement>();
        _enemyCounter = GetComponent<EnemyCounter>();
    }

    private void Start()
    {
        _world = World.Instance;

        EventManager.AddEventListener("OnCastUltimate", HandleCastUltimate);
        StartCoroutine(CameraShake());
    }

    IEnumerator CameraShake()
    {
        while (true)
        {
            _mainCamera.transform.position += (Vector3)Random.insideUnitCircle * _currentCameraShakeIntensity;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update()
    {
        _pulledEnemies.RemoveAll(x => !x);
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
        audio.clip = _charging;
        audio.Play();
        _allyMovement.SetIsMoving(false);
        _enemyCounter.SetCanBeKilled(false);
        _isUltimateActive = true;

        _pulledEnemies = _world.GetAllEnemies();

        // Get all initial positions of enemies.
        _enemiesStartingPositions = new Vector2[_pulledEnemies.Count];
        for (int i = 0; i < _pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = _pulledEnemies[i];
            if (enemy != null)
            {
                _enemiesStartingPositions[i] = enemy.transform.position;
            }
        }

        // Pull duration timer. Pull for x seconds then knockback.
        _pullTimerAnim = Animate.LerpSomething(_ultDuration, (progress)=> 
        {
            EventManager.Dispatch("OnUltimatePullUpdate", new FloatDataBytes(progress));
            _currentCameraShakeIntensity = _maximumCameraShakeIntensity - _maximumCameraShakeIntensity * (1 - progress);
            // Don't go too close to player.
            progress = Mathf.Min(progress, 0.9f);
            this._progress = progress;

            for (int i = 0; i < _pulledEnemies.Count; i++)
            {
                EnemyEntity enemy = _pulledEnemies[i];
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
            isFull = true;
            SecondCastKnockbackEnemies();
        }, true);

        for (int i = 0; i < _pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = _pulledEnemies[i];
            if (enemy != null)
                enemy.SetEnemyIsBeingKnocbacked(false);
        }
    }

    private void SecondCastKnockbackEnemies()
    {
        _isUltimateActive = false;
        _currentCameraShakeIntensity = 0;
        EventManager.Dispatch("OnUltimatePush", null);
        EventManager.Dispatch("OnUltimatePullUpdate", new FloatDataBytes(0f));
        var audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = _explosion;
        audio.Play();
        _allyMovement.SetIsMoving(true);
        // Wait a bit for enemies to be sent away before starting to detect death again.
        Animate.Delay(0.2f, () => { _enemyCounter.SetCanBeKilled(true); });

        // Stop pull timer since we already recast.
        _pullTimerAnim?.Stop(false);

        EventManager.Dispatch("OnButtonStateChanged", new BoolDataBytes(false));
        StartCoroutine(Cooldown());
        // cooldown timer. Null means its done.
        _cooldownTimerAnim = Animate.Delay(_ultCooldown, () => 
        {
            EventManager.Dispatch("OnButtonStateChanged", new BoolDataBytes(true));
            _cooldownTimerAnim = null;
        }, true);

        for (int i = 0; i < _pulledEnemies.Count; i++)
        {
            EnemyEntity enemy = _pulledEnemies[i];
            Vector2 dirToPlayer = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;
            enemy.UltKnockback(dirToPlayer, _ultKnockbackStrength * 2 * _progress * _progress * (isFull ? _fullChargeBonus : 1));
        }
        isFull = false;

        // After a bit of knockback, we re-enable the enemy movement.
        Animate.Delay(1f, () => 
        {
            for (int i = 0; i < _pulledEnemies.Count; i++)
            {
                EnemyEntity enemy = _pulledEnemies[i];
                if(enemy != null)
                    enemy.SetEnemyIsBeingKnocbacked(true);
            }
        }, true);
    }

    IEnumerator Cooldown()
    {
        for (float f = 0; f < 1; f += Time.deltaTime/_ultCooldown)
        {
            EventManager.Dispatch("Cooldown", new FloatDataBytes(f));
            yield return null;
        }
    }
}
