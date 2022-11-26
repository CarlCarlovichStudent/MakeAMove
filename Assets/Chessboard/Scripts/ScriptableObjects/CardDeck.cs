using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Deck")]
public class CardDeck : ScriptableObject
{
    [Serializable]
    public class CardSetup
    {
        public ChessPieceType piece;
    }

    [SerializeField] private List<CardSetup> cards;

    public List<CardSetup> GetCards()
    {
        return cards;
    }
}
