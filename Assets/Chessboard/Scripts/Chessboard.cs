using UnityEngine;
using UnityEngine.Serialization;

public class Chessboard : MonoBehaviour
{
    [Header("Tile Settings")] 
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 1.0f;
    
    // Logic
    private const int TileCountX = 8, TileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    
    private void Awake()
    {
        GenerateAllTiles(tileSize);
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }
        
        HoverTileHandler();
    }

    // Hover
    private void HoverTileHandler()
    {
        Vector2Int hoveredTileIndex = GetHoveredTileIndex();
        if (hoveredTileIndex != currentHover)
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().enabled = false;
            }

            if (hoveredTileIndex != -Vector2Int.one)
            {
                tiles[hoveredTileIndex.x, hoveredTileIndex.y].GetComponent<MeshRenderer>().enabled = true;
            }

            currentHover = hoveredTileIndex;
        }
    }
    
    private Vector2Int GetHoveredTileIndex()
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
    private void GenerateAllTiles(float tileSize)
    {
        tiles = new GameObject[TileCountX, TileCountY];
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(int x, int y)
    {
        GameObject tile = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tile.transform.parent = transform;

        Mesh mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>().material = hoverMaterial;
        
        Vector3[] vertices = new Vector3[4];

        Vector3 position = transform.position;
        
        vertices[0] = new Vector3((x - TileCountX / 2f) * tileSize + position.x, yOffset, (y - TileCountY / 2f) * tileSize + position.y);
        vertices[1] = new Vector3((x - TileCountX / 2f) * tileSize + position.x, yOffset, (y + 1 - TileCountY / 2f) * tileSize + position.y);
        vertices[2] = new Vector3((x + 1 - TileCountX / 2f) * tileSize + position.x, yOffset, (y - TileCountY / 2f) * tileSize + position.y);
        vertices[3] = new Vector3((x + 1 - TileCountX / 2f) * tileSize + position.x, yOffset, (y + 1 - TileCountY / 2f) * tileSize + position.y);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        
        tile.AddComponent<BoxCollider>();
        tile.layer = LayerMask.NameToLayer("Tile");

        tile.GetComponent<MeshRenderer>().enabled = false;

        return tile;
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
