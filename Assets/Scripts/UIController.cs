using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    public void UIUpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score}";
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
