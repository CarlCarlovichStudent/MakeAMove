using UnityEngine;

public enum CardType
{
    None = 0,
    Move = 1,
    Summon = 2,
    Special = 3
}

public class Card : MonoBehaviour
{
    public ChessPieceType piecesAffected;
}
