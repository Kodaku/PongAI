using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

using HashedState = System.Tuple<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2>;

[System.Serializable]
public class PlayerState
{
    private float[] playerPosition;
    private float[] ballPosition;
    private float[] playerDirection;
    private float[] ballDirection;
    private PlayerAction[] playerActions;
    private PlayerState[] nextStates;
    private Action[] actionSpace = new Action[]{Action.DOWN, Action.UP, Action.IDLE};
    [System.NonSerialized]
    private HashedState hashedState;
    private float epsilon = 0.1f;
    private float[] tau;
    private bool m_isTerminal = false;

    public PlayerState(Vector2 _playerPosition, Vector2 _ballPosition, Vector2 _playerDirection, Vector2 _ballDirection)
    {
        playerPosition = new float[]{_playerPosition.x, _playerPosition.y};
        ballPosition = new float[]{_ballPosition.x, _ballPosition.y};
        playerDirection = new float[]{_playerDirection.x, _playerDirection.y};
        ballDirection = new float[]{_ballDirection.x, _ballDirection.y};
        GenerateHashCode();
        GenerateBasicActions();
        InitializeNextStates();
        InitializeTau();
    }

    public bool isTerminal
    {
        get { return m_isTerminal; }
        set { m_isTerminal = value; }
    }

    public void GenerateHashCode()
    {
        Vector2 statePlayerPosition = new Vector2(playerPosition[0], playerPosition[1]);
        Vector2 stateBallPosition = new Vector2(ballPosition[0], ballPosition[1]);
        Vector2 statePlayerDirection = new Vector2(playerDirection[0], playerDirection[1]);
        Vector2 stateBallDirection = new Vector2(ballDirection[0], ballDirection[1]);
        hashedState = Tuple.Create(statePlayerPosition, stateBallPosition, statePlayerDirection, stateBallDirection);
    }

    private void GenerateBasicActions()
    {
        int totalActions = actionSpace.Length;
        playerActions = new PlayerAction[totalActions];
        foreach(Action action in actionSpace)
        {
            PlayerAction playerAction = new PlayerAction(action, 1.0f / totalActions);
            playerActions[(int)action] = playerAction;
        }
    }

    private void InitializeNextStates()
    {
        int totalActions = actionSpace.Length;
        nextStates = new PlayerState[totalActions];
        foreach(Action action in actionSpace)
        {
            nextStates[(int)action] = null;
        }
    }

    private void InitializeTau()
    {
        int totalActions = actionSpace.Length;
        tau = new float[totalActions];
        foreach(Action action in actionSpace)
        {
            tau[(int)action] = 0.0f;
        }
    }

    public void UpdateProbabilities(Action bestAction)
    {
        int totalActions = actionSpace.Length;
        foreach(PlayerAction playerAction in playerActions)
        {
            if(playerAction.name == bestAction)
            {
                playerAction.probability = 1.0f - epsilon + (epsilon / totalActions);
            }
            else
            {
                playerAction.probability = (epsilon / totalActions);
            }
        }
    }

    public Action ChooseAction()
    {
        float sum = 0.0f;
        List<float> distribution = new List<float>();
        foreach(PlayerAction playerAction in playerActions)
        {
            distribution.Add(playerAction.probability);
        }

        List<float> cumulative = distribution.Select(c => {
            var result = sum + c;
            sum += c;
            return result;
        }).ToList();

        float r = UnityEngine.Random.value;
        int idx = cumulative.BinarySearch(r);
        if(idx < 0)
        {
            idx = ~idx;
        }
        if(idx > cumulative.Count - 1)
        {
            idx = cumulative.Count - 1;
        }

        return playerActions[idx].name;
    }

    public Action GetBestAction()
    {
        Action bestAction = Action.IDLE;
        float maxActionValue = Mathf.NegativeInfinity;
        foreach(PlayerAction playerAction in playerActions)
        {
            float actionValue = playerAction.qValue;
            if(actionValue > maxActionValue)
            {
                maxActionValue = actionValue;
                bestAction = playerAction.name;
            }
        }
        return bestAction;
    }

    public void SetNextStateAndReward(Action currentAction, PlayerState nextState, float reward)
    {
        nextStates[(int)currentAction] = nextState;
        PlayerAction currentPlayerActionInstance = playerActions[(int)currentAction];
        currentPlayerActionInstance.reward = reward;
    }

    public Tuple<PlayerState, PlayerState, float, float> GetRandomNextStateAndReward()
    {
        List<Action> validActions = new List<Action>();
        foreach(Action action in actionSpace)
        {
            if(nextStates[(int)action] != null)
            {
                validActions.Add(action);
            }
        }
        int totalActions = validActions.Count;
        // Debug.Log(totalActions);
        if(totalActions > 0)
        {
            int index = UnityEngine.Random.Range(0, totalActions);
            Action randomAction = validActions[index];
            PlayerAction randomTakenAction = playerActions[(int)randomAction];
            // Debug.Log(randomAction + ", " + nextStates.Length);
            // Debug.Log(nextStates[0] + ", " + nextStates[1] + ", " + nextStates[2] + ", " + nextStates[3]);
            return Tuple.Create(this, nextStates[(int)randomAction], randomTakenAction.reward, tau[(int)randomAction]);
        }
        return null;
    }

    public float GetMaxQ()
    {
        float maxQ = Mathf.NegativeInfinity;
        foreach(PlayerAction playerAction in playerActions)
        {
            float qValue = playerAction.qValue;
            if(qValue > maxQ)
            {
                maxQ = qValue;
            }
        }

        return maxQ;
    }

    public float GetQ(Action currentAction)
    {
        return playerActions[(int)currentAction].qValue;
    }

    public void UpdateActionValue(Action currentAction, float maxQ, float reward, bool isEndState)
    {
        m_isTerminal = isEndState;
        PlayerAction playerAction = playerActions[(int)currentAction];
        playerActions[(int)currentAction].qValue += 0.01f * (reward + 1.0f * maxQ - playerAction.qValue);
    }

    public void IncreaseTau()
    {
        foreach(Action action in actionSpace)
        {
            tau[(int)action] += 1.0f;
        }
    }

    public void ResetTau(Action currentAction)
    {
        tau[(int)currentAction] = 0.0f;
    }

    public HashedState GetHashedState()
    {
        return hashedState;
    }
}
