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
    public float wallCheckDistance = 0.5f;

    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Jumpscare Settings")]
    [Tooltip("Hoelang de skull zichtbaar blijft nadat hij de speler raakt")]
    public float lingerTime = 2.5f;
    [Tooltip("Schaalfactor van de skull voor jumpscare effect")]
    public float jumpscareScale = 0.5f;
    [Tooltip("Afstand vóór de speler/camera waar de skull blijft hangen")]
    public float stopOffset = 2f;
    [Tooltip("Snelheid waarmee de skull de camera volgt")]
    public float followSpeed = 20f;
    [Tooltip("Amplitude van links/rechts kantelen (graden)")]
    public float tiltAngle = 15f;
    [Tooltip("Snelheid van kantelen")]
    public float tiltSpeed = 10f;

    [Header("Eye Contact Settings")]
    [Tooltip("Max hoek in graden voor oogcontact")]
    public float eyeContactAngle = 20f;
    [Tooltip("Tijd in seconden dat oogcontact moet duren om alert te triggeren")]
    public float eyeContactTimeThreshold = 2f;

    [Header("Audio")]
    public AudioClip attackSound;
    private AudioSource audioSource;

    private Transform player;
    private Camera playerCamera;
    private float playerHeight = 2f;

    private bool alertTriggered = false;
    private bool attackTriggered = false;
    private bool alertEnabled = true;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private bool isAttackingNow = false;
    private bool jumpscareActive = false;

    private float tiltTimer = 0f;
    private float eyeContactTimer = 0f;

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
            playerCamera = Camera.main;
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
        float distance = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = HasLineOfSight(eyePos, player.position);

        if (!attackTriggered)
            LookAtPlayer();

        // Attack starten (voorrang)
        if (distance <= chaseRadius && canSeePlayer && !attackTriggered)
        {
            attackTriggered = true;
            StopAllCoroutines();
            alertTriggered = false;
            alertEnabled = false;

            if (animator != null)
                animator.SetBool(attackBool, true);

            StartCoroutine(StartAttackWithDelay());
        }

        // ---------------- Eye Contact Alert ----------------
        if (alertEnabled && distance <= alertRadius && !alertTriggered)
        {
            if (IsPlayerLookingAtSkull())
            {
                eyeContactTimer += Time.deltaTime;
                if (eyeContactTimer >= eyeContactTimeThreshold)
                {
                    alertTriggered = true;
                    alertEnabled = false;
                    if (animator != null)
                        animator.SetTrigger(alertTrigger);
                }
            }
            else
            {
                eyeContactTimer = 0f;
            }
        }

        if (attackTriggered && isAttackingNow && !jumpscareActive)
            MoveTowardsPlayer();
    }

    IEnumerator StartAttackWithDelay()
    {
        PlayAttackSoundFromTime(0.5f);
        yield return new WaitForSeconds(0.25f);
        isAttackingNow = true;
    }

    void LookAtPlayer()
    {
        if (player == null) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        transform.rotation = targetRotation;
    }

    void MoveTowardsPlayer()
    {
        if (player == null || controller == null) return;

        Vector3 dir = player.position - transform.position;
        dir.Normalize();

        // Wall avoidance
        Vector3 rayOrigin = transform.position + Vector3.up * controller.height * 0.5f;
        if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, wallCheckDistance))
        {
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
            {
                dir += Vector3.up * 0.5f;
                dir.Normalize();
            }
        }

        Vector3 horizontalVelocity = dir * moveSpeed;
        if (controller.isGrounded)
            verticalVelocity.y = -1f;
        else
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        controller.Move((horizontalVelocity + verticalVelocity) * Time.deltaTime);

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

        isAttackingNow = false;
        jumpscareActive = true;
        transform.localScale = Vector3.one * jumpscareScale;

        StartCoroutine(JumpscareFollowCamera());
        StartCoroutine(StayBeforeDestroy(lingerTime));
    }

    IEnumerator JumpscareFollowCamera()
    {
        tiltTimer = 0f;

        while (jumpscareActive)
        {
            if (playerCamera != null)
            {
                Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * stopOffset;
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

                Vector3 lookDir = playerCamera.transform.position - transform.position;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion baseRot = Quaternion.LookRotation(lookDir);

                    tiltTimer += Time.deltaTime * tiltSpeed;
                    float tiltZ = Mathf.Sin(tiltTimer) * tiltAngle;
                    transform.rotation = Quaternion.Euler(baseRot.eulerAngles.x, baseRot.eulerAngles.y, tiltZ);
                }
            }
            yield return null;
        }
    }

    IEnumerator StayBeforeDestroy(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        jumpscareActive = false;
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

    bool IsPlayerLookingAtSkull()
    {
        if (playerCamera == null) return false;

        Vector3 toSkull = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, toSkull);

        return angle <= eyeContactAngle;
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