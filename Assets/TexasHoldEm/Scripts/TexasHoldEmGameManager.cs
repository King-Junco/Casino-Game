using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TexasHoldEmGameManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerHandCenter;
    public Transform communityCenter;
    public TextMeshProUGUI handRankText;

    [Header("Card Prefabs")]
    public List<CardPrefabEntry> cardPrefabs;
    
    [Header("Settings")]
    public float handCardOffset = 0.2f;
    public float handCardRotation = 10f;
    public Vector3 cardScale = new Vector3(0.5f, 0.5f, 0.02f);
    public float communityCardOffset = 0.6f;
    public Vector3 communityCardScale = new Vector3(0.5f, 0.5f, 0.02f);
    public Quaternion faceDownRotation = Quaternion.Euler(90, 0, 0);
    public Quaternion faceUpRotation = Quaternion.Euler(90, 0, 0);

    private List<CardData> deck = new List<CardData>();
    private List<CardData> playerCards = new List<CardData>();
    private List<CardData> communityCards = new List<CardData>();
    private int communityCardsDealt = 0;

    [System.Serializable]
    public class CardPrefabEntry
    {
        public Suit suit;
        public Rank rank;
        public GameObject prefab;
    }
    void Start()
    {
        BuildDeck();
        ShuffleDeck();
        DealHoleCards();
    }

    void BuildDeck()
    {
        deck.Clear();

        foreach (Suit s in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank r in System.Enum.GetValues(typeof(Rank)))
            {
                deck.Add(new CardData
                {
                    suit = s,
                    rank = r,
                    cardObject = null
                });
            }
        }
    }


    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            CardData temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    void DealHoleCards()
    {
        int numCards = 2;
        for (int i = 0; i < numCards; i++)
        {
            float xOffset = (i - (numCards - 1) / 2f) * handCardOffset;
            float yRotation = i == 0 ? -handCardRotation : handCardRotation;
            Vector3 cardPos = playerHandCenter.position + new Vector3(xOffset, 0, 0);
            Quaternion cardRot = Quaternion.Euler(45, yRotation, 0);

            CardData cardData = deck[0];
            deck.RemoveAt(0);

            SpawnCardAt(cardData, cardPos, cardRot, cardScale);
            playerCards.Add(cardData);
        }
    }

    public void DealNextCommunityCard()
    {
        // Determine how many community cards have been dealt
        if (communityCardsDealt == 0)
        {
            // Flop: deal 3 cards
            for (int i = 0; i < 3; i++)
            {
                CardData card = DealCommunityCardAtIndex(i, faceDownRotation);
                communityCards.Add(card);
            }
            communityCardsDealt = 3;
        }
        else if (communityCardsDealt == 3)
        {
            // Turn: deal 1 card (4th card)
            CardData card = DealCommunityCardAtIndex(3, faceDownRotation);
            communityCards.Add(card);
            communityCardsDealt = 4;
        }
        else if (communityCardsDealt == 4)
        {
            // River: deal 1 card (5th card)
            CardData card = DealCommunityCardAtIndex(4, faceDownRotation);
            communityCards.Add(card);
            communityCardsDealt = 5;
        }

        // Reveal all community cards that have been dealt so far
        for (int i = 0; i < communityCards.Count; i++)
        {
            if (communityCards[i].cardObject != null)
                communityCards[i].cardObject.transform.rotation = faceUpRotation;
        }

        // Evaluate player's best hand using hole cards + community cards
        EvaluatePlayerHand();
    }


    CardData DealCommunityCardAtIndex(int index, Quaternion rot)
    {
        // Calculate position based on the community card index
        float xOffset = (index - 2) * communityCardOffset; // centers 5 cards
        Vector3 cardPos = communityCenter.position + new Vector3(xOffset, 0, 0);

        // Take the next card from the deck
        CardData cardData = deck[0];
        deck.RemoveAt(0);

        // Spawn the correct prefab at the position
        GameObject cardObj = SpawnCardAt(cardData, cardPos, rot, communityCardScale);

        // Return the card data for tracking/evaluation
        return cardData;
    }


    GameObject GetPrefabForCard(Suit suit, Rank rank)
    {
        var entry = cardPrefabs.Find(e => e.suit == suit && e.rank == rank);
        if (entry != null)
            return entry.prefab;

        Debug.LogError($"Prefab not found for {rank} of {suit}");
        return null;
    }

    GameObject SpawnCardAt(CardData cardData, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        GameObject prefab = GetPrefabForCard(cardData.suit, cardData.rank);
        if (prefab == null)
        {
            Debug.LogError("Missing prefab for " + cardData.rank + " of " + cardData.suit);
            return null;
        }

        GameObject cardObject = Instantiate(prefab, pos, rot);
        cardObject.transform.localScale = scale;

        
        cardData.cardObject = cardObject;
        return cardObject;
    }

    void EvaluatePlayerHand()
    {
        List<CardData> allCards = new List<CardData>();
        allCards.AddRange(playerCards);
        allCards.AddRange(communityCards);

        HandRank result = HandEvaluator.GetBestHand(allCards);

        if (handRankText != null)
            handRankText.text = "Your hand: " + result.ToString() + "!";
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DealNextCommunityCard();
        }
    }
}




