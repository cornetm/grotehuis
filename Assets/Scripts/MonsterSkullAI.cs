using UnityEngine;

[ExecuteAlways]
public class MonsterSkullAI : MonoBehaviour
{
    [Header("Detection Ranges")]
    public float alertRadius = 5f;
    public float chaseRadius = 10f;
    public float viewHeightOffset = 1.5f;

    [Header("Animator")]
    public Animator animator;
    public string alertTrigger = "AlertTrigger";
    public string attackTrigger = "AttackTrigger";

    [Header("Chase Settings")]
    public float moveSpeed = 3.5f;

    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Audio")]
    public AudioClip attackSound;
    private AudioSource audioSource;

    private Transform player;
    private float playerHeight = 2f;

    private bool alertTriggered = false;
    private bool attackTriggered = false;
    private bool alertEnabled = true;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        controller = GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Collider col = player.GetComponent<Collider>();
            if (col != null)
                playerHeight = col.bounds.size.y;
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (player == null || animator == null) return;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;
        Vector3 horizontalDir = player.position - eyePos;
        horizontalDir.y = 0;
        float distance = horizontalDir.magnitude;

        bool canSeePlayer = HasLineOfSight(eyePos, player.position);

        // Altijd naar player kijken als niet attack
        if (!attackTriggered)
            LookAtPlayer();

        // Attack trigger
        if (distance <= chaseRadius && canSeePlayer && !attackTriggered)
        {
            animator.speed = 1f;
            animator.SetTrigger(attackTrigger);
            attackTriggered = true;
            alertEnabled = false;

            // Speel geluid vanaf 0.3s en begin meteen te bewegen
            PlayAttackSoundFromTime(0.3f);
        }

        // Alert trigger
        if (alertEnabled && distance <= alertRadius && canSeePlayer && !alertTriggered)
        {
            animator.speed = 1f;
            animator.SetTrigger(alertTrigger);
            alertTriggered = true;
            alertEnabled = false;
        }

        // Freeze alert animatie
        if (alertTriggered && !attackTriggered)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Alert") && state.normalizedTime >= 1f)
                animator.speed = 0f;
        }

        // Beweeg continu als attack gestart is (niet afhankelijk van animatie)
        if (attackTriggered)
            MoveTowardsPlayer();
    }

    void LookAtPlayer()
    {
        if (player == null) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
    }

    void MoveTowardsPlayer()
    {
        if (player == null || controller == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        Vector3 horizontalVelocity = dir * moveSpeed;
        if (controller.isGrounded)
            verticalVelocity.y = -1f;
        else
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        Vector3 totalVelocity = horizontalVelocity + verticalVelocity;
        CollisionFlags flags = controller.Move(totalVelocity * Time.deltaTime);

        if ((flags & CollisionFlags.CollidedSides) != 0)
            CheckHitPlayer();

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) => TryHitPlayer(other.gameObject);
    private void OnCollisionEnter(Collision collision) => TryHitPlayer(collision.gameObject);

    void CheckHitPlayer()
    {
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 1.5f)
            TryHitPlayer(player.gameObject);
    }

    void TryHitPlayer(GameObject other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(damage);

        Destroy(gameObject);
    }

    bool HasLineOfSight(Vector3 from, Vector3 playerPos)
    {
        Vector3 dir = (playerPos - from).normalized;
        float distance = Vector3.Distance(from, playerPos);

        if (Physics.Raycast(from, dir, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Player"))
                return true;
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                return false;
        }

        return true;
    }

    // Speel geluid vanaf een bepaald tijdstip
    void PlayAttackSoundFromTime(float startTime)
    {
        if (attackSound == null) return;

        GameObject soundObj = new GameObject("SkullAttackSound");
        soundObj.transform.position = transform.position;

        AudioSource src = soundObj.AddComponent<AudioSource>();
        src.clip = attackSound;
        src.spatialBlend = 1f;
        src.time = startTime;      // start vanaf 0.3s
        src.Play();

        Destroy(soundObj, attackSound.length - startTime);
    }

    void OnDrawGizmos()
    {
        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;

        // 🟡 Gele ray (alert)
        Gizmos.color = Color.yellow;
        Vector3 yellowEnd = eyePos + transform.forward * alertRadius;
        if (Physics.Raycast(eyePos, transform.forward, out RaycastHit hitAlert, alertRadius))
        {
            if (hitAlert.collider.isTrigger)
                Gizmos.DrawLine(eyePos, yellowEnd);
            else
                Gizmos.DrawLine(eyePos, hitAlert.point);
        }
        else
            Gizmos.DrawLine(eyePos, yellowEnd);

        // 🔴 Rode ray (chase/attack)
        Gizmos.color = Color.red;
        Vector3 forwardDir = transform.forward;
        float attackLength = chaseRadius;
        if (Physics.Raycast(eyePos, forwardDir, out RaycastHit hitAttack, attackLength))
        {
            if (hitAttack.collider.isTrigger)
                Gizmos.DrawLine(eyePos, eyePos + forwardDir * attackLength);
            else
                Gizmos.DrawLine(eyePos, hitAttack.point);
        }
        else
            Gizmos.DrawLine(eyePos, eyePos + forwardDir * attackLength);
    }
}