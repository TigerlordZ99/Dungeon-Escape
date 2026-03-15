using UnityEngine;

// Chase-only enemy — no combat. Player evades; enemies are pure pressure.
public class Enemy : MonoBehaviour
{
    public float speed = 2f;

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
            // Re-find if player reference lost (e.g. scene reload edge case)
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );
    }
}
