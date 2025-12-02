using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();
    public Transform cardParent;
    public Vector3 cardStartPosition;
    public Vector3 cardOffset;
    
    public void AddCard(GameObject card)
    {
        cards.Add(card);
        PositionCards();
    }
    
    public void ClearHand()
    {
        foreach (GameObject card in cards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        cards.Clear();
    }
    
    public int GetHandValue()
    {
        int value = 0;
        int aceCount = 0;
        
        foreach (GameObject cardObj in cards)
        {
            Card card = cardObj.GetComponent<Card>();
            if (card != null)
            {
                if (card.IsAce())
                {
                    aceCount++;
                    value += 11;
                }
                else
                {
                    value += card.GetValue();
                }
            }
        }
        
        while (value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }
        
        return value;
    }
    
    public bool IsBusted()
    {
        return GetHandValue() > 21;
    }
    
    public bool IsBlackjack()
    {
        return cards.Count == 2 && GetHandValue() == 21;
    }
    
    private void PositionCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.position = cardStartPosition + (cardOffset * i);
        }
    }
}