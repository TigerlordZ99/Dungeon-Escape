using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 8f;
    public float separationRadius = 1.5f;
    public float separationForce = 3f;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        Vector2 chaseDir = ((Vector2)player.position - (Vector2)transform.position).normalized;

        Vector2 separation = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (col.GetComponent<Enemy>() == null) continue;

            Vector2 pushDir = (Vector2)(transform.position - col.transform.position);
            float dist = pushDir.magnitude;
            if (dist > 0)
                separation += pushDir.normalized / dist;
        }

        Vector2 finalDir = chaseDir + separation * separationForce;
        transform.position = Vector2.MoveTowards(
            transform.position,
            (Vector2)transform.position + finalDir,
            speed * Time.deltaTime
        );
    }
}