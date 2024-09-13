using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AllyMovement : SingletonBehaviour<AllyMovement>
{
    public float maxDistance = 5;
    public int maxNeighbours = 5;
    public float speed = 3;

    private Rigidbody2D _body;

    private void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, maxDistance)
                            .Where(x => x.GetComponent<EnemyMovement>())
                            .Take(maxNeighbours).ToList();
        
        var vector = hits.Aggregate(Vector3.zero, (iter, other) => 
                          iter += (other.transform.position - transform.position).normalized 
                          * (maxDistance - Mathf.Clamp((other.transform.position - transform.position).magnitude, 0, maxDistance)));

        vector.Normalize();

        _body.velocity = -vector * speed;
    }
}
