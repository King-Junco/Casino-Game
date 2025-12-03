using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum GameState { Betting, PlayerTurn, DealerTurn, GameOver }

public class BlackjackGameManager : MonoBehaviour
{
    public static BlackjackGameManager Instance;
    
    [Header("Game Components")]
    public Deck deck;
    public Hand playerHand;
    public Hand dealerHand;
    public BlackjackBettingSystem blackjackbettingSystem;
    
    [Header("UI Elements")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI dealerScoreText;
    public TextMeshProUGUI messageText;
    public Button hitButton;
    public Button standButton;
    public Button dealButton;
    public GameObject bettingPanel;
    public TMP_InputField betInputField;
    
    private GameState currentState;
    private GameObject dealerHiddenCard;
    private bool dealerCardHidden = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start()
    {
        deck.InitializeDeck();
        SetGameState(GameState.Betting);
    }
    
    public void StartGame()
    {
        int betAmount = 0;
        
        if (betInputField != null && int.TryParse(betInputField.text, out betAmount))
        {
            if (blackjackbettingSystem.PlaceBet(betAmount))
            {
                BeginRound();
            }
            else
            {
                messageText.text = "Invalid bet amount!";
            }
        }
    }
    
    private void BeginRound()
    {
        playerHand.ClearHand();
        dealerHand.ClearHand();
        dealerCardHidden = false;
        
        playerHand.AddCard(deck.DealCard(playerHand.cardParent, playerHand.cardStartPosition));
        dealerHand.AddCard(deck.DealCard(dealerHand.cardParent, dealerHand.cardStartPosition));
        playerHand.AddCard(deck.DealCard(playerHand.cardParent, playerHand.cardStartPosition));
        
        dealerHiddenCard = deck.DealCard(dealerHand.cardParent, dealerHand.cardStartPosition);
        dealerHand.AddCard(dealerHiddenCard);
        dealerCardHidden = true;
        
        if (dealerHiddenCard != null)
        {
            dealerHiddenCard.transform.rotation = Quaternion.Euler(270, 0, 0);
        }
        
        UpdateScores();
        
        if (playerHand.IsBlackjack())
        {
            RevealDealerCard();
            if (dealerHand.IsBlackjack())
            {
                EndRound("Push! Both have Blackjack!", true);
            }
            else
            {
                EndRound("Blackjack! You win!", false, 2.5f);
            }
        }
        else
        {
            SetGameState(GameState.PlayerTurn);
        }
    }
    
    public void Hit()
    {
        if (currentState != GameState.PlayerTurn) return;
        
        playerHand.AddCard(deck.DealCard(playerHand.cardParent, playerHand.cardStartPosition));
        UpdateScores();
        
        if (playerHand.IsBusted())
        {
            RevealDealerCard();
            EndRound("Busted! Dealer wins!", false);
        }
    }
    
    public void Stand()
    {
        if (currentState != GameState.PlayerTurn) return;
        
        RevealDealerCard();
        SetGameState(GameState.DealerTurn);
        DealerPlay();
    }
    
    private void DealerPlay()
    {
        UpdateScores();
        
        while (dealerHand.GetHandValue() < 17)
        {
            dealerHand.AddCard(deck.DealCard(dealerHand.cardParent, dealerHand.cardStartPosition));
            UpdateScores();
        }
        
        DetermineWinner();
    }
    
    private void DetermineWinner()
    {
        int playerValue = playerHand.GetHandValue();
        int dealerValue = dealerHand.GetHandValue();
        
        if (dealerHand.IsBusted())
        {
            EndRound("Dealer busted! You win!", false);
        }
        else if (playerValue > dealerValue)
        {
            EndRound("You win!", false);
        }
        else if (playerValue < dealerValue)
        {
            EndRound("Dealer wins!", false);
        }
        else
        {
            EndRound("Push! It's a tie!", true);
        }
    }
    
    private void EndRound(string message, bool isPush, float winMultiplier = 2f)
    {
        if (messageText != null)
            messageText.text = message;
    
        if (isPush)
        {
            blackjackbettingSystem.PushBet();
        }
        else if (message.Contains("win") || message.Contains("busted"))
        {
            if (message.Contains("Dealer"))
            {
                blackjackbettingSystem.LoseBet();
            }
            else
            {
                blackjackbettingSystem.WinBet(winMultiplier);
            }
        }
        else
        {
            blackjackbettingSystem.LoseBet();
        }
    
        SetGameState(GameState.GameOver);
    }
    
    private void RevealDealerCard()
    {
        if (dealerCardHidden && dealerHiddenCard != null)
        {
            dealerHiddenCard.transform.rotation = Quaternion.Euler(90, 0, 0); 
            dealerCardHidden = false;
            UpdateScores();
        }
    }
    
    private void UpdateScores()
    {
        playerScoreText.text = "Player: " + playerHand.GetHandValue();
        
        if (dealerCardHidden)
        {
            // Only show first card value
            Card firstCard = dealerHand.cards[0].GetComponent<Card>();
            int visibleValue = firstCard.GetValue();
            dealerScoreText.text = "Dealer: " + visibleValue + " + ?";
        }
        else
        {
            dealerScoreText.text = "Dealer: " + dealerHand.GetHandValue();
        }
    }
    
    private void SetGameState(GameState newState)
    {
        currentState = newState;
    
        if (hitButton != null)
            hitButton.interactable = (currentState == GameState.PlayerTurn);
    
        if (standButton != null)
            standButton.interactable = (currentState == GameState.PlayerTurn);
    
        if (dealButton != null)
            dealButton.interactable = (currentState == GameState.Betting || currentState == GameState.GameOver);
    
        if (bettingPanel != null)
        {
            bettingPanel.SetActive(currentState == GameState.Betting || currentState == GameState.GameOver);
        }
    
        if (messageText != null)
        {
            if (currentState == GameState.Betting)
            {
                messageText.text = "Place your bet!";
            }
            else if (currentState == GameState.PlayerTurn)
            {
                messageText.text = "";
            }
        }
    }
}