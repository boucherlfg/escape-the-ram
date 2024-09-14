using Bytes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    private Rigidbody2D _rb;
    private EnemyMovement _enemyMovement;
    private SpriteRenderer _spriteRenderer;

    private bool _isKnockbacking = false;
    private bool _isBeingDestroyedByUlt = false;
    private Vector2 screenBounds; // To detect screen edges

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    private void Update()
    {
        if (_isKnockbacking && !_isBeingDestroyedByUlt)
        {
            // Check for horizontal bounds (left and right)
            if (transform.position.x > screenBounds.x || transform.position.x < -screenBounds.x)
            {
                _rb.velocity = new Vector2(-_rb.velocity.x, _rb.velocity.y) * 0.25f;
                _isBeingDestroyedByUlt = true;
            }

            // Check for vertical bounds (top and bottom)
            if (transform.position.y > screenBounds.y || transform.position.y < -screenBounds.y)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, -_rb.velocity.y) * 0.25f;
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
        Color startColor = _spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Animate.LerpSomething(0.5f, (progress) =>
        {
            transform.position += Vector3.one * Random.Range(0.01f, 0.01f) * Random.Range(-1, 2);
            _spriteRenderer.color = Color.Lerp(startColor, endColor, progress*progress);
        }, () =>
        {
            Destroy(this.gameObject);
        }, true);
    }

    public void UltKnockback(Vector2 direction, float strength) 
    {
        _isKnockbacking = true;
        _rb.velocity = -direction * (strength / (transform.localScale.x*1.5f) );
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
