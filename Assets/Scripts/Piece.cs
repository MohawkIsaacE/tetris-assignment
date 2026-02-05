using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetrominoData data;
    public Vector2Int[] cells;
    public Vector2Int position;
    public Board board;

    public bool freeze = false;
    private int activePieceAmount;

    public void Initialize(Board board, Tetromino tetromino)
    {
        // Piece object needs reference to game board
        this.board = board;

        // Find tetromino data and attach it
        for (int i = 0; i < board.tetrominos.Length; i++)
        {
            if (board.tetrominos[i].tetromino == tetromino)
            {
                this.data = board.tetrominos[i];
                break;
            }
        }

        // Create a copy of the tetromino local cell coords
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < cells.Length; i++) cells[i] = data.cells[i];

        position = board.startPosition;

        activePieceAmount = cells.Length;
    }

    private void Update()
    {
        // If game is over, don't process input
        if (board.tetrisManager.gameOver) return;

        // If piece is frozen, do not move
        if (freeze) return;

        board.Clear(this);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            // Using WASD to move, Arrow Keys to rotate
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }
            //else if (Input.GetKeyDown(KeyCode.W))
            //{
            //    Move(Vector2Int.up);
            //}

            // Using arrow keys to rotate piece
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Counter clockwise
                Rotate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Clockwise
                Rotate(1);
            }
        }

        board.Set(this);
        if (freeze)
        {
            board.CheckBoard();
            board.SpawnRandomPiece();
            //board.SpawnSequence();
        }
    }

    // Hard drop
    void HardDrop()
    {
        // Keep moving down until translation is invalid
        while (Move(Vector2Int.down))
        {
            // Do nothing
        }
        freeze = true;
    }

    // Rotation
    void Rotate(int direction)
    {
        // Store cell locations so we can revert it
        Vector2Int[] temporaryCells = new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            temporaryCells[i] = cells[i];
        }

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks())
            {
                RevertRotation(temporaryCells);
            }
            //else
            //{
            //    Debug.Log("Wall kick successful");
            //}
        }
        //else
        //{
        //    Debug.Log("Valid rotation");
        //}
    }

    bool TryWallKicks()
    {
        //Vector2Int[] wallKickOffsets = new Vector2Int[]
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>()
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1), // Diagonal down-right
            new Vector2Int(1, -1)   // Diagonal down-left
        };

        // Special case for rotating I pieces
        if (data.tetromino == Tetromino.I)
        {
            wallKickOffsets.Add(Vector2Int.left * 2);
            wallKickOffsets.Add(Vector2Int.right * 2);
        }

        foreach (Vector2Int offSet in wallKickOffsets)
        {
            if (Move(offSet)) return true;
        }

        return false;
    }

    void RevertRotation(Vector2Int[] temporaryCells)
    {
        for (int i = 0; i < temporaryCells.Length;i++) cells[i] = temporaryCells[i];
    }

    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90f * direction);

        bool isVIP = data.tetromino == Tetromino.I || data.tetromino == Tetromino.O;

        for (int i = 0; i < cells.Length; i++)
        {
            // Local cell position
            //Vector2Int cellPosition = cells[i];

            // Cast to vector 3
            Vector3 cellPositionV3 = new Vector3(cells[i].x, cells[i].y);

            if (isVIP)
            {
                cellPositionV3.x -= 0.5f;
                cellPositionV3.y -= 0.5f;
            }

            Vector3 result = rotation * cellPositionV3;

            // Take result and apply to cell data
            if (isVIP)
            {
                cells[i] = new Vector2Int(
                Mathf.CeilToInt(result.x),
                Mathf.CeilToInt(result.y));
            }
            else
            {
                cells[i] = new Vector2Int(
                Mathf.RoundToInt(result.x),
                Mathf.RoundToInt(result.y));
            }
        }
    }

    // Move returns position validitiy
    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;

        newPosition += translation;

        // If new position is valid, move to that new position
        bool isValid = board.IsPositionValid(this, newPosition);
        if (board.IsPositionValid(this, newPosition)) position = newPosition;
        return isValid;
    }

    public void ReduceActiveCount()
    {
        activePieceAmount -= 1;

        // Destroy self when there is no cells left of the piece
        if (activePieceAmount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
