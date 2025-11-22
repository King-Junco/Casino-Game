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

    void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        if (isRacing)
        {
            // Move horse forward
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        }
    }

    public void StartRace()
    {
        isRacing = true;
        
        // Roll steady pace
        float pace = baseSpeed + Random.Range(paceMin, paceMax);

        //Roll random burst
        float burst = Random.Range(burstMin, burstMax);

        // Total speed
        currentSpeed = pace + burst;

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