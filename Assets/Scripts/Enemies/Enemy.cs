using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 25f;
    public float speed = 2f;
    public float damage = 20f;

    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return; // Stop chasing if player is gone

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
            Destroy(gameObject);
    }
}