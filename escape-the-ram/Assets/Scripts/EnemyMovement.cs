using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public float detectDistance = 5;
    public int maxNeighbours = 5;
    public float maxFleeDistance = 1;
    public float speed = 3;

    private Rigidbody2D _body;

    private void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        var vector = FleeVector() + FollowVector();
        _body.velocity = speed * vector;
    }

    private Vector2 FollowVector()
    {
        var player = AllyMovement.Instance;
        return -(transform.position - player.transform.position).normalized;
    }

    private Vector2 FleeVector()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, detectDistance)
                            .Where(x => x.GetComponent<EnemyMovement>())
                            .Take(maxNeighbours).ToList();

        var vector = hits.Aggregate(Vector3.zero, (iter, other) =>
                          iter += (other.transform.position - transform.position).normalized
                          * (detectDistance - Mathf.Clamp((other.transform.position - transform.position).magnitude, 0, maxFleeDistance)));

        return -vector.normalized;
    }
}