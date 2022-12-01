using Unity.VisualScripting;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Tile Settings")] 
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 1.0f;

    [Header("Team Materials")] 
    [SerializeField] private Material whiteTeamMaterial;
    [SerializeField] private Material blackTeamMaterial;
    
    [Header("Piece prefabs")]
    [SerializeField] private GameObject pawn;

    // Logic
    private const int TileCountX = 8, TileCountY = 8;
    
    private GameObject[,] tiles;
    private MeshRenderer[,] tileRenderers; // better for performance
    private Camera currentCamera;
    private Vector2Int currentHover;
    private ChessPiece[,] pieces;
    private CardBehavior selectedBehavior;
    private GameObject pieceContainer;
    private CardDeckHandler handler;
    private ChessPieceTeam team;
    
    private void Awake()
    {
        handler = GetComponent<CardDeckHandler>();
        pieceContainer = CreateContainer("Pieces", transform);
        pieces = new ChessPiece[TileCountX, TileCountY];
        team = ChessPieceTeam.White;
        
        GenerateAllTiles();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }
        
        TileHandler();
    }

    private void TileHandler() // To have correct order
    {
        HighlightTileHandler();
        if (HoverTileHandler())
        {
            SelectTileHandler();
        }
    }

    public void SetSelectedBehavior(CardBehavior behavior)
    {
        selectedBehavior = behavior;
        UnHighlightAll();
    }

    // Use card
    private void SelectTileHandler()
    {
        if (currentHover != -Vector2Int.one && Input.GetMouseButtonDown(0))
        {
            switch (selectedBehavior.cardType)
            {
                case CardType.Summon:
                    SpawnPiece(currentHover);
                    handler.UseCard();
                    break;
            }

            selectedBehavior = null;
        }
    }
    
    // Highlight
    private void HighlightTileHandler() // TODO: FIX
    {
        if (selectedBehavior != null)
        {
            switch (selectedBehavior.cardType)
            {
                case CardType.Summon:
                    HighlightSummon();
                    break;
                
                case CardType.Move:
                    HighlightMove();
                    break;
            }
        }
        else
        {
            UnHighlightAll();
        }
    }

    private void HighlightMove()
    {
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                if (pieces[x, y]?.type == selectedBehavior.piecesAffected)
                {
                    HighlightTile(x, y);
                }
            }
        }
    }

    private void HighlightSummon()
    {
        for (int x = 0; x < TileCountX; x++)
        {
            if (pieces[x, 0] is null)
            {
                HighlightTile(x, 0);
            }
        }
    }

    private void HighlightTile(int x, int y)
    {
        tileRenderers[x, y].material = highlightMaterial;
        tileRenderers[x, y].enabled = true;
    }

    private void UnHighlightAll()
    {
        foreach (MeshRenderer renderer in tileRenderers)
        {
            renderer.enabled = false;
        }
    }

    // Spawning pieces
    private void SpawnPiece(Vector2Int position)
    {
        ChessPiece piece = Instantiate(pawn).GetComponent<ChessPiece>();
        piece.transform.parent = pieceContainer.transform;

        piece.team = team;
        piece.GetComponent<MeshRenderer>().material = team == ChessPieceTeam.White ? whiteTeamMaterial : blackTeamMaterial;

        PositionPiece(ref piece, position, true);
        piece.boardPosition = position;

        pieces[position.x, position.y] = piece;
    }
    
    // Position pieces
    private void PositionPiece(ref ChessPiece piece, Vector2Int position, bool spawning = false)
    {
        if (spawning)
        {
            piece.transform.localPosition = GetTileCenter(position);
        }
    }

    private Vector3 GetTileCenter(Vector2Int position)
    {
        return new Vector3((0.5f + position.x - TileCountX / 2f) * tileSize  ,yOffset,
            (0.5f + position.y - TileCountY / 2f) * tileSize);
    }
    
    // Hover
    private bool HoverTileHandler()
    {
        bool highlighted = false;
        Vector2Int hoveredTileIndex = GetHoveredTileIndex();
        if (hoveredTileIndex != currentHover)
        {
            if (currentHover != -Vector2Int.one)
            {
                tileRenderers[currentHover.x, currentHover.y].enabled = false;
            }

            if (hoveredTileIndex != -Vector2Int.one)
            {
                if (tileRenderers[hoveredTileIndex.x, hoveredTileIndex.y].sharedMaterial == highlightMaterial)
                {
                    highlighted = true;
                }
                tileRenderers[hoveredTileIndex.x, hoveredTileIndex.y].enabled = true;
                tileRenderers[hoveredTileIndex.x, hoveredTileIndex.y].material = hoverMaterial;
            }

            currentHover = hoveredTileIndex;
        }
        else
        {
            if (hoveredTileIndex != -Vector2Int.one)
            {
                if (tileRenderers[hoveredTileIndex.x, hoveredTileIndex.y].sharedMaterial == highlightMaterial)
                {
                    highlighted = true;
                }
                tileRenderers[hoveredTileIndex.x, hoveredTileIndex.y].enabled = true;
                tileRenderers[hoveredTileIndex.x, hoveredTileIndex.y].material = hoverMaterial;
            }
        }

        return highlighted;
    }
    
    private Vector2Int GetHoveredTileIndex() // TODO: Improve by making bool and sending tile index as out variable
    {
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, LayerMask.GetMask("Tile")))
        {
            GameObject hitGameObject = hitInfo.transform.gameObject;
            for (int x = 0; x < TileCountX; x++)
            {
                for (int y = 0; y < TileCountY; y++)
                {
                    if (tiles[x, y] == hitGameObject)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
        }

        return -Vector2Int.one;
    }

    // Generate tiles
    private void GenerateAllTiles()
    {
        GameObject container = CreateContainer("Tiles", transform);

        tileRenderers = new MeshRenderer[TileCountX, TileCountY];
        tiles = new GameObject[TileCountX, TileCountY];
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                tiles[x, y] = GenerateTile(x, y, container);
            }
        }
    }

    private GameObject GenerateTile(int x, int y, GameObject container)
    {
        GameObject tile = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tile.transform.parent = container.transform;

        Mesh mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;

        MeshRenderer renderer = tile.AddComponent<MeshRenderer>();
        renderer.material = hoverMaterial;
        renderer.enabled = false;
        tileRenderers[x, y] = renderer;
        
        Vector3[] vertices = new Vector3[4];
        
        Vector3 position = transform.position;
        float centerOffsetX = TileCountX / 2f;
        float centerOffsetY = TileCountY / 2f;
        
        vertices[0] = new Vector3((x - centerOffsetX) * tileSize + position.x, yOffset, (y - centerOffsetY) * tileSize + position.y);
        vertices[1] = new Vector3((x - centerOffsetX) * tileSize + position.x, yOffset, (y + 1 - centerOffsetY) * tileSize + position.y);
        vertices[2] = new Vector3((x + 1 - centerOffsetX) * tileSize + position.x, yOffset, (y - centerOffsetY) * tileSize + position.y);
        vertices[3] = new Vector3((x + 1 - centerOffsetX) * tileSize + position.x, yOffset, (y + 1 - centerOffsetY) * tileSize + position.y);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        
        tile.AddComponent<BoxCollider>();
        tile.layer = LayerMask.NameToLayer("Tile");
        
        return tile;
    }
    
    // Reusable code
    private GameObject CreateContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name);
        container.transform.parent = parent;
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        
        return container;
    }

    // Draw preview of tiles in editor (shows outlines of tiles)
    #if UNITY_EDITOR
            /// <summary>
    		/// Draw the grid in the scene view
    		/// </summary>
    		void OnDrawGizmos()
            {
                Transform thisTransform = transform;
                
    			Color prevCol = Gizmos.color;
    			Gizmos.color = Color.cyan;
    
    			Matrix4x4 originalMatrix = Gizmos.matrix;
    			Gizmos.matrix = thisTransform.localToWorldMatrix;

                float size = tileSize / thisTransform.lossyScale.x;
    
    			// Draw local space flattened cubes
    			for (int y = 0; y < TileCountY; y++)
    			{
    				for (int x = 0; x < TileCountX; x++)
    				{
    					var position = new Vector3((x + 0.5f - TileCountX / 2f) * size, yOffset, (y + 0.5f - TileCountY / 2f) * size);
    					Gizmos.DrawWireCube(position, new Vector3(size, 0, size));
    				}
    			}
    
    			Gizmos.matrix = originalMatrix;
    			Gizmos.color = prevCol;
            }
    #endif
}
