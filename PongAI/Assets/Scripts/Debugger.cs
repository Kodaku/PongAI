using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private float scaledQ1;
    private float scaledQ2;
    public GameObject valueIdle;
    public GameObject valueUp;
    public GameObject valueDown;
    private Dictionary<Vector2, GameObject> valueBreadcrumbs1 = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, GameObject> valueBreadcrumbs2 = new Dictionary<Vector2, GameObject>();

    public void SetColors(float currentQ, float minQValue, float maxQValue, bool isPlayer1)
    {
        if(isPlayer1)
            scaledQ1 = (currentQ - minQValue) / (maxQValue - minQValue + Mathf.Epsilon);
        else
            scaledQ2 = (currentQ - minQValue) / (maxQValue - minQValue + Mathf.Epsilon);
    }

    public void SpawnMove(Player player, Cell ballCell, bool isPlayer1)
    {
        Dictionary<Vector2, GameObject> valueBreadcrumbs = isPlayer1 ? valueBreadcrumbs1 : valueBreadcrumbs2;
        Action playerAction = player.GetCurrentAction();
        Vector2 ballPosition = new Vector2(ballCell.column, ballCell.row);
        switch(playerAction)
        {
            case Action.IDLE:
            {
                if(valueBreadcrumbs.ContainsKey(ballPosition))
                {
                    Destroy(valueBreadcrumbs[ballPosition]);
                    valueBreadcrumbs.Remove(ballPosition);
                }
                GameObject newArrow = Instantiate(valueIdle, ballPosition, Quaternion.identity);
                newArrow.transform.parent = transform;
                SpriteRenderer spriteRenderer = newArrow.GetComponent<SpriteRenderer>();
                if(isPlayer1)
                    spriteRenderer.color = new Color(scaledQ1, scaledQ1, 0.0f);
                else
                    spriteRenderer.color = new Color(0.0f, scaledQ2, scaledQ1);
                valueBreadcrumbs.Add(ballPosition, newArrow);
                break;
            }
            case Action.UP:
            {
                if(valueBreadcrumbs.ContainsKey(ballPosition))
                {
                    Destroy(valueBreadcrumbs[ballPosition]);
                    valueBreadcrumbs.Remove(ballPosition);
                }
                GameObject newArrow = Instantiate(valueUp, ballPosition, Quaternion.identity);
                newArrow.transform.parent = transform;
                SpriteRenderer spriteRenderer = newArrow.GetComponent<SpriteRenderer>();
                if(isPlayer1)
                    spriteRenderer.color = new Color(scaledQ1, scaledQ1, 0.0f);
                else
                    spriteRenderer.color = new Color(0.0f, scaledQ2, scaledQ2);
                valueBreadcrumbs.Add(ballPosition, newArrow);
                break;
            }
            case Action.DOWN:
            {
                if(valueBreadcrumbs.ContainsKey(ballPosition))
                {
                    Destroy(valueBreadcrumbs[ballPosition]);
                    valueBreadcrumbs.Remove(ballPosition);
                }
                GameObject newArrow = Instantiate(valueDown, ballPosition, Quaternion.identity);
                newArrow.transform.parent = transform;
                SpriteRenderer spriteRenderer = newArrow.GetComponent<SpriteRenderer>();
                if(isPlayer1)
                    spriteRenderer.color = new Color(scaledQ1, scaledQ1, 0.0f);
                else
                    spriteRenderer.color = new Color(0.0f, scaledQ2, scaledQ2);
                valueBreadcrumbs.Add(ballPosition, newArrow);
                break;
            }
        }
    }

    public void Reset()
    {
        foreach(Vector2 value in valueBreadcrumbs1.Keys)
        {
            Destroy(valueBreadcrumbs1[value]);
        }
        valueBreadcrumbs1.Clear();
        foreach(Vector2 value in valueBreadcrumbs2.Keys)
        {
            Destroy(valueBreadcrumbs2[value]);
        }
        valueBreadcrumbs2.Clear();
    }
}
