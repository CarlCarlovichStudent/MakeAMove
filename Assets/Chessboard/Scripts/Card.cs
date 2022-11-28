using UnityEngine;

public class Card : MonoBehaviour
{
    private CardBehavior behavior;
    private Canvas canvas;

    public Card(CardBehavior behavior, Canvas canvas)
    {
        this.behavior = behavior;
        this.canvas = canvas;

        GameObject image = new GameObject(behavior.cardType.ToString() + behavior.piecesAffected.ToString());
        
        Instantiate(image);
    }
}
