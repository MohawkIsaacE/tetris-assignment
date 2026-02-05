using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public UIController uiController;
    public ParticleSystem bountyParticle;

    public TetrominoData[] tetrominos;
    public Piece piecePrefab;

    public Vector2Int startPosition;
    public Tilemap tilemap;
    public Vector2Int boardSize;

    public float dropInterval = 0.5f;
    private float time;

    private Piece activePiece;

    public int currentLineBounty = 4;

    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();

    // Board size getters
    int left
    {
        get { return -boardSize.x / 2; }
    }
    int right
    {
        get { return boardSize.x / 2; }
    }
    int top
    {
        get { return boardSize.y / 2; }
    }
    int bottom
    {
        get { return -boardSize.y / 2; }
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;

        time += Time.deltaTime;

        if (time >= dropInterval)
        {
            time = 0.0f;

            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moveResult)
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnRandomPiece();
            }
        }
    }

    private void Start()
    {
        tetrisManager.SetGameOver(false);
        //SpawnRandomPiece();
    }

    public void SpawnRandomPiece()
    {
        activePiece = Instantiate(piecePrefab);

        activePiece.Initialize(this, (Tetromino)Random.Range(0, tetrominos.Length));

        CheckEndGame();

        // Randomize the next bounty
        currentLineBounty = Random.Range(1, 5);
        tetrisManager.SetNewBounty();

        Set(activePiece);
    }

    // Remapping the set tile functionality
    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);
            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);

            // Attaches the piece game object to the cell position
            pieces[cellPosition] = piece;
        }
    }

    // Set tile colour for this piece
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);
            
            // Check if position is already occupied
            if (tilemap.HasTile(cellPosition)) return false;

            // Make sure piece is in bounds
            if (cellPosition.x < left || cellPosition.x >= right || cellPosition.y < bottom || cellPosition.y >= top)
            {
                return false;
            }
        }

        return true;
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();

        for (int y = bottom;  y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        // Still need to fix because ?
        // Shift down
        int rowsShiftedDown = 0;
        foreach (int clearedRow in destroyedLines)
        {
            ShiftRowsDown(clearedRow - rowsShiftedDown);
            // Shift rows down more each time so there are no spaces
            rowsShiftedDown++;
        }
        
        int score = tetrisManager.CalculateScore(destroyedLines.Count);

        // If the player hit the bounty, multiply the points gained
        if (destroyedLines.Count == currentLineBounty)
        {
            score *= uiController.bountyMultiplier[destroyedLines.Count - 1];
            bountyParticle.Play();
        }

        tetrisManager.ChangeScore(score);
    }

    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                // Make sure there is something there so it doesn't crash while accessing nothing
                if (pieces.ContainsKey(cellPosition))
                {
                    // Save old piece
                    //TileBase currentTile = tilemap.GetTile(cellPosition);
                    Piece currentPiece = pieces[cellPosition];

                    // Clear position it is int
                    SetTile(cellPosition, null);

                    // Move new tile down
                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPiece);
                }
            }
        }
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (!tilemap.HasTile(cellPosition)) return false;
        }

        return true;
    }

    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            if (pieces.ContainsKey(cellPosition))
            {
                Piece activePiece = pieces[cellPosition];
                activePiece.ReduceActiveCount();
            }

            SetTile(cellPosition, null);
        }
    }

    void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver) ResetBoard();
    }

    void ResetBoard()
    {
        // Find all pieces on the board and destroy them
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        // clear the active piece
        activePiece = null;

        tilemap.ClearAllTiles();

        // Clear pieces dictionary
        pieces.Clear();

        SpawnRandomPiece();
    }
}
