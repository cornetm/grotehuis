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

    [Header("Eye Contact Settings")]
    public float eyeContactAngle = 20f;
    public float eyeContactTimeThreshold = 2f;

    private Transform player;
    private Camera playerCamera;
    private FirstPersonCamera fpsCamera;

    private bool alertTriggered = false;
    private bool attackTriggered = false;

    private float eyeContactTimer = 0f;

    // ---------------- JUMPSCARE ADDITIONS (UNCHANGED) ----------------
    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Jumpscare Settings")]
    public float lingerTime = 2.5f;
    public float jumpscareScale = 0.5f;
    public float stopOffset = 2f;
    public float followSpeed = 20f;
    public float tiltAngle = 15f;
    public float tiltSpeed = 10f;

    [Header("Audio")]
    public AudioClip attackSound;
    private AudioSource audioSource;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private bool isAttackingNow = false;
    private bool jumpscareActive = false;

    private float tiltTimer = 0f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        ResolveCamera();

        controller = GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null || animator == null) return;

        if (fpsCamera == null)
            ResolveCamera();

        float distance = Vector3.Distance(transform.position, player.position);

        HandleChase(distance);
        HandleEyeContact(distance);

        if (attackTriggered && isAttackingNow && !jumpscareActive)
            MoveTowardsPlayer();
    }

    // ---------------- CHASE (UNCHANGED) ----------------
    void HandleChase(float distance)
    {
        if (!attackTriggered)
            LookAtPlayer();

        if (distance <= chaseRadius && !attackTriggered)
        {
            attackTriggered = true;
            animator.SetBool(attackBool, true);

            StartCoroutine(StartAttackWithDelay());
        }
    }

    // ---------------- EYE CONTACT (RESTORED OLD LOGIC ONLY) ----------------
    void HandleEyeContact(float distance)
    {
        if (fpsCamera == null) return;

        bool lookingAtSkull =
            distance <= alertRadius &&
            IsPlayerLookingAtSkull();

        // 🔥 OLD BEHAVIOR: ALWAYS UPDATE
        fpsCamera.SetEyeContact(lookingAtSkull);

        if (!alertTriggered)
        {
            if (lookingAtSkull)
            {
                eyeContactTimer += Time.deltaTime;

                if (eyeContactTimer >= eyeContactTimeThreshold)
                {
                    alertTriggered = true;

                    if (animator != null)
                        animator.SetTrigger(alertTrigger);
                }
            }
            else
            {
                eyeContactTimer = 0f;
            }
        }
    }

    // ---------------- CAMERA ----------------
    void ResolveCamera()
    {
        if (FirstPersonCamera.Instance != null)
        {
            fpsCamera = FirstPersonCamera.Instance;
            playerCamera = fpsCamera.GetComponent<Camera>();
            return;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        if (playerCamera != null)
            fpsCamera = playerCamera.GetComponent<FirstPersonCamera>();
    }

    // ---------------- LOOK ----------------
    bool IsPlayerLookingAtSkull()
    {
        if (playerCamera == null) return false;

        Vector3 toSkull =
            (transform.position - playerCamera.transform.position).normalized;

        float angle =
            Vector3.Angle(playerCamera.transform.forward, toSkull);

        return angle <= eyeContactAngle;
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            5f * Time.deltaTime
        );
    }

    // ---------------- ATTACK + JUMPSCARE (UNCHANGED) ----------------
    IEnumerator StartAttackWithDelay()
    {
        PlayAttackSoundFromTime(0.5f);
        yield return new WaitForSeconds(0.25f);
        isAttackingNow = true;
    }

    void MoveTowardsPlayer()
    {
        if (player == null || controller == null) return;

        Vector3 dir = player.position - transform.position;
        dir.Normalize();

        Vector3 horizontalVelocity = dir * moveSpeed;

        if (controller.isGrounded)
            verticalVelocity.y = -1f;
        else
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        controller.Move((horizontalVelocity + verticalVelocity) * Time.deltaTime);

        CheckHitPlayer();

        Quaternion lookRot =
            Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));

        transform.rotation =
            Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);
    }

    void CheckHitPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 1.5f)
            TryHitPlayer(player.gameObject);
    }

    void OnTriggerEnter(Collider other) => TryHitPlayer(other.gameObject);
    void OnCollisionEnter(Collision collision) => TryHitPlayer(collision.gameObject);

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
                Vector3 targetPos =
                    playerCamera.transform.position +
                    playerCamera.transform.forward * stopOffset;

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    Time.deltaTime * followSpeed
                );

                Vector3 lookDir =
                    playerCamera.transform.position - transform.position;

                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion baseRot = Quaternion.LookRotation(lookDir);

                    tiltTimer += Time.deltaTime * tiltSpeed;
                    float tiltZ = Mathf.Sin(tiltTimer) * tiltAngle;

                    transform.rotation = Quaternion.Euler(
                        baseRot.eulerAngles.x,
                        baseRot.eulerAngles.y,
                        tiltZ
                    );
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
}