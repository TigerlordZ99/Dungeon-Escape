using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float damage = 1;
    public float range = 1.5f;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(transform.position,range);

            foreach(Collider2D hit in hits)
            {
                Enemy e = hit.GetComponent<Enemy>();

                if(e != null)
                    e.TakeDamage(damage);
            }
        }
    }
}