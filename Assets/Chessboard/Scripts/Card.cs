using UnityEngine;

public class Card : MonoBehaviour
{
    private CardBehavior behaviour;

    public Card(CardBehavior behavior) : base()
    {
        this.behaviour = behavior;
    }
}
