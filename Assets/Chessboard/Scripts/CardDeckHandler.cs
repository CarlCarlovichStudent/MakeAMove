using System;
using System.Collections.Generic;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardDeckHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int handSize;
    
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

    // Initialize
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
        
        Transform holderTransform = InstantiateGameObject("Card " + (slot + 1), canvas.transform).transform;
        
        GameObject pieceImage = InstantiateGameObject("Piece Image", holderTransform);
        Image image = pieceImage.AddComponent<Image>();
        image.sprite = PieceSprite(behavior.piecesAffected);
        
        GameObject cardTypeImage = InstantiateGameObject("Card Type Image", holderTransform);
        image = cardTypeImage.AddComponent<Image>();
        image.sprite = CardTypeSprite(behavior.cardType);

        return behavior;
    }

    // Instantiate objects
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
