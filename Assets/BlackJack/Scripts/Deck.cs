using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public GameObject[] cardPrefabs; 
    private List<GameObject> deck = new List<GameObject>();
    private List<GameObject> discardPile = new List<GameObject>();
    
    public void InitializeDeck()
    {
        deck.Clear();
        discardPile.Clear();
        
        // Add all cards to deck
        foreach (GameObject cardPrefab in cardPrefabs)
        {
            deck.Add(cardPrefab);
        }
        
        Shuffle();
    }
    
    public void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            GameObject temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    
    public GameObject DealCard(Transform parent, Vector3 position)
    {
        if (deck.Count == 0)
        {
            ReshuffleDiscardPile();
        }
        
        if (deck.Count == 0)
        {
            Debug.LogError("No cards available!");
            return null;
        }
        
        GameObject cardPrefab = deck[0];
        deck.RemoveAt(0);
        
        GameObject cardInstance = Instantiate(cardPrefab, position, Quaternion.identity, parent);
        discardPile.Add(cardInstance);
        
        return cardInstance;
    }
    
    private void ReshuffleDiscardPile()
    {
        foreach (GameObject card in discardPile)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        
        discardPile.Clear();
        InitializeDeck();
    }
    
    public void ReturnCardToDiscard(GameObject card)
    {
        if (!discardPile.Contains(card))
        {
            discardPile.Add(card);
        }
    }
}