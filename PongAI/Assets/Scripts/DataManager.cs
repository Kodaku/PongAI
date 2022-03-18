using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private Observation observation;
    private float episodeDuration = 0.0f;
    private int numberOfSteps = 0;
    private float minQValue1 = Mathf.Infinity;
    private float minQValue2 = Mathf.Infinity;
    private float maxQValue1 = Mathf.NegativeInfinity;
    private float maxQValue2 = Mathf.NegativeInfinity;
    private float qValue1 = 0.0f;
    private float qValue2 = 0.0f;
    private int hitCount1 = 0;
    private int hitCount2 = 0;
    private bool hasPlayer1Scored;

    public DataManager()
    {
        observation = new Observation();
    }

    public void UpdateTimer(float timer)
    {
        episodeDuration += timer;
        numberOfSteps++;
    }

    public void IncreaseHitCount(bool isPlayer1)
    {
        if(isPlayer1)
        {
            hitCount1++;
        }
        else
        {
            hitCount2++;
        }
    }

    public void Player1Scored()
    {
        hasPlayer1Scored = true;
    }

    public void Player2Scored()
    {
        hasPlayer1Scored = false;
    }

    public void SetMinMaxQValues(float qValue, bool isPlayer1)
    {
        if(isPlayer1)
        {
            if(qValue > maxQValue1)
            {
                maxQValue1 = qValue;
            }
            if(qValue < minQValue1)
            {
                minQValue1 = qValue;
            }
        }
        else
        {
            if(qValue > maxQValue2)
            {
                maxQValue2 = qValue;
            }
            if(qValue < minQValue2)
            {
                minQValue2 = qValue;
            }
        }
    }

    public float GetMinQValue(bool isPlayer1)
    {
        if(isPlayer1)
            return minQValue1;
        return minQValue2;
    }

    public float GetMaxQValue(bool isPlayer1)
    {
        if(isPlayer1)
            return maxQValue1;
        return maxQValue2;
    }

    public void SetQValues(float q1, float q2)
    {
        qValue1 = q1;
        qValue2 = q2;
    }

    public void RegisterObservation()
    {
        observation.episodeDuration = episodeDuration;
        observation.numberOfSteps = numberOfSteps;
        observation.qMin1 = minQValue1;
        observation.qMin2 = minQValue2;
        observation.qMax1 = maxQValue1;
        observation.qMax2 = maxQValue2;
        observation.qValue1 = qValue1;
        observation.qValue2 = qValue2;
        observation.hitCount1 = hitCount1;
        observation.hitCount2 = hitCount2;
        observation.hasPlayer1Scored = hasPlayer1Scored;

        observation.SaveToFile(append:true);
    }

    public void Reset()
    {
        episodeDuration = 0.0f;
        numberOfSteps = 0;
        hitCount1 = 0;
        hitCount2 = 0;
    }
}
