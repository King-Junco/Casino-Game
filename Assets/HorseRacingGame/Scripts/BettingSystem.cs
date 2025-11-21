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
    [SerializeField] private Transform horseButtonContainer;
    [SerializeField] private GameObject horseButtonPrefab;
    [SerializeField] private Button placeBetButton;

    [Header("Result UI")]
    [SerializeField] private TextMeshProUGUI resultText;

    private bool bettingLocked = false;
    private List<Button> horseButtons = new List<Button>();

    void Start()
    {
        UpdateMoneyDisplay();
        UpdateBetDisplay();

        if (resultText != null)
        {
            resultText.text = "";
        }

        // Connect place bet button if assigned
        if (placeBetButton != null)
        {
            placeBetButton.onClick.AddListener(PlaceBet);
        }
    }

    public void SetupHorseButtons(List<Horse> horses)
    {
        // Clear existing buttons
        foreach (Button btn in horseButtons)
        {
            Destroy(btn.gameObject);
        }
        horseButtons.Clear();

        // Create button for each horse
        foreach (Horse horse in horses)
        {
            GameObject buttonObj;

            if (horseButtonPrefab != null)
            {
                buttonObj = Instantiate(horseButtonPrefab, horseButtonContainer);
            }
            else
            {
                // Create simple button if no prefab
                buttonObj = new GameObject($"Horse{horse.GetHorseNumber()}Button");
                buttonObj.transform.SetParent(horseButtonContainer);
                buttonObj.AddComponent<Image>();
                buttonObj.AddComponent<Button>();

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform);
                textObj.AddComponent<TextMeshProUGUI>();
            }

            Button btn = buttonObj.GetComponent<Button>();
            TextMeshProUGUI btnText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                btnText.text = $"{horse.GetHorseName()}\nOdds: {horse.GetOdds()}:1";
            }

            Horse horseRef = horse; // Capture for lambda
            btn.onClick.AddListener(() => SelectHorse(horseRef));

            horseButtons.Add(btn);
        }
    }

    public void SelectHorse(Horse horse)
    {
        if (bettingLocked) return;

        bettedHorse = horse;

        if (resultText != null)
        {
            resultText.text = $"Selected: {horse.GetHorseName()}";
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
            ShowResult($"Bet ${betAmount} on {bettedHorse.GetHorseName()}");
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
            // Player wins
            float winnings = currentBetAmount * winner.GetOdds();
            playerMoney += winnings;
            ShowResult($"YOU WIN! +${winnings}");
        }
        else
        {
            // Player loses
            ShowResult($"You Lost! -{currentBetAmount}");
        }

        currentBetAmount = 0;
        bettedHorse = null;

        UpdateMoneyDisplay();
        UpdateBetDisplay();
    }

    public void LockBetting()
    {
        bettingLocked = true;

        foreach (Button btn in horseButtons)
        {
            btn.interactable = false;
        }

        if (placeBetButton != null)
        {
            placeBetButton.interactable = false;
        }
    }

    public void UnlockBetting()
    {
        bettingLocked = false;

        foreach (Button btn in horseButtons)
        {
            btn.interactable = true;
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

    void ShowResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    public Horse GetCurrentBet()
    {
        return currentBetAmount > 0 ? bettedHorse : null;
    }
}