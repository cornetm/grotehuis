using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MonsterStats : MonoBehaviour
{
    [Header("Monster Settings")]
    public int damage = 10;
    public float damageCooldown = 1f;

    private float lastDamageTime = -Mathf.Infinity;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true; // box collider als trigger
        }
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void TryDamagePlayer(GameObject target)
    {
        if (!target.CompareTag("Player")) return;

        if (Time.time - lastDamageTime < damageCooldown) return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }
}