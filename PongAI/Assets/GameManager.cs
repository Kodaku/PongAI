using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Ball")]
    public GameObject ball;
    [Header("Player1")]
    public GameObject player1Paddle;
    public GameObject player1Goal;
    [Header("Player2")]
    public GameObject player2Paddle;
    public GameObject player2Goal;
    [Header("Score UI")]
    public GameObject player1Text;
    public GameObject player2Text;

    private int player1Score;
    private int player2Score;

    public void Player1Scored()
    {
        player1Score++;
        player1Text.GetComponent<TextMeshProUGUI>().text = player1Score.ToString();
        // player2Paddle.GetComponent<PaddleAI>().EvaluateAngleOnGoal();
        // player2Paddle.GetComponent<PaddleAI>().ConcededGoal();
        // player2Paddle.GetComponent<PaddleAI>().UpdateCycle();
        ResetPosition();
    }

    public void Player2Scored()
    {
        player2Score++;
        player2Text.GetComponent<TextMeshProUGUI>().text = player2Score.ToString();
        // player1Paddle.GetComponent<PaddleAI>().ConcededGoal();
        // player1Paddle.GetComponent<PaddleAI>().UpdateCycle();
        ResetPosition();
    }
    
    private void ResetPosition()
    {
        ball.GetComponent<Ball>().Reset();
        player1Paddle.GetComponent<PaddleAI2>().Reset();
        player2Paddle.GetComponent<PaddleAI2>().Reset();
    }
}
