using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 20f;
    public float damageCooldown = 1f; // Seconds between damage ticks

    private float lastDamageTime;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            lastDamageTime = Time.time;
            Debug.Log("Enemy dealt " + damage + " damage to player!");
        }
    }
}