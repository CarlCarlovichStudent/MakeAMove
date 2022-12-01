using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Deck")]
public class CardDeck : ScriptableObject
{
    [SerializeField] [NonReorderable] private List<CardBehavior> cards = new List<CardBehavior>();

    public List<CardBehavior> GetCards()
    {
        return cards;
    }
}

[Serializable]
public class CardBehavior
{
    public ChessPieceType piecesAffected;
    public CardType cardType;
    public Sprite sprite;
    [Range(1, 10)] public int weightedChance;
}

public enum CardType
{
    None = 0,
    Move = 1,
    Summon = 2,
    Special = 3
}
