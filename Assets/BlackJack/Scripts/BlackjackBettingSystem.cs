using UnityEngine;
using TMPro;

public class BlackjackBettingSystem : MonoBehaviour
{
    public int playerChips = 1000;
    public int currentBet = 0;
    public int minBet = 10;
    public int maxBet = 500;
    
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI betText;
    
    private void Start()
    {
        UpdateUI();
    }
    
    public bool PlaceBet(int amount)
    {
        if (amount < minBet || amount > maxBet)
        {
            Debug.Log("Bet must be between " + minBet + " and " + maxBet);
            return false;
        }
        
        if (amount > playerChips)
        {
            Debug.Log("Not enough chips!");
            return false;
        }
        
        currentBet = amount;
        playerChips -= amount;
        UpdateUI();
        return true;
    }
    
    public void WinBet(float multiplier = 2f)
    {
        int winnings = Mathf.RoundToInt(currentBet * multiplier);
        playerChips += winnings;
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
        playerChips += currentBet;
        currentBet = 0;
        UpdateUI();
    }
    
    public void IncreaseBet(int amount)
    {
        int newBet = currentBet + amount;
        if (newBet <= playerChips && newBet <= maxBet)
        {
            currentBet = newBet;
            UpdateUI();
        }
    }
    
    public void DecreaseBet(int amount)
    {
        int newBet = currentBet - amount;
        if (newBet >= minBet)
        {
            currentBet = newBet;
            UpdateUI();
        }
    }
    
    private void UpdateUI()
    {
        if (chipsText != null)
            chipsText.text = "Chips: $" + playerChips;
        
        if (betText != null)
            betText.text = "Bet: $" + currentBet;
    }
}