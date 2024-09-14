using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AllyMovement : SingletonBehaviour<AllyMovement>
{
    public float maxDistance = 5;
    public int maxNeighbours = 5;
    [SerializeField]
    private float maxSpeed = 3;
    private float speed = 3;
    public float MaxSpeed => maxSpeed;
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    private Rigidbody2D _body;

    private bool _isMoving = true;

    private void Start()
    {
        speed = maxSpeed;
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!_isMoving)
            return;

        var hits = Physics2D.OverlapCircleAll(transform.position, maxDistance)
                            .Where(x => x.GetComponent<EnemyMovement>())
                            .Take(maxNeighbours).ToList();
        
        var vector = hits.Aggregate(Vector3.zero, (iter, other) => 
                          iter += (other.transform.position - transform.position).normalized 
                          * (maxDistance - Mathf.Clamp((other.transform.position - transform.position).magnitude, 0, maxDistance)));

        vector.Normalize();

        _body.velocity = -vector * speed;
    }

    public void SetIsMoving(bool isMoving) 
    {
        _isMoving = isMoving;
        _body.velocity = Vector2.zero;
    }
}
