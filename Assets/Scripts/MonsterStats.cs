using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    [Header("Monster Settings")]
    public int damage = 10;
    public float damageCooldown = 1f;

    private float lastDamageTime = -Mathf.Infinity;

    private void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject player)
    {
        if (!player.CompareTag("Player")) return;

        // Check cooldown
        if (Time.time - lastDamageTime < damageCooldown) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }
}