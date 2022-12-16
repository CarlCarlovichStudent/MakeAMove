using UnityEngine;
using UnityEngine.Rendering;

public class Tile : MonoBehaviour
{
    public ChessPiece piece;
    
    public bool hovered;
    public bool highlighted;
    public bool selected;

    public Vector2Int Position => new Vector2Int(x, y);
    
    private MeshRenderer renderer;
    private Material validHoverMaterial, invalidHoverMaterial, highlightMaterial, selectedMaterial;

    private int x, y;

    private void Update()
    {
        renderer.enabled = true;
        if (selected)
        {
            renderer.material = selectedMaterial;
        }
        else if (hovered && highlighted)
        {
            renderer.material = validHoverMaterial;
        }
        else if (hovered)
        {
            renderer.material = invalidHoverMaterial;
        }
        else if (highlighted)
        {
            renderer.material = highlightMaterial;
        }
        else
        {
            renderer.enabled = false;
        }
    }

    public ChessPiece Select()
    {
        selected = true;
        
        return piece;
    }

    public Tile CreateTile(int x, int y, GameObject container, Chessboard creator)
    {
        this.x = x;
        this.y = y;
        
        validHoverMaterial = creator.ValidHoverMaterial;
        invalidHoverMaterial = creator.InvalidHoverMaterial;
        highlightMaterial = creator.HighlightMaterial;
        selectedMaterial = creator.SelectedMaterial;
        
        transform.parent = container.transform;

        Mesh mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;

        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = highlightMaterial;
        
        Vector3[] vertices = new Vector3[4];
        
        Vector3 position = transform.position;
        float centerOffsetX = creator.BoardSize.x / 2f;
        float centerOffsetY = creator.BoardSize.y / 2f;
        
        vertices[0] = new Vector3((x - centerOffsetX) * creator.TileSize + position.x, creator.YOffset, (y - centerOffsetY) * creator.TileSize + position.y);
        vertices[1] = new Vector3((x - centerOffsetX) * creator.TileSize + position.x, creator.YOffset, (y + 1 - centerOffsetY) * creator.TileSize + position.y);
        vertices[2] = new Vector3((x + 1 - centerOffsetX) * creator.TileSize + position.x, creator.YOffset, (y - centerOffsetY) * creator.TileSize + position.y);
        vertices[3] = new Vector3((x + 1 - centerOffsetX) * creator.TileSize + position.x, creator.YOffset, (y + 1 - centerOffsetY) * creator.TileSize + position.y);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        
        gameObject.AddComponent<BoxCollider>();
        gameObject.layer = LayerMask.NameToLayer("Tile");

        //gameObject.GetComponent<MeshRenderer>().shadowCastingMode == ShadowCastingMode.Off;

        return this;
    }
}
