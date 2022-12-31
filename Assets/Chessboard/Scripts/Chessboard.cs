using System;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.VFX;
using Button = UnityEngine.UI.Button;

public class Chessboard : MonoBehaviour
{
    [Header("Tile Settings")] 
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material validHoverMaterial;
    [SerializeField] private Material invalidHoverMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 1.0f;

    [Header("Team Textures")]
    [SerializeField] private Material blackTeamMaterial;
    [SerializeField] private Material whiteTeamMaterial;

    [Header("Piece prefabs")]
    [SerializeField] private GameObject pawn;

    [Header("Points, mana and timmer")] 
    [SerializeField] private int winPoints;
    [SerializeField] private bool manaisOff;
    [SerializeField] private int manaStart;
    [SerializeField] private int manaGrowth;
    [SerializeField] private int startTimmer;

    [Header("TextManager")]
    [SerializeField] private TextManager textManager;
    
    [Header("Rematch")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private Transform rematchIndicator;
    [SerializeField] private Button rematchButton;

    [Header("Rematch Text")] 
    [SerializeField] private TextMeshProUGUI whiteWinText;
    [SerializeField] private TextMeshProUGUI blackWinText;
    [SerializeField] private TextMeshProUGUI otherWantRematch;
    [SerializeField] private TextMeshProUGUI noToRematch;

    [Header("Handlers")]
    [SerializeField] private AudioHandler audioHandler;
    [SerializeField] private CardDeckHandler cardDeckHandler;

    [Header("Trail")] 
    [SerializeField] private GameObject trailFollow;

    [Header("VFX")] 
    [SerializeField] private GameObject[] celebrationEffects;
    
    // Getters for Tile
    public Vector2Int BoardSize => new Vector2Int(TileCountX, TileCountY);
    public Material HighlightMaterial => highlightMaterial;
    public Material ValidHoverMaterial => validHoverMaterial;
    public Material InvalidHoverMaterial => invalidHoverMaterial;
    public Material SelectedMaterial => selectedMaterial;
    public float TileSize => tileSize;
    public float YOffset => yOffset;
    
    // Getters for Card Deck Handler
    public int TutorialGameStep => tutorialGameStep;
    public bool TutorialGame => tutorialGame;
    public AudioHandler AudioHandler => audioHandler;
    
    // Logic
    private const int TileCountX = 8, TileCountY = 8;

    private Tile[,] tiles;
    private Camera currentCamera;
    private Tile currentHover;
    private ChessPiece currentlyDragging;
    private CardBehavior selectedBehavior;
    private GameObject pieceContainer;
    private ChessPieceTeam team;
    private int myPoints;
    private int enemyPoints;
    private int myTimmer;
    private int enemyTimmer;
    public int MyMana { get; set;}
    private int currentMana;
    private float timeBank;
    private bool myTurn; // Improve when mana and multiple cards per round is implemented (fully fix current logic)
    
    //MultiLogic
    private int playerCount = -1;
    private int currentTeam = -1;
    private bool localGame = true;
    private bool[] playerRematch = new bool[2];
    
    //TutorialBase
    private bool tutorialGame;
    private int tutorialGameStep;
    private Vector2Int enemyPlayerSpawnTutorial;

    private bool puzzleSetUp;
    public bool PuzzleActive { get; set; }

    private void Awake()
    {
        cardDeckHandler = GetComponent<CardDeckHandler>();
        pieceContainer = CreateContainer("Pieces", transform);
        team = ChessPieceTeam.White;
        myTurn = false;
        textManager.SetText($"White Timer: {startTimmer}","Timer White", textCollection.GamePlay);
        textManager.SetText($"Black Timer: {startTimmer}","Timer Black", textCollection.GamePlay);
        myTimmer = startTimmer;
        enemyTimmer = startTimmer;
        if (!manaisOff)
        {
            currentMana = manaStart;
            MyMana = currentMana;
            textManager.SetText($"Mana: {MyMana}/{currentMana}","Mana Value",textCollection.GamePlay);
        }
        //textManager.ResetTexts(textCollection.GamePlay);
        textManager.ResetTexts(textCollection.Tutorial);

        GenerateAllTiles();

        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        WinConditionHandler();

        TileHandler();

        if (!GameUINet.Instance.GetGameOver())
        {
            TimerUpdate();

            if (!manaisOff)
            {
                ManaUpdate();
            }
        }
    }

    private void WinConditionHandler()
    {
        if (!tutorialGame || tutorialGameStep > 8)
        {
            if (localGame)
            {
                textManager.SetText((team == ChessPieceTeam.White ? "White's" : "Black's") 
                                    + $" turn\n\nWhite points: {myPoints}/{winPoints}\nBlack points: {enemyPoints}/{winPoints}", "Score Text", textCollection.GamePlay);
            }
            else
            {
                textManager.SetText((myTurn ? "Your" : "Opponent's") 
                                    + $" turn\n\nYour points: {myPoints}/{winPoints}\nEnemy points: {enemyPoints}/{winPoints}", "Score Text", textCollection.GamePlay);
            }
        }
        else
        {
            textManager.SetText("", "Score Text", textCollection.GamePlay);
        }

        if (tutorialGame)
        {
            if (myPoints >= winPoints)
            {
                GameUINet.Instance.OnWinTutorila();
            }
            return;
        }
        
        if (localGame)
        {
            if (myPoints >= winPoints || enemyPoints >= winPoints)
            {
                if (myPoints > enemyPoints)
                {
                    whiteWinText.enabled = true;
                    blackWinText.enabled = false;
                }
                else
                {
                    whiteWinText.enabled = false;
                    blackWinText.enabled = true;
                }
                
                GameUINet.Instance.OnRematchMenuTrigger();
                otherWantRematch.enabled = false;
                noToRematch.enabled = false;
                myPoints = 0;
                enemyPoints = 0;
                audioHandler.playList.StopAudio(audioHandler.fadeOut);
                audioHandler.ambLoop.StopAudio(audioHandler.fadeOut);
                audioHandler.victoryStinger.PlayAudio();
                audioHandler.exitMenuMusic.PlayAudio();
            }

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
            audioHandler.playList.StopAudio(audioHandler.fadeOut);
            audioHandler.ambLoop.StopAudio(audioHandler.fadeOut);
            audioHandler.victoryStinger.PlayAudio();
            audioHandler.exitMenuMusic.PlayAudio();
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
            audioHandler.playList.StopAudio(audioHandler.fadeOut);
            audioHandler.ambLoop.StopAudio(audioHandler.fadeOut);
            audioHandler.defeatStinger.PlayAudio();
            audioHandler.exitMenuMusic.PlayAudio();
        }
    }

    private void TileHandler() // To have correct order
    {
        HighlightTileHandler();
        HoverTileHandler();
        SelectTileHandler();
        DeselectPieceHandler();
    }

    public void SetSelectedBehavior(CardBehavior behavior)
    {
        selectedBehavior = behavior;
        UnHighlightAll();
    }

    // Use card
    private void SelectTileHandler()
    {
        if (currentHover is not null && currentHover.highlighted && Input.GetMouseButtonDown(0))
        {
            switch (selectedBehavior.cardType)
            {
                case CardType.Summon:
                    SelectSummon();
                    break;
                
                case CardType.Move:
                    SelectMove();
                    break;
                
                case CardType.Special:
                    SelectMove();
                    break;
            }
        }
    }

    private void SelectMove()
    {
        currentlyDragging = currentHover.Select();
        trailFollow.transform.SetParent(currentlyDragging.transform);
        trailFollow.transform.localScale = Vector3.one;
        trailFollow.transform.localPosition = new Vector3(0,-0.015f,0);
        trailFollow.transform.GetChild(1).localScale = Vector3.one/30;
        trailFollow.transform.GetChild(1).localPosition = Vector3.zero;
    }

    private void DeselectPieceHandler()
    {
        if (currentlyDragging is not null && Input.GetMouseButtonUp(0))
        {
            if (myTurn && currentHover is not null && currentHover.highlighted && (manaisOff || cardDeckHandler.ValidCardPlay()))
            {
                if (currentHover.piece is null) audioHandler.moveKnight.PlayAudio();
                else audioHandler.killKnight.PlayAudio(); 
                
                if (currentHover.piece is null) audioHandler.moveKnight.PlayAudio();
                else audioHandler.killKnightSwosh.PlayAudio(); 
                
                MoveTo(currentlyDragging.boardPosition, currentHover.Position);

                if (tutorialGameStep == 10 && PuzzleActive && myPoints>1)
                {
                    ResetGame();
                    GameUINet.Instance.OnFreePlayTutorial();
                    myPoints = 2;
                }
            }
            else
            {
                PositionPiece(ref currentlyDragging, currentlyDragging.boardPosition);
                currentlyDragging = null;
                audioHandler.wrongMove.PlayAudio();
            }
        }
    }

    private void MoveTo(Vector2Int from, Vector2Int to)
    {
        trailFollow.transform.SetParent(null);
        trailFollow.transform.localScale = Vector3.one;
        trailFollow.transform.position = new Vector3(0,0,0);
        
        tiles[to.x, to.y].piece?.DestroyPiece();
        tiles[to.x, to.y].piece = currentlyDragging;
        tiles[from.x, from.y].piece = null; 
        
        trailFollow.transform.GetChild(1).position = GetTileCenter(new Vector2Int(to.x,to.y)) + new Vector3(0,0.2f,0);
        trailFollow.transform.GetChild(1).localScale = Vector3.one;
        
        PositionPiece(ref currentlyDragging, to);
        
        currentlyDragging = null;
        selectedBehavior = null;
        cardDeckHandler.UseCard();
        
        if (team == ChessPieceTeam.White)
        {
            if (to.y == 7 && tiles[to.x, to.y].piece.team != ChessPieceTeam.Black)
            {
                myPoints++;
                tiles[to.x, to.y].piece.DestroyPiece();
                tiles[to.x, to.y].piece = null;
                audioHandler.score.PlayAudio();
                celebrationEffects[2].GetComponent<VisualEffect>().Play();
                celebrationEffects[3].GetComponent<VisualEffect>().Play();
            }
        }
        else
        {
            if (to.y == 0 && tiles[to.x, to.y].piece.team != ChessPieceTeam.White)
            {
                if (localGame)
                {
                    enemyPoints++;
                }
                else
                {
                    myPoints++;
                }
                tiles[to.x, to.y].piece.DestroyPiece();
                tiles[to.x, to.y].piece = null;
                audioHandler.score.PlayAudio();
                celebrationEffects[0].GetComponent<VisualEffect>().Play();
                celebrationEffects[1].GetComponent<VisualEffect>().Play();
            }
        }

        HandleTurn();

        if (tutorialGameStep == 5 || tutorialGameStep == 7)
        {
            GameUINet.Instance.OnOpponentTutorial();
        }

        if (tutorialGameStep == 9)
        {
            cardDeckHandler.ResetHand(4);
            GameUINet.Instance.OnPuzzleTutorial();
            Invoke("DelayedTutorialAction", 0.7f);
            myPoints = 1;
        }

        if (tutorialGame) return;
        
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
        if (myTurn && (manaisOff || cardDeckHandler.ValidCardPlay()))
        {
            SpawnPiece(currentHover.Position);
            selectedBehavior = null;
            cardDeckHandler.UseCard();
            
            
            if (tutorialGameStep == 3) // Important order
            {
                enemyPlayerSpawnTutorial = currentHover.Position;
                HandleTurn();
                GameUINet.Instance.OnOpponentTutorial();
            }
            else
            {
                HandleTurn();
            }
        }
    }
    
    // Receive moves
    private void ReceiveMove(Vector2Int from, Vector2Int to)
    {
        tiles[to.x, to.y].piece?.DestroyPiece();
        tiles[to.x, to.y].piece = tiles[from.x, from.y].piece;
        tiles[from.x, from.y].piece = null;
        
        audioHandler.moveKnight.PlayAudio();
        
        PositionPiece(ref tiles[to.x, to.y].piece, to);

        myTurn = true;
        if (!manaisOff)
        {
            currentMana += manaGrowth;
            MyMana = manaGrowth;
        }

        if (team != ChessPieceTeam.White)
        {
            if (to.y == 7 && tiles[to.x, to.y].piece.team != ChessPieceTeam.Black)
            {
                enemyPoints++;
                tiles[to.x, to.y].piece.DestroyPiece();
                tiles[to.x, to.y].piece = null;
                audioHandler.loosePoint.PlayAudio();
            }
        }
        else
        {
            if (to.y == 0 && tiles[to.x, to.y].piece.team != ChessPieceTeam.White)
            {
                enemyPoints++;
                tiles[to.x, to.y].piece.DestroyPiece();
                tiles[to.x, to.y].piece = null;
                audioHandler.loosePoint.PlayAudio();
            }
        }
    }

    // Highlight
    private void HighlightTileHandler()
    {
        UnHighlightAll();
        if (selectedBehavior is not null)
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
                    case CardType.Special:
                        HighlightReverseMovablePieces();
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
            Vector2Int move = currentlyDragging.boardPosition + (team == ChessPieceTeam.White ? movementPattern.move : movementPattern.move * new Vector2Int(1, -1));
            
            if (move.x < 0) continue;
            if (move.x > 7) continue;
            if (move.y < 0) move.y = 0;
            if (move.y > 7) move.y = 7;

            ChessPiece piece = tiles[move.x, move.y].piece;
            if (piece is null)
            {
                if (movementPattern.moveType is MoveType.MoveOnly or MoveType.MoveAndCapture or MoveType.MoveAndNoCapture)
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
                if (tiles[x, y].piece?.type == selectedBehavior.piecesAffected && tiles[x, y].piece.team == team) // TODO: not needed!
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
            if (tiles[x, teamSide].piece is null)
            {
                HighlightTile(x, teamSide);
            }
        }
    }
    
    private void HighlightReverseMovablePieces() 
    {
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                if (tiles[x, y].piece?.type == selectedBehavior.piecesAffected && tiles[x, y].piece.team != team) // TODO: not needed!
                {
                    HighlightTile(x, y);
                }
            }
        }
    }

    private void HighlightTile(int x, int y)
    {
        tiles[x, y].highlighted = true;
        tiles[x, y].GetComponent<Renderer>().renderingLayerMask = (uint)LayerMask.NameToLayer("hover");
    }

    private void UnHighlightAll()
    {
        foreach (Tile tile in tiles)
        {
            tile.highlighted = false;
        }
    }

    // Spawning pieces
    private void SpawnPiece(Vector2Int position)
    {
        InstantiatePiece(position, ChessPieceType.Pawn, team);
        
        audioHandler.summonKnight.PlayAudio();
        
        //Net Implementation
        if (tutorialGame)
        {
            return;
        }
        
        NetSpawnPiece sp = new NetSpawnPiece();
        sp.spawnX = position.x;
        sp.spawnY = position.y;
        sp.teamId = currentTeam;
        Client.Instace.SendToServer(sp);
    }

    private void ReceiveSpawnedPiece(Vector2Int position, int teamId) // teamId could be replaced with reversing own team
    {
        InstantiatePiece(position, ChessPieceType.Pawn, teamId == 0 ? ChessPieceTeam.White : ChessPieceTeam.Black);
        audioHandler.summonKnight.PlayAudio();
        myTurn = true;
        if (!manaisOff)
        {
            currentMana += manaGrowth;
            MyMana = manaGrowth;
        }
    }

    private void InstantiatePiece(Vector2Int position, ChessPieceType type, ChessPieceTeam team) // fix for all types
    {
        ChessPiece piece = Instantiate(pawn).GetComponent<ChessPiece>();
        piece.transform.parent = pieceContainer.transform;

        piece.team = team;

        piece.GetComponent<MeshRenderer>().material = team == ChessPieceTeam.White ? whiteTeamMaterial : blackTeamMaterial;
        
        if (team == ChessPieceTeam.Black)
        {
            piece.transform.eulerAngles = transform.eulerAngles + 180f * Vector3.up;
        }

        PositionPiece(ref piece, position, true);
        piece.boardPosition = position;

        tiles[position.x, position.y].piece = piece;
        
    }

    // Position pieces
    private void PositionPiece(ref ChessPiece piece, Vector2Int position, bool spawning = false)
    {
        tiles[piece.boardPosition.x, piece.boardPosition.y].selected = false;
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
    private void HoverTileHandler()
    {
        Tile hoveredTile = GetHoveredTile();

        if (currentHover is not null) currentHover.hovered = false;
        if (hoveredTile is not null) hoveredTile.hovered = true;
        
        currentHover = hoveredTile;
    }

    private Tile GetHoveredTile()
    {
        Tile foundTile = null;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, LayerMask.GetMask("Tile","Hover")))
        {
            GameObject hitGameObject = hitInfo.transform.gameObject;
            foreach (Tile tile in tiles)
            {
                if (tile.gameObject == hitGameObject)
                {
                    foundTile = tile;
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

        return foundTile;
    }

    // Generate tiles
    private void GenerateAllTiles()
    {
        GameObject container = CreateContainer("Tiles", transform);

        tiles = new Tile[TileCountX, TileCountY];
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                tiles[x, y] = new GameObject($"X:{x}, Y:{y}").AddComponent<Tile>().CreateTile(x, y, container, this);
            }
        }
    }

    // For handler TODO: improve or remove (make in to property)
    public int GetPieceAmount()
    {
        int amount = 0;
        foreach (Tile tile in tiles)
        {
            if (tile.piece?.team == team)
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

    private void HandleTurn()
    {
        if (!tutorialGame)
        {
            if (localGame)
            {

                GameUINet.Instance.ChangeCamera(team == ChessPieceTeam.White ? CameraAngle.blackTeam : CameraAngle.whiteTeam);
                if (!manaisOff)
                {
                    currentMana += team == ChessPieceTeam.White ? 0 : manaGrowth;
                    MyMana = currentMana;
                }

                team = team == ChessPieceTeam.White ? ChessPieceTeam.Black : ChessPieceTeam.White;
                myTimmer = startTimmer;
                textManager.GetText("Timer White",textCollection.GamePlay).color=Color.white;
                textManager.GetText("Timer Black",textCollection.GamePlay).color=Color.white;
            }
            else
            {
                myTurn = false;
            }
        }
        //tutorial only
        else
        {
            switch (tutorialGameStep)
            {
                case 3:
                    textManager.ResetTexts(textCollection.Tutorial);
                    textManager.EnableText("Title", textCollection.Tutorial, true);
                    textManager.EnableText("Info Placed", textCollection.Tutorial, true);
                    Invoke("DelayedTutorialAction", 0.6f);
                    break;
                case 5:
                    textManager.ResetTexts(textCollection.Tutorial);
                    textManager.EnableText("Title After Move", textCollection.Tutorial, true);
                    textManager.EnableText("Info Move", textCollection.Tutorial, true);
                    Invoke("DelayedTutorialAction", 0.6f);
                    break;
                case 7:
                    textManager.ResetTexts(textCollection.Tutorial);
                    textManager.EnableText("Title After Capture", textCollection.Tutorial, true);
                    textManager.EnableText("Info Score", textCollection.Tutorial, true);
                    Invoke("DelayedTutorialAction", 0.7f);
                    break;
                case 9:
                    Invoke("DelayedTutorialAction", 0.5f);
                    break;
            }

            if (PuzzleActive)
            {
                switch (cardDeckHandler.GetCurrentAmountCardsHeld())
                {
                    case 4:
                        ReceiveMove(new Vector2Int(7, 2), new Vector2Int(7, 1));
                        break;
                    case 3:
                        ReceiveMove(new Vector2Int(1, 6), new Vector2Int(1, 3));
                        break;
                    case 2:
                        ReceiveMove(new Vector2Int(2, 7), new Vector2Int(2, 6));
                        break;
                    default:
                        break;
                }
            }
            myTurn = true;
        }
    }
    
    private void TimerUpdate()
    {
        textManager.SetText($"White Timer: {startTimmer}","Timer White", textCollection.GamePlay);
        textManager.SetText($"Black Timer: {startTimmer}","Timer Black", textCollection.GamePlay);
        textManager.GetText("Timer White",textCollection.GamePlay).color=Color.white;
        textManager.GetText("Timer Black",textCollection.GamePlay).color=Color.white;
        if (TutorialGame)
        {
            textManager.SetText("","Timer White", textCollection.GamePlay);
            textManager.SetText("","Timer Black", textCollection.GamePlay);
            return;
        }
        else
        {
            if (myTurn)
            {
                enemyTimmer = startTimmer;
                timeBank += Time.deltaTime;

                if ((int)timeBank == 1)
                {
                    myTimmer--;
                    timeBank = 0;
                }

                switch (team)
                {
                    case ChessPieceTeam.White:
                        textManager.SetText($"White Timer: {myTimmer}","Timer White", textCollection.GamePlay);
                        break;
                    case ChessPieceTeam.Black:
                        textManager.SetText($"Black Timer: {myTimmer}","Timer Black", textCollection.GamePlay);
                        break;
                }
                
                if (myTimmer <= 5)
                {
                    switch (team)
                    {
                        case ChessPieceTeam.White:
                            textManager.GetText("Timer White",textCollection.GamePlay).color=Color.red;
                            break;
                        case ChessPieceTeam.Black:
                            textManager.GetText("Timer Black",textCollection.GamePlay).color=Color.red;
                            break;
                    }
                }

                if (myTimmer <= 0)
                {
                    HandleTurn();
                    textManager.GetText("Timer Black",textCollection.GamePlay).color=Color.white;
                    textManager.GetText("Timer White",textCollection.GamePlay).color=Color.white;
                    Debug.Log("heyo");
                }
            }
            else
            {
                myTimmer = startTimmer;
                if (!localGame)
                {
                    timeBank += Time.deltaTime;

                    if ((int)timeBank == 1)
                    {
                        enemyTimmer--;
                        timeBank = 0;
                    }
                    
                    switch (team)
                    {
                        case ChessPieceTeam.White:
                            textManager.SetText($"Black Timer: {enemyTimmer}","Timer Black",textCollection.GamePlay);
                            break;
                        case ChessPieceTeam.Black:
                            textManager.SetText($"White Timer: {enemyTimmer}","Timer White",textCollection.GamePlay);
                            break;
                    }
                    
                    if (enemyTimmer <= 5)
                    {
                        switch (team)
                        {
                            case ChessPieceTeam.White:
                                textManager.GetText("Timer Black",textCollection.GamePlay).color=Color.red;
                                break;
                            case ChessPieceTeam.Black:
                                textManager.GetText("Timer White",textCollection.GamePlay).color=Color.red;
                                break;
                        }
                    }
                    
                    if (enemyTimmer <= 0)
                    {
                        myTurn = true;
                        textManager.GetText("Timer Black",textCollection.GamePlay).color=Color.white;
                        textManager.GetText("Timer White", textCollection.GamePlay).color = Color.white;
                        Debug.Log("sewp");
                    }
                }
            }
        }
    }

    private void ManaUpdate()
    {
        textManager.SetText($"Mana: {MyMana}/{currentMana}","Mana Value",textCollection.GamePlay);
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

    #region tutorial

    private void DelayedTutorialAction()
    {
        switch (tutorialGameStep)
        {
            case 3:
            case 4:
                ReceiveSpawnedPiece(new Vector2Int(enemyPlayerSpawnTutorial.x,7),1);
                break;
            case 5:
            case 6:
                ReceiveMove(new Vector2Int(enemyPlayerSpawnTutorial.x,7), new Vector2Int(enemyPlayerSpawnTutorial.x,5));
                break;
            case 7:
            case 8:
                ReceiveSpawnedPiece(new Vector2Int(enemyPlayerSpawnTutorial.x,7),1);
                break;
            case 9:
            case 10:
                PuzzleSetUp();
                break;
        }
    }

    private void PuzzleSetUp()
    {
        if (puzzleSetUp)
        {
            ReceiveSpawnedPiece(new Vector2Int(3, 0), 0);

            ReceiveSpawnedPiece(new Vector2Int(3, 1), 1);
            ReceiveSpawnedPiece(new Vector2Int(2, 1), 1);
            ReceiveSpawnedPiece(new Vector2Int(2, 2), 1);
            ReceiveSpawnedPiece(new Vector2Int(3, 3), 1);
            
            ReceiveSpawnedPiece(new Vector2Int(2, 7), 1);
            
            //Deadweight
            
            ReceiveSpawnedPiece(new Vector2Int(3, 6), 1);
            ReceiveSpawnedPiece(new Vector2Int(7, 2), 1);
            ReceiveSpawnedPiece(new Vector2Int(5, 5), 1);
            ReceiveSpawnedPiece(new Vector2Int(0, 4), 1);
            ReceiveSpawnedPiece(new Vector2Int(1, 6), 1);
            ReceiveSpawnedPiece(new Vector2Int(0, 1), 1);

            PuzzleActive = true;
            puzzleSetUp = false;
        }
    }

    #endregion
    
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
        
        // Music
        audioHandler.menuMusic.Play();
        audioHandler.exitMenuMusic.StopAudio(audioHandler.fadeOut);
    }

    private void ResetGame()
    {
        //UI
        rematchButton.gameObject.SetActive(true);
        
        //Field reset
        currentlyDragging = null;
        playerRematch[0] = playerRematch[1] = false;

        //Clean up
        foreach (Tile tile in tiles)
        {
            tile.piece?.DestroyPiece();
            tile.piece = null;
        }

        //Set up all cards
        cardDeckHandler.ResetHand();

        //Points restart
        myPoints = 0;
        enemyPoints = 0;
    }

    public void OnMenuButton()
    {
        if (tutorialGame)
        {
            GameUINet.Instance.OnLeaveFromGameMenu();
            cardDeckHandler.ResetHand(5);
            audioHandler.menuMusic.Play();
            audioHandler.exitMenuMusic.StopAudio(audioHandler.fadeOut);
            return;
        }
        NetRematch rm = new NetRematch();
        rm.teamId = currentTeam;
        rm.wantRematch = 0;
        Client.Instace.SendToServer(rm);
        
        GameUINet.Instance.OnLeaveFromGameMenu();
        
        cardDeckHandler.ResetHand(5);
        
        Invoke("ShutdownRelay", 1.0f);
        
        // Music
        audioHandler.menuMusic.Play();
        audioHandler.exitMenuMusic.StopAudio(audioHandler.fadeOut);
        
        //resetValues
        playerCount = -1;
        currentTeam = -1;
    }
    
    public void OnMenuButtonPause()
    {
        if (tutorialGame)
        {
            GameUINet.Instance.OnLeaveFromGameMenuWithPause();
            audioHandler.menuMusic.Play();
            audioHandler.exitMenuMusic.StopAudio(audioHandler.fadeOut);
            return;
        }
        NetRematch rm = new NetRematch();
        rm.teamId = currentTeam;
        rm.wantRematch = 0;
        Client.Instace.SendToServer(rm);
        
        GameUINet.Instance.OnLeaveFromGameMenuWithPause();

        Invoke("ShutdownRelay", 1.0f);
        
        // Music
        audioHandler.menuMusic.Play();
        audioHandler.exitMenuMusic.StopAudio(audioHandler.fadeOut);
        
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
        GameUINet.Instance.SetTutorialGame += OnSetTutorialGame;
        GameUINet.Instance.SetTutorialGameStep += OnSetTutorialGameStep;
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
        GameUINet.Instance.SetTutorialGame -= OnSetTutorialGame;
        GameUINet.Instance.SetTutorialGameStep -= OnSetTutorialGameStep;
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
        audioHandler.menuMusic.Stop();
        audioHandler.summonGame.PlayAudio();
        audioHandler.entryStinger.PlayAudio();
        audioHandler.playList.PlayAudio();
        audioHandler.ambLoop.PlayAudio();
        audioHandler.fire.PlayAudio();
        audioHandler.randomSounds.PlayAudio();
        
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
                rematchButton.gameObject.SetActive(false);
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
        ResetGame();
    }
    
    private void OnSetTutorialGame(bool obj)
    {
        playerCount = -1;
        currentTeam = -1;
        tutorialGame = obj;
        myTurn = false;
        ResetGame();
        cardDeckHandler.ResetHand(1);
        team = ChessPieceTeam.White;
        puzzleSetUp = true;
    }
    
    private void OnSetTutorialGameStep(int obj)
    {
        tutorialGameStep = obj;

        if (obj == 3)
        {
            myTurn = true;
        }
    }

    #endregion
}
