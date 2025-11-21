using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour
{
    [Header("Race Setup")]
    [SerializeField] private List<Horse> horses = new List<Horse>();
    [SerializeField] private float finishLineZPosition = 50f;

    [Header("Race State")]
    private bool raceInProgress = false;
    private Horse winner;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button startRaceButton;

    [Header("Betting")]
    [SerializeField] private BettingSystem bettingSystem;

    void Start()
    {
        // Auto-find horses if list is empty
        if (horses.Count == 0)
        {
            Horse[] foundHorses = GetComponentsInChildren<Horse>();
            horses.AddRange(foundHorses);
            Debug.Log($"Auto-detected {horses.Count} horses");
        }

        // Auto-detect finish line by finding GameObject with tag "FinishLine"
        GameObject finishLineObj = GameObject.FindGameObjectWithTag("FinishLine");
        if (finishLineObj != null)
        {
            finishLineZPosition = finishLineObj.transform.position.z;
            Debug.Log($"Finish line auto-detected at Z: {finishLineZPosition}");
        }
        else
        {
            Debug.LogWarning("No GameObject with 'FinishLine' tag found. Using default finish line position.");
        }

        UpdateStatusText("Place your bets!");

        if (startRaceButton != null)
        {
            startRaceButton.onClick.AddListener(StartRace);
        }

        // Setup betting system with horses
        if (bettingSystem != null)
        {
            bettingSystem.SetupHorseButtons(horses);
        }
    }

    void Update()
    {
        if (raceInProgress)
        {
            CheckForWinner();
        }
    }

    public void StartRace()
    {
        if (raceInProgress) return;

        // Check if player has placed a bet
        if (bettingSystem != null && bettingSystem.GetCurrentBet() == null)
        {
            UpdateStatusText("Please place a bet first!");
            return;
        }

        raceInProgress = true;
        winner = null;

        if (startRaceButton != null)
        {
            startRaceButton.interactable = false;
        }

        // Lock betting
        if (bettingSystem != null)
        {
            bettingSystem.LockBetting();
        }

        UpdateStatusText("Race started! GO!");

        // Start all horses
        foreach (Horse horse in horses)
        {
            horse.StartRace();
        }
    }

    void CheckForWinner()
    {
        foreach (Horse horse in horses)
        {
            if (horse.transform.position.z >= finishLineZPosition && winner == null)
            {
                winner = horse;
                EndRace();
                break;
            }
        }
    }

    void EndRace()
    {
        raceInProgress = false;

        // Stop all horses
        foreach (Horse horse in horses)
        {
            horse.StopRacing();
        }

        UpdateStatusText($"{winner.GetHorseName()} wins!");

        // Process betting results
        if (bettingSystem != null)
        {
            bettingSystem.ProcessRaceResult(winner);
        }

        // Wait before allowing reset
        StartCoroutine(ResetRaceDelayed(3f));
    }

    IEnumerator ResetRaceDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetRace();
    }

    public void ResetRace()
    {
        // Reset all horses
        foreach (Horse horse in horses)
        {
            horse.ResetPosition();
        }

        winner = null;

        if (startRaceButton != null)
        {
            startRaceButton.interactable = true;
        }

        // Unlock betting
        if (bettingSystem != null)
        {
            bettingSystem.UnlockBetting();
        }

        UpdateStatusText("Place your bets!");
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Race Status: {message}");
    }

    public bool IsRaceInProgress()
    {
        return raceInProgress;
    }

    public List<Horse> GetHorses()
    {
        return horses;
    }
}