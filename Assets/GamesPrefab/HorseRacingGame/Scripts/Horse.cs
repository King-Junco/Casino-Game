using Unity.VisualScripting;
using UnityEngine;

public class Horse : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int horseNumber;
    [SerializeField] private string horseName;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float paceMin = -2f;
    [SerializeField] private float paceMax = 2f;
    [SerializeField] private float burstMin = 0;
    [SerializeField] private float burstMax = 2f;

    [Header("Odds")]
    [SerializeField] private float odds = 2.0f; // Payout multiplier

    private float currentSpeed;
    private bool isRacing = false;
    private Vector3 startingPosition;
    private float pace;
    private float burstTimer = 0f;
    private float timeBetweenBursts = 1.0f;
    private Animator animator;

    void Start()
    {
        startingPosition = transform.position;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isRacing) return;
        
        // Move horse forward
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // update animation
        animator.SetFloat("Speed", currentSpeed);

        // Burst timing
        burstTimer += Time.deltaTime;

        if (burstTimer >= timeBetweenBursts) 
        {
            burstTimer = 0f;
            ApplyBurst();
        }

        // fade burst 
        currentSpeed = Mathf.Lerp(currentSpeed, pace, Time.deltaTime * 2f);
        //currentSpeed = Mathf.Max(0f, currentSpeed);
        
    }


    public void StartRace()
    {
        isRacing = true;
        
        // Roll steady pace
        pace = baseSpeed + Random.Range(paceMin, paceMax);

        //pace = Mathf.Max(0f, pace);

        // Total speed
        currentSpeed = pace; 

    }

    void ApplyBurst()
    {
        // Roll a burst that temporarily modifies the speed
        float burst = Random.Range(burstMin, burstMax);

        //Burst short-term effect
        currentSpeed = pace + burst;

        //currentSpeed = Mathf.Max(0f, currentSpeed);
    }

    public void ResetPosition()
    {
        isRacing = false;
        transform.position = startingPosition;
        currentSpeed = 0f;
    }

    public bool IsRacing()
    {
        return isRacing;
    }

    public void StopRacing()
    {
        isRacing = false;
    }

    // Getters for private fields
    public int GetHorseNumber() => horseNumber;
    public string GetHorseName() => horseName;
    public float GetOdds() => odds;
    public float GetPaceMin() => paceMin;
    public float GetPaceMax() => paceMax;
    public float GetBurstMin => burstMin;
    public float GetBurstMax() => burstMax;
    public float GetBaseSpeed() => baseSpeed;

}