using UnityEngine;
using TMPro;

public class BlackjackBettingSystem : MonoBehaviour
{
    public int playerMoney = 1000;
    public int currentBet = 0;
    public int minBet = 10;
    public int maxBet = 500;
    
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI betText;
    
    private void Start()
    {
        UpdateUI();
    }
    
    public bool PlaceBet(int amount)
    {
        if (amount < minBet || amount > maxBet)
        {
            Debug.Log("Bet must be between $" + minBet + " and $" + maxBet);
            return false;
        }
        
        if (amount > playerMoney)
        {
            Debug.Log("Not enough money!");
            return false;
        }
        
        currentBet = amount;
        playerMoney -= amount;
        UpdateUI();
        return true;
    }
    
    public void WinBet(float multiplier = 2f)
    {
        int winnings = Mathf.RoundToInt(currentBet * multiplier);
        playerMoney += winnings;
        currentBet = 0;
        UpdateUI();
    }
    
    public void LoseBet()
    {
        currentBet = 0;
        UpdateUI();
    }
    
    public void PushBet()
    {
        playerMoney += currentBet;
        currentBet = 0;
        UpdateUI();
    }
    
    public void AddToBet10()
    {
        AddToBetInput(10);
    }
    
    public void AddToBet50()
    {
        AddToBetInput(50);
    }
    
    public void AddToBet100()
    {
        AddToBetInput(100);
    }
    
    private void AddToBetInput(int amount)
    {
        TMP_InputField betInputField = FindObjectOfType<TMP_InputField>();
        if (betInputField != null)
        {
            int currentValue = 0;
            int.TryParse(betInputField.text, out currentValue);
            betInputField.text = (currentValue + amount).ToString();
        }
    }
    
    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "Money: $" + playerMoney.ToString();
        else
            Debug.LogWarning("MoneyText is not assigned!");
        
        if (betText != null)
            betText.text = "Current Bet: $" + currentBet.ToString();
        else
            Debug.LogWarning("BetText is not assigned!");
    }
}