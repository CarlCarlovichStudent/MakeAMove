using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardDeckHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int handSize;
    [SerializeField] private Vector2Int cardSize;
    [SerializeField] private Vector3Int cardOffset; // z value is hover offset
    
    [Header("General references")]
    [SerializeField] private CardDeck deck;
    [SerializeField] private Canvas canvas;

    private Chessboard board;
    
    // Cards
    private List<CardBehavior> cardPool;
    private Card[] hand;
    private Card lastSelected;
    private Card lastHovered;

    private void Awake()
    {
        cardPool = deck.GetCards();
        board = GetComponent<Chessboard>();
        InitializeHand();
    }

    private void Update()
    {
        if (lastSelected is not null)
        {
            //?
        }
    }

    public void UseCard()
    {
        lastSelected.Use();
    }

    // Hover and select handlers for HUD Raycaster
    public void HandleCardHover(Card card)
    {
        if (card == lastHovered || card == lastSelected) return;
        
        lastHovered?.Unhover();
        lastHovered = card;
        card.Hover();
    }

    public void HandleCardSelect(Card card) // TODO: handle deselect when already selected
    {
        if (card == lastSelected) return;
        
        lastSelected?.Deselect();
        lastSelected = card;
        lastSelected.Select();
        
        board.SetSelectedBehavior(lastSelected.behavior);
    }

    public void HandleNoCardHover()
    {
        lastHovered?.Unhover();
        lastHovered = null;
    }

    // Initialize cards
    private void InitializeHand()
    {
        hand = new Card[handSize];
        for (int i = 0; i < handSize; i++)
        {
            hand[i] = InitializeCard(i);
        }
    }

    private Card InitializeCard(int slot)
    {
        CardBehavior behavior = GetRandomCardBehavior();
        
        GameObject gameObject = InstantiateImageObject("Card " + slot, canvas.transform, behavior.sprite, cardSize);
        Card card = gameObject.AddComponent<Card>();

        //InstantiateImageObject("Piece Image", gameObject.transform, PieceSprite(behavior.piecesAffected));
        //InstantiateImageObject("Card Type Image", gameObject.transform, CardTypeSprite(behavior.cardType));
        
        card.SetStartValues(slot + 0.5f - handSize / 2f, cardOffset, behavior);

        return card;
    }

    // Instantiate objects
    private GameObject InstantiateImageObject(string name, Transform parent, Sprite sprite, Vector2 size)
    {
        GameObject gameObject = InstantiateGameObject(name, parent);
        
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;

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
}
