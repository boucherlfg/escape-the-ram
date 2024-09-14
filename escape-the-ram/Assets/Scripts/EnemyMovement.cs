using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public float detectDistance = 5;
    public int maxNeighbours = 5;
    public float speed = 3;

    private Rigidbody2D _body;

    private void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        var speed = this.speed + 1 - transform.localScale.magnitude;
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
                            .Where(x => x.GetComponent<EnemyMovement>() && x.gameObject != gameObject)
                            .Take(maxNeighbours).ToList();

        var vector = Vector3.zero;
        foreach (var hit in hits)
        {
            var delta = hit.transform.position - transform.position;
            var force = transform.localScale.magnitude / 2 - Mathf.Clamp(delta.magnitude, 0, transform.localScale.magnitude / 2);
            vector += delta.normalized * force;
        }
        return -vector.normalized;
    }
}