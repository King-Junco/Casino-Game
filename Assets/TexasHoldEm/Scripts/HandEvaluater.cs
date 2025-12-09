using System.Collections.Generic;
using UnityEngine;

// ------------------------- Card Definitions -------------------------
public enum Suit { Clubs, Diamonds, Hearts, Spades }

public enum Rank
{
    Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
    Jack, Queen, King, Ace
}

// CardData class to store suit, rank, and GameObject reference
[System.Serializable]
public class CardData
{
    public Suit suit;
    public Rank rank;
    public GameObject cardObject; // reference to instantiated prefab
}

// ------------------------- Hand Rankings -------------------------
public enum HandRank
{
    HighCard,
    OnePair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush
}

// ------------------------- Hand Evaluator -------------------------
public static class HandEvaluator
{
    public static HandRank GetBestHand(List<CardData> cards)
    {
        // 1. Sort cards descending by rank
        cards.Sort((a, b) => ((int)b.rank).CompareTo((int)a.rank));

        // 2. Check for flush
        var suitGroups = new Dictionary<Suit, List<CardData>>();
        foreach (var card in cards)
        {
            if (!suitGroups.ContainsKey(card.suit))
                suitGroups[card.suit] = new List<CardData>();
            suitGroups[card.suit].Add(card);
        }

        bool isFlush = false;
        List<CardData> flushCards = new List<CardData>();
        foreach (var group in suitGroups.Values)
        {
            if (group.Count >= 5)
            {
                isFlush = true;
                flushCards = group;
                break;
            }
        }

        // 3. Check for straight
        List<int> uniqueRanks = new List<int>();
        foreach (var card in cards)
        {
            if (!uniqueRanks.Contains((int)card.rank))
                uniqueRanks.Add((int)card.rank);
        }

        bool isStraight = false;
        int consecutive = 1;
        for (int i = 0; i < uniqueRanks.Count - 1; i++)
        {
            if (uniqueRanks[i] - 1 == uniqueRanks[i + 1])
            {
                consecutive++;
                if (consecutive >= 5)
                    isStraight = true;
            }
            else
            {
                consecutive = 1;
            }
        }

        // 4. Straight flush / royal flush
        if (isFlush && isStraight)
        {
            if (flushCards.Exists(c => c.rank == Rank.Ace) && consecutive >= 5)
                return HandRank.RoyalFlush;
            return HandRank.StraightFlush;
        }

        // 5. Group by rank
        var rankGroups = new Dictionary<Rank, int>();
        foreach (var card in cards)
        {
            if (!rankGroups.ContainsKey(card.rank))
                rankGroups[card.rank] = 0;
            rankGroups[card.rank]++;
        }

        if (rankGroups.ContainsValue(4)) return HandRank.FourOfAKind;
        if (rankGroups.ContainsValue(3) && rankGroups.ContainsValue(2)) return HandRank.FullHouse;
        if (isFlush) return HandRank.Flush;
        if (isStraight) return HandRank.Straight;
        if (rankGroups.ContainsValue(3)) return HandRank.ThreeOfAKind;

        int pairs = 0;
        foreach (var count in rankGroups.Values)
        {
            if (count == 2) pairs++;
        }

        if (pairs >= 2) return HandRank.TwoPair;
        if (pairs == 1) return HandRank.OnePair;

        return HandRank.HighCard;
    }
}

