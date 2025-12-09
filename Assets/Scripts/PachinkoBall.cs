using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PachinkoBall : MonoBehaviour
{
    private PachinkoMachine machine;
    private float betAmount;
    private float lifetime = 15f; // Auto-return to pool after 15 seconds
    private float spawnTime;
    private bool hasScored = false;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioClip scoreSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource && (bounceSound || scoreSound))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        hasScored = false;
    }

    void Update()
    {
        // Auto-return to pool if ball is alive too long
        if (Time.time - spawnTime > lifetime && machine != null)
        {
            ReturnToPool();
        }

        // Also return if ball falls too far down
        if (transform.position.y < -10f && machine != null)
        {
            ReturnToPool();
        }
    }

    public void Initialize(PachinkoMachine parentMachine, float bet)
    {
        machine = parentMachine;
        betAmount = bet;
        spawnTime = Time.time;
        hasScored = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Play bounce sound on pin hits
        if (collision.gameObject.CompareTag("PachinkoPin") && audioSource && bounceSound)
        {
            audioSource.PlayOneShot(bounceSound, 0.3f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if entered a scoring zone
        ScoringZone zone = other.GetComponent<ScoringZone>();
        if (zone != null && !hasScored)
        {
            hasScored = true;

            // Play score sound
            if (audioSource && scoreSound)
            {
                audioSource.PlayOneShot(scoreSound);
            }

            // Notify the scoring zone
            zone.OnBallEntered(this, betAmount);

            // Return to pool after short delay
            Invoke(nameof(ReturnToPool), 1f);
        }
    }

    void ReturnToPool()
    {
        if (machine != null)
        {
            machine.ReturnBallToPool(gameObject);
        }
    }
}