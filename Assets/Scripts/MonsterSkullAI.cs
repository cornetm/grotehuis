using UnityEngine;
using System.Collections;

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
    public string attackBool = "IsAttacking";

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

    private bool isAttackingNow = false;

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

        // Attack starten (nieuwe volgorde)
        if (distance <= chaseRadius && canSeePlayer && !attackTriggered)
        {
            attackTriggered = true;

            // 1. Attack animatie direct AAN
            if (animator != null)
                animator.SetBool(attackBool, true);

            // 2. Alert UIT
            alertEnabled = false;

            // 3. Start coroutine voor sound + delay + movement
            StartCoroutine(StartAttackWithDelay());
        }

        // Alert trigger 1x (alleen als attack nog niet bezig is)
        if (alertEnabled && distance <= alertRadius && canSeePlayer && !alertTriggered)
        {
            alertTriggered = true;
            alertEnabled = false;

            if (animator != null)
                animator.SetTrigger(alertTrigger);
        }

        // 4. Beweeg pas NA delay
        if (attackTriggered && isAttackingNow)
            MoveTowardsPlayer();
    }

    IEnumerator StartAttackWithDelay()
    {
        // 3. Speel geluid
        PlayAttackSoundFromTime(0.5f);

        // Wacht 0.25 sec
        yield return new WaitForSeconds(0.25f);

        // 4. Movement starten
        isAttackingNow = true;
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
        dir.Normalize();

        Vector3 horizontalVelocity = new Vector3(dir.x, 0, dir.z) * moveSpeed;
        if (controller.isGrounded)
            verticalVelocity.y = -1f;
        else
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        Vector3 totalVelocity = horizontalVelocity + verticalVelocity;
        controller.Move(totalVelocity * Time.deltaTime);

        CheckHitPlayer();

        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
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

        RaycastHit[] hits = Physics.RaycastAll(from, dir, distance);
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
                return true;
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                return false;
        }

        return true;
    }

    void PlayAttackSoundFromTime(float startTime)
    {
        if (attackSound == null) return;

        GameObject soundObj = new GameObject("SkullAttackSound");
        soundObj.transform.position = transform.position;

        AudioSource src = soundObj.AddComponent<AudioSource>();
        src.clip = attackSound;
        src.spatialBlend = 1f;
        src.time = startTime;
        src.Play();

        Destroy(soundObj, attackSound.length - startTime);
    }

    void OnDrawGizmos()
    {
        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(eyePos, eyePos + transform.forward * alertRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePos, eyePos + transform.forward * chaseRadius);
    }
}