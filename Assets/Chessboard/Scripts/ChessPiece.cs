using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1
}

public enum ChessPieceTeam
{
    White = 0,
    Black = 1
}

public abstract class ChessPiece : MonoBehaviour
{
    public ChessPieceTeam team;
    public Vector2Int boardPosition;

    public abstract ChessPieceType type { get; }

    private Vector3 desiredPosition;
    private Vector3 desiredScale;
}
