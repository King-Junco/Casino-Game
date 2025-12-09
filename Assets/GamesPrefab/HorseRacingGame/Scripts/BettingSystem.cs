using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BettingSystem : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float playerMoney = 1000f;
    private float currentBetAmount = 0f;
    private Horse bettedHorse = null;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI betAmountText;
    [SerializeField] private TMP_InputField betInputField;
    [SerializeField] private Button placeBetButton;

    [Header("Horse Selection")]
    [SerializeField] private TMP_Dropdown horseDropdown;
    [SerializeField] private List<Horse> horses = new List<Horse>();

    [Header("Result UI")]
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Horse Stats")]
    [SerializeField] private TextMeshProUGUI OddsText;
    [SerializeField] private TextMeshProUGUI HorseSpeedText;
    [SerializeField] private TextMeshProUGUI HorsePaceText;
    [SerializeField] private TextMeshProUGUI HorseBurstText;


    private bool bettingLocked = false;

    void Start()
    {
        UpdateMoneyDisplay();
        UpdateBetDisplay();

        if (resultText != null)
        {
            resultText.text = "";  
        }

        // Connect place bet button
        if (placeBetButton != null)
        {
            placeBetButton.onClick.AddListener(PlaceBet);
        }

        // Setup dropdown
        SetupHorseDropdown();

        // Listen for dropdown changes
        if (horseDropdown != null)
        {
            horseDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    void SetupHorseDropdown()
    {
        if (horseDropdown == null) return;

        // Clear existing options
        horseDropdown.ClearOptions();

        // Create option list
        List<string> options = new List<string>();

        foreach (Horse horse in horses)
        {
            if (horse != null)
            {
                options.Add($"{horse.GetHorseName()}");
            }
        }

        // Add to dropdown
        horseDropdown.AddOptions(options);

        // Select first by default
        if (horses.Count > 0)
        {
            horseDropdown.value = 0;
            bettedHorse = horses[0];
        }
    }

    void OnDropdownChanged(int index)
    {
        if (bettingLocked) return;

        if (index >= 0 && index < horses.Count)
        {
            bettedHorse = horses[index];

            UpdateHorseStats(bettedHorse);
            /*
            if (resultText != null)
            {
                resultText.text = $"Selected: {bettedHorse.GetHorseName()}";
            }
            */
        }
    }

    public void PlaceBet()
    {
        if (bettingLocked) return;

        if (bettedHorse == null)
        {
            ShowResult("Please select a horse first!");
            return;
        }

        float betAmount = 0f;
        if (betInputField != null && float.TryParse(betInputField.text, out betAmount))
        {
            if (betAmount <= 0)
            {
                ShowResult("Bet amount must be positive!");
                return;
            }

            if (betAmount > playerMoney)
            {
                ShowResult("Insufficient funds!");
                return;
            }

            currentBetAmount = betAmount;
            playerMoney -= betAmount;

            UpdateMoneyDisplay();
            UpdateBetDisplay();
            ShowResult($"Bet ${betAmount:F2} on {bettedHorse.GetHorseName()}");
        }
        else
        {
            ShowResult("Invalid bet amount!");
        }
    }

    public void ProcessRaceResult(Horse winner)
    {
        if (currentBetAmount == 0) return;

        if (bettedHorse == winner)
        {
            float winnings = currentBetAmount * winner.GetOdds();
            playerMoney += winnings;
            ShowResult($"YOU WIN! +${winnings:F2}");
        }
        else
        {
            ShowResult($"You Lost! -${currentBetAmount:F2}");
        }

        currentBetAmount = 0;
        bettedHorse = null;

        UpdateMoneyDisplay();
        UpdateBetDisplay();
    }

    public void LockBetting()
    {
        bettingLocked = true;

        if (horseDropdown != null)
        {
            horseDropdown.interactable = false;
        }

        if (placeBetButton != null)
        {
            placeBetButton.interactable = false;
        }
    }

    public void UnlockBetting()
    {
        bettingLocked = false;

        if (horseDropdown != null)
        {
            horseDropdown.interactable = true;
        }

        if (placeBetButton != null)
        {
            placeBetButton.interactable = true;
        }

        if (resultText != null)
        {
            resultText.text = "";
        }
    }

    void UpdateHorseStats(Horse horse)
    {
        if (horse == null) return;

        OddsText.text = $"Odds: {horse.GetOdds()}";
        HorseSpeedText.text = $"Speed: {horse.GetBaseSpeed()}";
        HorsePaceText.text = $"Pace: {horse.GetPaceMin()} - {horse.GetPaceMax()}";
        HorseBurstText.text = $"Burst: {horse.GetBurstMin} - {horse.GetBurstMax()}";
    }

    void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${playerMoney:F2}";
        }
    }

    void UpdateBetDisplay()
    {
        if (betAmountText != null)
        {
            betAmountText.text = $"Current Bet: ${currentBetAmount:F2}";
        }
    }

    public void HideResult() 
    {
        if (resultText != null) 
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }
    }

    public void ShowResult(string message)
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = message;
        }
    }

    public Horse GetCurrentBet()
    {
        return currentBetAmount > 0 ? bettedHorse : null;
    }
}