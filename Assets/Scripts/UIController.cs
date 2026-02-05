using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Board board;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bountyText;
    public GameObject gameOverPanel;

    public int[] bountyMultiplier { get; private set; } = new int[] { 10, 8, 6, 4 };

    public void UIUpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score}";
    }

    public void UIUpdateBounty()
    {
        bountyText.text = $"Score {board.currentLineBounty} line(s) to earn {bountyMultiplier[board.currentLineBounty-1]}x points!";
    }

    // When the GameOver state changes, update the UI
    public void UpdateGameOver()
    {
        gameOverPanel.SetActive(tetrisManager.gameOver);
    }

    // Click play again to reset the game
    public void PlayAgain()
    {
        tetrisManager.SetGameOver(false);
    }
}
