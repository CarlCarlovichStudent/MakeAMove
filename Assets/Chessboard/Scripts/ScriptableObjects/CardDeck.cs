using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Deck")]
public class CardDeck : ScriptableObject
{
    [SerializeField] [NonReorderable] private List<CardSetup> cards;

    public List<CardSetup> GetCards()
    {
        return cards;
    }
}

[Serializable]
public class CardSetup
{
    public ChessPieceType piecesAffected;
    public CardType cardType;
    public int weightedChance;
}
