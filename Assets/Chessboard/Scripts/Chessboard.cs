using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Tile Settings")] 
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    
    // Logic
    private const int TileCountX = 8, TileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    
    private void Awake()
    {
        GenerateAllTiles(tileSize, TileCountX, TileCountY);
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

    // Generate tiles
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float size, int x, int y)
    {
        GameObject tile = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tile.transform.parent = transform;

        Mesh mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>().material = tileMaterial;
        
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize);
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        
        tile.AddComponent<BoxCollider>();
        tile.layer = LayerMask.NameToLayer("Tile");

        tile.GetComponent<MeshRenderer>().enabled = false;

        return tile;
    }
}
