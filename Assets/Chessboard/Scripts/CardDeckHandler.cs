using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardDeckHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHandSize;
    [SerializeField] private Vector2Int cardSize;
    [SerializeField] private Vector3Int cardOffset; // z value is hover offset
    
    [Header("General references")]
    [SerializeField] private CardDeck deck;
    [SerializeField] private Canvas canvas;
    [SerializeField] private AudioPlay audioHoverCard;
    [SerializeField] private AudioPlay audioSelectCard;
    
    private const float UseCardTime = 0.5f;
    private const float RespawnOffset = 0.3f;
    
    private Chessboard board;
    private float[] respawnTimers;
    private int starting;
    
    // Cards
    private List<CardBehavior> cardPool;
    private Card[] hand;
    private Card lastSelected;
    private Card lastHovered;

    private int handSize;
    
    //Tutorial
    private int tutorialStepForCards = -1;
    private int[] tutorialCards = new[] { 0, 3, 2, 2, 1, 3, 1, 3 };

    private void Awake()
    {
        cardPool = deck.GetCards();
        board = GetComponent<Chessboard>();
        handSize = maxHandSize;
        InitializeHand();
    }

    private void Update()
    {
        HandleDeath();
    }

    private void HandleDeath()
    {
        for (int i = 0; i < handSize; i++)
        {
                if (respawnTimers[i] <= 0) continue;
                else
                {
                    respawnTimers[i] -= Time.deltaTime;
                    if (respawnTimers[i] <= 0)
                    {
                        if (!board.PuzzleActive)
                        {
                            hand[i] = InitializeCard(i);
                        }
                    }
                } 
        }
        
    }

    public void ResetHand(int size = 0)
    {
        foreach (Card card in hand)
        { 
            card.Use(0.1f);
        }

        if (board.TutorialGameStep != 10)
        {
            board.PuzzleActive = false;
        }
        
        handSize = size <= 0 ? maxHandSize : size;

        InitializeHand();
    }

    public int GetCurrentAmountCardsHeld()
    {
        int totalCards = 0;
        foreach (Card c in hand)
        {
            if (c != null)
            {
                totalCards++;
            }
        }
        return totalCards;
    }

    public void UseCard()
    {
        board.MyMana -= lastSelected.ManaCost;
        lastSelected.Use(UseCardTime);
            for (int i = 0; i < hand.Length; i++)
            {
                if (lastSelected == hand[i])
                {
                    respawnTimers[i] = RespawnOffset;
                }
            }
    }

    public bool ValidCardPlay()
    {
        return lastSelected.ManaCost <= board.MyMana;
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

        if (board.TutorialGameStep == 2)
        {
            GameUINet.Instance.OnSpawnPieceTutorial();
        }

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
        respawnTimers = new float[handSize];
        
        starting = 0;
        for (int i = 0; i < handSize; i++)
        {
            hand[i] = InitializeCard(i);
            starting++;
        }
        starting = 0;
    }

    private Card InitializeCard(int slot)
    {
        CardBehavior behavior = new CardBehavior();
        if (board.TutorialGameStep == 0 || board.TutorialGameStep >= 11)
        {
            tutorialStepForCards = -1;
        }

        if (board.TutorialGame && board.TutorialGameStep<10)
        {
            behavior = GetTutorialCardBehavior();
        }
        else
        {
            behavior = GetRandomCardBehavior();
        }


        GameObject gameObject = InstantiateImageObject("Card " + slot, canvas.transform, behavior.sprite, cardSize);
        Card card = gameObject.AddComponent<Card>();

        //InstantiateImageObject("Piece Image", gameObject.transform, PieceSprite(behavior.piecesAffected));
        //InstantiateImageObject("Card Type Image", gameObject.transform, CardTypeSprite(behavior.cardType));
        
        card.SetStartValues(slot + 0.5f - handSize / 2f, cardOffset, behavior, audioHoverCard, audioSelectCard);

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
    
    private CardBehavior GetTutorialCardBehavior()
    {
        //Define all cards
        List<CardBehavior> behaviors = deck.GetCards();
        tutorialStepForCards++;
        
        //Check tutorial cards array for the order of the cards
        return behaviors[tutorialCards[tutorialStepForCards]];
    }
    
    // Randomizer
    private CardBehavior GetRandomCardBehavior() // TODO: Improve
    {
        int spawnAmount = starting + board.GetPieceAmount();
        
        foreach (Card card in hand)
        { 
            if (card?.behavior.cardType == CardType.Summon && !card.forcedMovement) spawnAmount++;
        }

        List<CardBehavior> behaviors = new List<CardBehavior>();
        if (spawnAmount < 1)
        {
            foreach (CardBehavior behavior in cardPool)
            {
                if (behavior.cardType == CardType.Summon)
                {
                    for (int i = 0; i < behavior.weightedChance; i++)
                    {
                        behaviors.Add(behavior);
                    }
                }
            }

            return behaviors[Random.Range(0, behaviors.Count)];
        }

        if (spawnAmount > 5)
        {
            foreach (CardBehavior behavior in cardPool)
            {
                if (behavior.cardType == CardType.Summon)
                {
                    for (int i = 0; i < behavior.weightedChance - (spawnAmount - 5) * 3; i++)
                    {
                        behaviors.Add(behavior);
                    }
                }
                else
                {
                    for (int i = 0; i < behavior.weightedChance; i++)
                    {
                        behaviors.Add(behavior);
                    }
                }
            }
            
            return behaviors[Random.Range(0, behaviors.Count)];
        }

        foreach (CardBehavior behavior in cardPool)
        {
            for (int i = 0; i < behavior.weightedChance; i++)
            {
                behaviors.Add(behavior);
            }
        }
        return behaviors[Random.Range(0, behaviors.Count)];
    }
}
