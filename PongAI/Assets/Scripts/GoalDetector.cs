using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public bool isPlayer1;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Ball"))
        {
            if(!isPlayer1)
            {
                GameObject.Find("Player2").GetComponent<PaddleAI2>().ConcededGoal();
                // GameObject.Find("Player2").GetComponent<PaddleAI>().EvaluateAngleOnGoal();
            }
            else
            {
                GameObject.Find("Player1").GetComponent<PaddleAI2>().ConcededGoal();
                // GameObject.Find("Player1").GetComponent<PaddleAI>().EvaluateAngleOnGoal();
            }
        }
    }
}
