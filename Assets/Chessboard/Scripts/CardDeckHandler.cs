using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckHandler : MonoBehaviour
{
    [SerializeField] private CardDeck deck;
    [SerializeField] private Canvas canvas;
    [SerializeField] private int handSize;

    private List<CardBehavior> cardPool;
    private List<Card> hand;

    private void Awake()
    {
        cardPool = deck.GetCards();
    }
}
