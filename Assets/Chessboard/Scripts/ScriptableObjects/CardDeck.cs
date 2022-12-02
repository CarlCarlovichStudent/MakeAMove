using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
    [NonReorderable] public List<MovementPattern> movementPatterns;
}

[Serializable]
public class MovementPattern
{
    public Vector2Int move;
    public bool repeating;
    public bool symmetric; // Rotational symmetry 
}

public enum CardType
{
    None = 0,
    Move = 1,
    Summon = 2,
    Special = 3
}
