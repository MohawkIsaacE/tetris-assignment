using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    // Public so other code can access it
    // Do not want to show in editor
    public int score { get; private set; }
    public bool gameOver { get; private set; }

    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;
    public UnityEvent OnNewPiece;

    private void Start()
    {
        SetGameOver(true);
    }

    public int CalculateScore(int linesCleared)
    {
        switch(linesCleared)
        {
            case 1: return 100;
            case 2: return 300;
            case 3: return 500;
            case 4: return 800;
            default: return 0;
        }
    }
    
    public void ChangeScore(int amount)
    {
        score += amount;
        OnScoreChanged.Invoke();
    }

    public void SetGameOver(bool gameOver)
    {
        if (!gameOver)
        {
            score = 0;
            ChangeScore(0);
        }

        this.gameOver = gameOver;
        OnGameOver.Invoke();
    }

    public void SetNewBounty()
    {
        OnNewPiece.Invoke();
    }
}
