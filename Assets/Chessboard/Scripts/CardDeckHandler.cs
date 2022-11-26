using UnityEngine;

public class CardDeckHandler : MonoBehaviour
{
    [SerializeField] private CardDeck deck;

    private CardDeck.CardSetup[] cards;
    private void Awake()
    {
        cards = deck.GetCards().ToArray();
    }
}
