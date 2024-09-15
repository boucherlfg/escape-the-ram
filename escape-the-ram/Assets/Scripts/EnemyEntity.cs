using Bytes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyDeathPrefab;
    [SerializeField]
    private float screenBoundsMargin = 5;
    [SerializeField]
    private float minimumRandomSize = 0.5f;
    [SerializeField]
    private float maximumRandomSize = 2f;
    [SerializeField]
    private AudioClip death;
    private Rigidbody2D _rb;
    private EnemyMovement _enemyMovement;
    private SpriteRenderer _spriteRenderer;

    private bool _isKnockbacking = false;
    private bool _isBeingDestroyedByUlt = false;
    private Vector2 screenBounds; // To detect screen edges

    private Camera _mainCam;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mainCam = Camera.main;
    }

    private void Start()
    {
        transform.localScale *= Random.Range(minimumRandomSize, maximumRandomSize);
    }

    private void Update()
    {
        var topRight = new Vector2(Screen.width, Screen.height);
        var min = _mainCam.ScreenToWorldPoint(Vector2.one * screenBoundsMargin);
        var max = _mainCam.ScreenToWorldPoint(topRight - topRight.normalized * screenBoundsMargin);
        var cameraRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        if (_isKnockbacking && !_isBeingDestroyedByUlt)
        {
            // Check for horizontal bounds (left and right)
            if (!cameraRect.Contains(transform.position))
            {
                _rb.velocity = new Vector2(-_rb.velocity.x, _rb.velocity.y) * 0.25f;
                _isBeingDestroyedByUlt = true;
            }

            if (_isBeingDestroyedByUlt) 
            {
                DyingAnimation();
            }
        }    
    }

    private void DyingAnimation()
    {
        Instantiate(_enemyDeathPrefab, transform.position, Quaternion.identity);

        AudioSource.PlayClipAtPoint(death, transform.position);
        Color startColor = _spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Animate.LerpSomething(0.5f, (progress) =>
        {
            transform.position += Vector3.one * Random.Range(0.01f, 0.01f) * Random.Range(-1, 2);
            _spriteRenderer.color = Color.Lerp(startColor, endColor, progress*progress);
        }, () =>
        {
            EventManager.Dispatch("OnEnemyDeath", null);
            Destroy(this.gameObject);
        }, true);
    }

    public void UltKnockback(Vector2 direction, float strength) 
    {
        _isKnockbacking = true;
        _rb.velocity = -direction * (strength / (transform.localScale.x) );
    }

    public Rigidbody2D GetRigidbody2D() 
    {
        return _rb;
    }

    public void SetEnemyIsBeingKnocbacked(bool isBeingKnockbacked) 
    {
        _isKnockbacking = isBeingKnockbacked;
        _enemyMovement.enabled = isBeingKnockbacked;
    }
}
