using UnityEngine;

public enum Suit { Hearts, Diamonds, Clubs, Spades }
public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

public class Card : MonoBehaviour
{
    public Suit suit;
    public Rank rank;
    public GameObject cardModel;
    
    public int GetValue()
    {
        if (rank >= Rank.Jack && rank <= Rank.King)
            return 10;
        return (int)rank;
    }
    
    public bool IsAce()
    {
        return rank == Rank.Ace;
    }
}