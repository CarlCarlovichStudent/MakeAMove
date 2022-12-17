//using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent OnDestroyEvents;
    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * speed); // maybe fix linear speed option
    }

    public void DestroyPiece()
    {
        OnDestroyEvents.Invoke();
        Destroy(gameObject, 2f);
    }

    public void SetDesiredPosition(Vector3 position, int speed = 9)
    {
        this.speed = speed;
        desiredPosition = position + Vector3.up * 0.44f;
    }
}
