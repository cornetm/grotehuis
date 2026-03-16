using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    [Header("Monster Settings")]
    public int damage = 10;                // Hoeveel schade het monster doet
    public float damageCooldown = 1f;      // Tijd tussen schade momenten

    private float lastDamageTime = -Mathf.Infinity; // Houdt bij wanneer speler voor het laatst schade kreeg

    // Voor triggers
    private void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    // Voor colliders
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
            lastDamageTime = Time.time; // reset cooldown
        }
    }
}