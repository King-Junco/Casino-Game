using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PachinkoMachine : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private BoxCollider spawnZone; // The box collider at the top
    [SerializeField] private float spawnHeight = 0.5f; // Height above spawn zone to drop from

    [Header("Machine Settings")]
    [SerializeField] private float baseBetAmount = 10f;

    [Header("References")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballPoolContainer;
    [SerializeField] private List<ScoringZone> scoringZones = new List<ScoringZone>();

    [Header("UI References")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text betAmountText;
    [SerializeField] private Button dropBallButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private Button decreaseBetButton;

    [Header("Ball Pool")]
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> ballPool = new Queue<GameObject>();
    private float currentBetAmount;
    private float playerBalance = 1000f; // Connect this to your main balance system

    void Start()
    {
        InitializeBallPool();
        currentBetAmount = baseBetAmount;

        // Setup UI listeners
        if (dropBallButton) dropBallButton.onClick.AddListener(DropBall);
        if (increaseBetButton) increaseBetButton.onClick.AddListener(IncreaseBet);
        if (decreaseBetButton) decreaseBetButton.onClick.AddListener(DecreaseBet);

        // Register all scoring zones with this machine
        foreach (var zone in scoringZones)
        {
            zone.RegisterMachine(this);
        }

        UpdateUI();
    }

    void InitializeBallPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab, ballPoolContainer);
            ball.SetActive(false);
            ballPool.Enqueue(ball);
        }
    }

    GameObject GetPooledBall()
    {
        if (ballPool.Count > 0)
        {
            GameObject ball = ballPool.Dequeue();
            ball.SetActive(true);
            return ball;
        }
        else
        {
            GameObject ball = Instantiate(ballPrefab, ballPoolContainer);
            return ball;
        }
    }

    public void ReturnBallToPool(GameObject ball)
    {
        ball.SetActive(false);
        ball.transform.position = ballPoolContainer.position;
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        ballPool.Enqueue(ball);
    }

    public void DropBall()
    {
        if (playerBalance < currentBetAmount)
        {
            Debug.Log("Insufficient balance!");
            return;
        }

        // Deduct bet
        playerBalance -= currentBetAmount;
        UpdateUI();

        // Get ball from pool
        GameObject ball = GetPooledBall();

        // Get random position within spawn zone bounds
        Vector3 randomPos = GetRandomPositionInSpawnZone();
        ball.transform.position = randomPos;

        // Reset physics
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Setup ball component
        PachinkoBall ballScript = ball.GetComponent<PachinkoBall>();
        if (ballScript)
        {
            ballScript.Initialize(this, currentBetAmount);
        }
    }

    Vector3 GetRandomPositionInSpawnZone()
    {
        if (spawnZone == null)
        {
            Debug.LogWarning("Spawn zone not assigned!");
            return transform.position;
        }

        // Get bounds of the spawn zone
        Bounds bounds = spawnZone.bounds;

        // Random position within X and Z, above the spawn zone
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float spawnY = bounds.max.y + spawnHeight;

        return new Vector3(randomX, spawnY, randomZ);
    }

    public void OnBallScored(float multiplier, float betAmount)
    {
        float winAmount = betAmount * multiplier;
        playerBalance += winAmount;
        UpdateUI();

        Debug.Log($"Ball landed! Multiplier: {multiplier}x, Won: ${winAmount:F2}");
    }

    void IncreaseBet()
    {
        currentBetAmount += baseBetAmount;
        currentBetAmount = Mathf.Min(currentBetAmount, playerBalance);
        UpdateUI();
    }

    void DecreaseBet()
    {
        currentBetAmount -= baseBetAmount;
        currentBetAmount = Mathf.Max(currentBetAmount, baseBetAmount);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (balanceText) balanceText.text = $"Balance: ${playerBalance:F2}";
        if (betAmountText) betAmountText.text = $"Bet: ${currentBetAmount:F2}";
    }

    // Public methods to connect your game's balance system
    public void SetPlayerBalance(float balance)
    {
        playerBalance = balance;
        UpdateUI();
    }

    public float GetPlayerBalance()
    {
        return playerBalance;
    }
}