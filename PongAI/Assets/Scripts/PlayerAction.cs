using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAction
{
    private Action m_name;
    private float m_probability;
    private float m_qValue;
    private float m_reward;

    public PlayerAction(Action name, float probability)
    {
        m_name = name;
        m_probability = probability;
        m_qValue = 0.0f;
    }

    public Action name
    {
        get { return m_name; }
        set { m_name = value; }
    }

    public float probability
    {
        get { return m_probability; }
        set { m_probability = value; }
    }

    public float qValue
    {
        get { return m_qValue; }
        set { m_qValue = value; }
    }

    public float reward
    {
        get { return m_reward; }
        set { m_reward = value; }
    }
}
