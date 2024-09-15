using Bytes;
using System;
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
    [SerializeField]
    private float minSpeed = 1;
    private float speed = 3;
    private float targetSpeed = 3;

    private Rigidbody2D _body;

    private bool _isMoving = true;

    private void Start()
    {
        speed = maxSpeed;
        targetSpeed = speed;
        _body = GetComponent<Rigidbody2D>();
        EventManager.AddEventListener("OnDamageChanged", HandleDamageChanged);
    }

    private void HandleDamageChanged(BytesData data)
    {
        var value = (data as FloatDataBytes).FloatValue;

        targetSpeed = maxSpeed - (maxSpeed - minSpeed) * value;
    }

    private void Update()
    {
        if (!_isMoving)
            return;

        speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime);

        var hits = Physics2D.OverlapCircleAll(transform.position, maxDistance)
                            .Where(x => x.GetComponent<EnemyMovement>())
                            .Take(maxNeighbours).ToList();

        if (hits.Count <= 0) return;
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
