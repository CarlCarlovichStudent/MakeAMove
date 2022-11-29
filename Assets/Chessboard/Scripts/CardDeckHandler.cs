using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardDeckHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int handSize;
    [SerializeField] private Vector2Int cardOffset;

    [Header("General references")]
    [SerializeField] private CardDeck deck;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Sprite cardSprite;

    [Header("Piece Sprites")]
    [SerializeField] private Sprite pawn;

    [Header("Card Type Sprites")]
    [SerializeField] private Sprite summon;
    [SerializeField] private Sprite move;
    [SerializeField] private Sprite special;

    private List<CardBehavior> cardPool;
    private CardBehavior[] hand;

    private void Awake()
    {
        cardPool = deck.GetCards();
        hand = new CardBehavior[handSize];

        InitializeHand();
    }

    // Initialize cards
    private void InitializeHand()
    {
        for (int i = 0; i < handSize; i++)
        {
            hand[i] = InitializeCard(i);
        }
    }

    private CardBehavior InitializeCard(int slot)
    {
        CardBehavior behavior = GetRandomCardBehavior();

        GameObject gameObject = InstantiateImageObject("Card " + slot, canvas.transform, cardSprite);
        Card card = gameObject.AddComponent<Card>();
        
        InstantiateImageObject("Piece Image", gameObject.transform, PieceSprite(behavior.piecesAffected));
        InstantiateImageObject("Card Type Image", gameObject.transform, CardTypeSprite(behavior.cardType));
        
        card.SetStartValues(slot, handSize, cardOffset);

        return behavior;
    }

    // Instantiate objects
    private GameObject InstantiateImageObject(string name, Transform parent, Sprite sprite)
    {
        GameObject gameObject = InstantiateGameObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;

        return gameObject;
    }
    
    private GameObject InstantiateGameObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        Transform transform = gameObject.AddComponent<RectTransform>();
        transform.transform.SetParent(parent.transform);
        transform.localScale = Vector3.one;

        return gameObject;
    }

    // Randomizer
    private CardBehavior GetRandomCardBehavior()
    {
        List<CardBehavior> behaviors = new List<CardBehavior>();
        foreach (CardBehavior behavior in cardPool)
        {
            for (int i = 0; i < behavior.weightedChance; i++)
            {
                behaviors.Add(behavior);
            }
        }

        return behaviors[Random.Range(0, behaviors.Count - 1)];
    }
    
    // Look up textures
    private Sprite PieceSprite(ChessPieceType pieceType)
    {
        switch (pieceType)
        {
            case ChessPieceType.Pawn:
                return pawn;
            
            default:
                throw new Exception("Invalid Piece Type");
        }
    }
    
    private Sprite CardTypeSprite(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Move:
                return move;
            
            case CardType.Summon:
                return summon;
            
            case CardType.Special:
                return special;
            
            default:
                throw new Exception("Invalid Card Type");
        }
    }
}
