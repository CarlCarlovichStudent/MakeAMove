using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using Button = UnityEngine.UI.Button;

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

    [Header("Points and mana")] 
    [SerializeField] private int winPoints;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Rematch")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private Transform rematchIndicator;
    [SerializeField] private Button rematchButton;

    [Header("Remove this")] 
    [SerializeField] private TextMeshProUGUI whiteWinText;
    [SerializeField] private TextMeshProUGUI blackWinText;
    [SerializeField] private TextMeshProUGUI otherWantRematch;
    [SerializeField] private TextMeshProUGUI noToRematch;
    
    
    [Header("Audio")] 
    [SerializeField] private AudioPlay playList;
    [SerializeField] private AudioPlay entryStinger;
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private AudioPlay summonGame;
    [SerializeField] private AudioPlay summonKnight;
    [SerializeField] private AudioPlay score;
    [SerializeField] private AudioPlay victoryStinger;
    [SerializeField] private AudioPlay defeatStinger;
    [SerializeField] private bool fadeOut;
    [SerializeField] private AudioPlay killKnight;
    [SerializeField] private AudioPlay moveKnight;
    [SerializeField] private AudioPlay wrongMove;
    [SerializeField] private AudioPlay loosePoint;
    




    // Logic
    private const int TileCountX = 8, TileCountY = 8;

    private GameObject[,] tiles;
    private MeshRenderer[,] tileRenderers; // better for performance
    private Camera currentCamera;
    private Vector2Int currentHover;
    private ChessPiece[,] pieces;
    private ChessPiece currentlyDragging;
    private CardBehavior selectedBehavior;
    private GameObject pieceContainer;
    private CardDeckHandler handler;
    private ChessPieceTeam team;
    private int myPoints;
    private int enemyPoints;
    private bool myTurn; // Improve when mana and multiple cards per round is implemented (fully fix current logic)
    
    //MultiLogic
    private int playerCount = -1;
    private int currentTeam = -1;
    private bool localGame = true;
    private bool[] playerRematch = new bool[2];

    private void Awake()
    {
        handler = GetComponent<CardDeckHandler>();
        pieceContainer = CreateContainer("Pieces", transform);
        pieces = new ChessPiece[TileCountX, TileCountY];
        team = ChessPieceTeam.White;
        myTurn = false;
        
        GenerateAllTiles();

        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }

        if (myPoints >= winPoints)
        {
            
            GameUINet.Instance.OnRematchMenuTrigger();
            otherWantRematch.enabled = false;
            noToRematch.enabled = false;
            if (team == ChessPieceTeam.White)
            {
                whiteWinText.enabled = true;
                blackWinText.enabled = false;
               
            }
            else
            {
                whiteWinText.enabled = false;
                blackWinText.enabled = true;
            }
            myPoints = 0;
            enemyPoints = 0;
            playList.StopAudio(fadeOut);
            victoryStinger.PlayAudio();
        }

        if (enemyPoints >= winPoints)
        {
            GameUINet.Instance.OnRematchMenuTrigger();
            otherWantRematch.enabled = false;
            noToRematch.enabled = false;
            if (team != ChessPieceTeam.White)
            {
                whiteWinText.enabled = true;
                blackWinText.enabled = false;
            }
            else
            {
                whiteWinText.enabled = false;
                blackWinText.enabled = true;
            }
            myPoints = 0;
            enemyPoints = 0;
            playList.StopAudio(fadeOut);
            defeatStinger.PlayAudio();
            
        }
        
        scoreText.text = (myTurn ? "Your" : "Enemy") + $" turn\n\nYour points: {myPoints}/{winPoints}\nEnemy points: {enemyPoints}/{winPoints}";

        TileHandler();
    }

    private void TileHandler() // To have correct order
    {
        HighlightTileHandler();
        if (HoverTileHandler())
        {
            SelectTileHandler();
            DeselectPieceHandler(true);
        }
        else
        {
            DeselectPieceHandler(false);
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
                    SelectSummon();
                    break;
                
                case CardType.Move:
                    SelectMove();
                    break;
            }
        }
    }

    private void SelectMove()
    {
        currentlyDragging = pieces[currentHover.x, currentHover.y];
    }

    private void DeselectPieceHandler(bool wasHighlighted)
    {
        if (currentlyDragging is not null && Input.GetMouseButtonUp(0))
        {
            if (wasHighlighted && myTurn)
            {
                MoveTo(currentlyDragging.boardPosition, currentHover);
                moveKnight.PlayAudio();
            }
            else
            {
                PositionPiece(ref currentlyDragging, currentlyDragging.boardPosition);
                currentlyDragging = null;
                wrongMove.PlayAudio();
            }
        }
    }

    private void MoveTo(Vector2Int from, Vector2Int to)
    {
        pieces[to.x, to.y]?.DestroyPiece();
        pieces[to.x, to.y] = currentlyDragging;
        pieces[from.x, from.y] = null;
      
        
        
        PositionPiece(ref currentlyDragging, to);
        
        currentlyDragging = null;
        selectedBehavior = null;
        handler.UseCard();
        myTurn = false;
        

        if (team == ChessPieceTeam.White)
        {
            if (to.y == 7)
            {
                myPoints++;
                pieces[to.x, to.y].DestroyPiece();
                pieces[to.x, to.y] = null;
                score.PlayAudio();
            }
        }
        else
        {
            if (to.y == 0)
            {
                myPoints++;
                pieces[to.x, to.y].DestroyPiece();
                pieces[to.x, to.y] = null;
                score.PlayAudio();
            }
        }
        
        //Net Implementation
        NetMakeMove mm = new NetMakeMove();
        mm.originalX = from.x;
        mm.originalY = from.y;
        mm.destinationX = to.x;
        mm.destinationY = to.y;
        mm.teamId = currentTeam;
        Client.Instace.SendToServer(mm);
    }

    private void SelectSummon()
    {
        if (myTurn)
        {
            SpawnPiece(currentHover);
            selectedBehavior = null;
            handler.UseCard();
            
            myTurn = false;
        }
    }
    
    // Receive moves
    private void ReceiveMove(Vector2Int from, Vector2Int to)
    {
        pieces[to.x, to.y]?.DestroyPiece();
        pieces[to.x, to.y] = pieces[from.x, from.y];
        pieces[from.x, from.y] = null;
        
        
        PositionPiece(ref pieces[to.x, to.y], to);

        myTurn = true;
        
        if (team != ChessPieceTeam.White)
        {
            if (to.y == 7)
            {
                enemyPoints++;
                pieces[to.x, to.y].DestroyPiece();
                pieces[to.x, to.y] = null;
                loosePoint.PlayAudio();
            }
        }
        else
        {
            if (to.y == 0)
            {
                enemyPoints++;
                pieces[to.x, to.y].DestroyPiece();
                pieces[to.x, to.y] = null;
                loosePoint.PlayAudio();
            }
        }
    }
    
    // Highlight
    private void HighlightTileHandler()
    {
        UnHighlightAll();
        if (selectedBehavior != null)
        {
            if (currentlyDragging is null)
            {
                switch (selectedBehavior.cardType)
                {
                    case CardType.Summon:
                        HighlightSummon();
                        break;
                    
                    case CardType.Move:
                        HighlightMovablePieces();
                        break;
                }
            }
            else
            {
                HighlightValidMoves();
            }
        }
    }

    private void HighlightValidMoves()
    {
        foreach (MovementPattern movementPattern in selectedBehavior.movementPatterns)
        {
            Vector2Int move = currentlyDragging.boardPosition + (team == ChessPieceTeam.White ? movementPattern.move : movementPattern.move * Vector2Int.down);
            
            if (move.x < 0) move.x = 0;
            if (move.x > 7) move.x = 7;
            if (move.y < 0) move.y = 0;
            if (move.y > 7) move.y = 7;
            
            ChessPiece piece = pieces[move.x, move.y];
            if (piece is null)
            {
                if (movementPattern.moveType is MoveType.MoveOnly or MoveType.MoveAndCapture)
                {
                    HighlightTile(move.x, move.y);
                }
            }
            else
            {
                if (piece.team != team && movementPattern.moveType is MoveType.CaptureOnly or MoveType.MoveAndCapture)
                {
                    HighlightTile(move.x, move.y);
                }
            }
        }
    }

    private void HighlightMovablePieces()
    {
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                if (pieces[x, y]?.type == selectedBehavior.piecesAffected && pieces[x, y].team == team)
                {
                    HighlightTile(x, y);
                }
            }
        }
    }

    private void HighlightSummon()
    {
        int teamSide = team == ChessPieceTeam.White ? 0 : 7;
        for (int x = 0; x < TileCountX; x++)
        {
            if (pieces[x, teamSide] is null)
            {
                HighlightTile(x, teamSide);
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
        InstantiatePiece(position, ChessPieceType.Pawn, team);
        
        //Net Implementation
        NetSpawnPiece sp = new NetSpawnPiece();
        sp.spawnX = position.x;
        sp.spawnY = position.y;
        sp.teamId = currentTeam;
        Client.Instace.SendToServer(sp);
        summonKnight.PlayAudio();
    }

    private void ReceiveSpawnedPiece(Vector2Int position, int teamId) // teamId could be replaced with reversing own team
    {
        InstantiatePiece(position, ChessPieceType.Pawn, teamId == 0 ? ChessPieceTeam.White : ChessPieceTeam.Black);
        myTurn = true;
    }

    private void InstantiatePiece(Vector2Int position, ChessPieceType type, ChessPieceTeam team) // fix for all types
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
        piece.boardPosition = position;
        if (spawning)
        {
            piece.transform.localPosition = GetTileCenter(position) + Vector3.down * 1f;
            piece.SetDesiredPosition(GetTileCenter(position), 6);
        }
        else
        {
            piece.SetDesiredPosition(GetTileCenter(position));
        }
    }

    private Vector3 GetTileCenter(Vector2Int position)
    {
        return new Vector3((0.5f + position.x - TileCountX / 2f) * tileSize ,yOffset,
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
        Vector2Int vector = -Vector2Int.one;
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
                        vector = new Vector2Int(x, y);
                    }
                }
            }
        }

        if (currentlyDragging is not null)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetDesiredPosition(ray.GetPoint(distance));
            }
        }

        return vector;
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
    
    // For handler TODO: improve or remove (make in to property)
    public int GetPieceAmount()
    {
        int amount = 0;
        foreach (ChessPiece piece in pieces)
        {
            if (piece is not null)
            {
                amount++;
            }
        }

        return amount;
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

    #region Rematch

    public void OnRematchButton()
    {
        if (localGame)
        {
            NetRematch wrm = new NetRematch();
            wrm.teamId = 0;
            wrm.wantRematch = 1;
            Client.Instace.SendToServer(wrm);
            
            NetRematch brm = new NetRematch();
            brm.teamId = 1;
            brm.wantRematch = 1;
            Client.Instace.SendToServer(brm);
        }
        else
        {
            NetRematch rm = new NetRematch();
            rm.teamId = currentTeam;
            rm.wantRematch = 1;
            Client.Instace.SendToServer(rm);
        }
    }

    public void ResetGame() //Todo: Fix if a reset is going to be made
    {
        //UI
        rematchButton.interactable.Equals(true);
        
        rematchIndicator.transform.gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);
        
        //Field reset
        currentlyDragging = null;
        playerRematch[0] = playerRematch[1] = false;

        //clean up
        /*
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                //Remove all pieces
            }
        }
        
        //Set up all cards
        */
    }

    public void OnMenuButton()
    {
        NetRematch rm = new NetRematch();
        rm.teamId = currentTeam;
        rm.wantRematch = 0;
        Client.Instace.SendToServer(rm);
        
        //ResetGame();
        GameUINet.Instance.OnLeaveFromGameMenu();

        Invoke("ShutdownRelay", 1.0f);
        
        //resetValues
        playerCount = -1;
        currentTeam = -1;
    }

    #endregion

    #region EventCalling

    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_SPAWN_PIECE += OnSpawnPieceServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGame;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_SPAWN_PIECE += OnSpawnPieceClient;
        NetUtility.C_REMATCH += OnRematchClient;

        GameUINet.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_SPAWN_PIECE -= OnSpawnPieceServer;
        NetUtility.S_REMATCH -= OnRematchServer;
        
        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGame;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_SPAWN_PIECE -= OnSpawnPieceClient;
        NetUtility.C_REMATCH -= OnRematchClient;

        GameUINet.Instance.SetLocalGame -= OnSetLocalGame;
    }

    //Server
    private void OnWelcomeServer(Netmessage msg, NetworkConnection cnn)
    {
        //Client has connected and send back
        NetWelcome nw = msg as NetWelcome;
        
        //Assign team
        nw.AssignedTeam = ++playerCount;
        
        //Return message
        Server.Instace.SendToClient(cnn, nw);
        //If full (two players), start game
        if (playerCount == 1)
        {
            Server.Instace.Broadcast(new NetStartGame());
        }
    }
    
    private void OnMakeMoveServer(Netmessage msg, NetworkConnection cnn)
    {
        //Receive, and just broadcast it back
        NetMakeMove mm = msg as NetMakeMove;
        
        //This is where we could do validation checks
        
        // Send it back
        Server.Instace.Broadcast(msg);
        
    }
    
    private void OnSpawnPieceServer(Netmessage msg, NetworkConnection cnn)
    {
        //Receive, and just broadcast it back
        NetSpawnPiece sp = msg as NetSpawnPiece;
        
        //This is where we could do validation checks
        
        // Send it back
        Server.Instace.Broadcast(msg);
        
    }
    
    private void OnRematchServer(Netmessage msg, NetworkConnection cnn)
    {
        Server.Instace.Broadcast(msg);
    }
    
    //Client
    
    private void OnWelcomeClient(Netmessage msg)
    {
        //Client has connected and send back
        NetWelcome nw = msg as NetWelcome;
        
        //Assign team
        currentTeam = nw.AssignedTeam;
        
        Debug.Log($"My assigned team is {nw.AssignedTeam}");
        
        if (localGame && currentTeam == 0)
        {
            Server.Instace.Broadcast(new NetStartGame());
        }
    }

    private void OnStartGame(Netmessage msg)
    {
        
        Debug.Log("Game Begin");
        summonGame.PlayAudio();
        entryStinger.PlayAudio();
        playList.PlayAudio();
        menuMusic.Stop();
        
        
        
        GameUINet.Instance.ChangeCamera((currentTeam==0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
        
        team = currentTeam == 0 ? ChessPieceTeam.White : ChessPieceTeam.Black;
        if (team == ChessPieceTeam.White) myTurn = true;
    }
    
    private void OnMakeMoveClient(Netmessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;
        
        Debug.Log($"MM : {mm.teamId} : {mm.originalX}, {mm.originalY} -> {mm.destinationX}, {mm.destinationY}");

        if (mm.teamId != currentTeam)
        {
            ReceiveMove(new Vector2Int(mm.originalX, mm.originalY), new Vector2Int(mm.destinationX, mm.destinationY));
        }
        
    }
    
    private void OnSpawnPieceClient(Netmessage msg)
    {
        NetSpawnPiece sp = msg as NetSpawnPiece;
        
        Debug.Log($"SP : {sp.teamId} : {sp.spawnX}, {sp.spawnY}");
        
        if (sp.teamId != currentTeam)
        {
            ReceiveSpawnedPiece(new Vector2Int(sp.spawnX, sp.spawnY), sp.teamId);
        }
        
    }
    
    private void OnRematchClient(Netmessage msg)
    {
        //Receive the connection message
        NetRematch rm = msg as NetRematch;
        
        //Set the boolean for rematch
        playerRematch[rm.teamId] = rm.wantRematch == 1;
        
        //Activate the piece of UI
        if (rm.teamId != currentTeam)
        {
            //rematchIndicator.transform.GetChild((rm.wantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
            if (rm.wantRematch != 1)
            {
                //TODO: Seems not to be working properly and unsure of how to fix.
                rematchButton.interactable.Equals(false);
                //
                noToRematch.enabled = true;
                otherWantRematch.enabled = false;
            }
            else
            {
                otherWantRematch.enabled = true;
            }
        }

        //If both wants to rematch
        if (playerRematch[0] && playerRematch[1])
        {
            ResetGame();
            GameUINet.Instance.OnResetToGameMenu();
        }
    }

    //Other
    private void ShutdownRelay()
    {
        Client.Instace.ShutDown();
        Server.Instace.ShutDown();
    }
    
    private void OnSetLocalGame(bool obj)
    {
        playerCount = -1;
        currentTeam = -1;
        localGame = obj;
    }

    #endregion
}
