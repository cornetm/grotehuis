using UnityEngine;

public class MonsterChasePlayer : MonoBehaviour
{
    [Header("References")]
    public Transform player;             // Zet hier je speler in
    public Transform stopObject;         // Object waar het monster stopt
    public float moveSpeed = 5f;

    [Header("Animator")]
    public Animator animator;
    public string runBoolName = "Run";
    public string idleBoolName = "Idle";

    [Header("Stop Distance")]
    public float stopDistanceFromObject = 3f; // Monster stopt X meter voor het startObject

    [Header("Attack Settings")]
    public float attackDistance = 1.5f;
    public int damage = 100;

    [Header("Chase Delay")]
    public float chaseDelay = 2f;

    [Header("Audio")]
    public AudioSource moveSound;        // 🔥 NIEUW: geluid bij bewegen
    public float moveSoundInterval = 5f; // 🔥 Afspelen om de 5 seconden

    private bool canMove = false;
    private bool isIdle = true;
    private bool hasAttacked = false;
    private float timer = 0f;

    private BoxCollider stopCollider;

    // 🔥 NIEUW: timer voor geluid
    private float moveSoundTimer = 0f;

    void Start()
    {
        if (stopObject != null)
            stopCollider = stopObject.GetComponent<BoxCollider>();

        if (animator != null)
        {
            animator.SetBool(idleBoolName, true);
            animator.SetBool(runBoolName, false);
        }
    }

    void Update()
    {
        // Timer voor chase delay
        if (!canMove)
        {
            timer += Time.deltaTime;

            if (timer >= chaseDelay)
            {
                canMove = true;
                isIdle = false;

                if (animator != null)
                {
                    animator.SetBool(runBoolName, true);
                    animator.SetBool(idleBoolName, false);
                }
            }
        }

        // Monster beweegt pas als canMove true is
        if (canMove && player != null && !isIdle)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;

            // Check of monster 3 meter voor stopObject moet stoppen
            if (stopObject != null && stopCollider != null)
            {
                // Gebruik bounds van collider
                Vector3 closestPointOnBounds = stopCollider.bounds.ClosestPoint(transform.position);
                float distanceToStopEdge = Vector3.Distance(transform.position, closestPointOnBounds);

                if (distanceToStopEdge <= stopDistanceFromObject)
                {
                    canMove = false;
                    isIdle = true;

                    if (animator != null)
                    {
                        animator.SetBool(runBoolName, false);
                        animator.SetBool(idleBoolName, true);
                    }
                    return; // Stop verdere beweging
                }
            }

            // Beweeg monster
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);

            // 🔥 NIEUW: speel geluid tijdens bewegen om de 5 seconden
            if (moveSound != null)
            {
                moveSoundTimer += Time.deltaTime;
                if (moveSoundTimer >= moveSoundInterval)
                {
                    moveSound.Play();
                    moveSoundTimer = 0f;
                }
            }

            // Check aanval
            if (!hasAttacked)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackDistance)
                {
                    AttackPlayer();
                }
            }
        }
        else
        {
            // Reset geluid timer wanneer monster stil staat
            moveSoundTimer = moveSoundInterval;
        }
    }

    private void AttackPlayer()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("Monster heeft de speler geraakt!");
        }

        hasAttacked = true;
        canMove = false;
        isIdle = true;

        if (animator != null)
        {
            animator.SetBool(runBoolName, false);
            animator.SetBool(idleBoolName, true);
        }
    }
}