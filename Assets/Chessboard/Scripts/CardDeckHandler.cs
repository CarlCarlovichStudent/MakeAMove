using System.Collections.Generic;
using UnityEngine;

public class CardDeckHandler : MonoBehaviour
{
    [SerializeField] private CardDeck deck;

    private List<CardSetup> cards;
    private void Awake()
    {
        cards = deck.GetCards();
    }
}
