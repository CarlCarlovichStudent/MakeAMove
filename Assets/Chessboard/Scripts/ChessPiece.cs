using UnityEditor.UIElements;
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
    private int speed;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * speed); // maybe fix linear speed option
    }

    public void SetDesiredPosition(Vector3 position, int speed = 9)
    {
        this.speed = speed;
        desiredPosition = position;
    }
}
