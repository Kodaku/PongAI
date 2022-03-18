using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using State = System.Tuple<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2>;

public class Player : MonoBehaviour
{
    public float speed;
    private Rigidbody2D rb;
    private float movement = 0.0f;
    private Vector2 startPosition;
    private State currentState;
    private State nextState;
    private Policy policy;
    private ActionValue q;
    private Model model;
    private Action currentAction;
    public delegate void OnHitBall();
    public OnHitBall hitBallObservers;
    private float kappa = 0.001f;
    // Start is called before the first frame update
    public void Initialize()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    private State GetState(MyGrid grid, Ball ball)
    {
        Cell playerCell = grid.WorldPointToCell(transform.position);
        Cell ballCell = grid.WorldPointToCell(ball.transform.position);

        Vector2 playerPosition = new Vector2(playerCell.column, playerCell.row);
        Vector2 ballPosition = new Vector2(ballCell.column, ballCell.row);

        Vector2 playerDirection = rb.velocity.normalized;
        Vector2 ballDirection = ball.GetVelocityDirection();

        return Tuple.Create(playerPosition, ballPosition, playerDirection, ballDirection);
    }

    public void SetCurrentState(MyGrid grid, Ball ball)
    {
        currentState = GetState(grid, ball);
        if(!q.ContainsState(currentState))
        {
            PlayerState playerState = new PlayerState(currentState.Item1, currentState.Item2, currentState.Item3, currentState.Item4);
            q.AddPlayerState(playerState);
        }
        if(!policy.ContainsState(currentState))
        {
            PlayerState playerState = new PlayerState(currentState.Item1, currentState.Item2, currentState.Item3, currentState.Item4);
            policy.AddPlayerState(playerState);
        }
        if(!model.ContainsState(currentState))
        {
            PlayerState playerState = new PlayerState(currentState.Item1, currentState.Item2, currentState.Item3, currentState.Item4);
            model.AddPlayerState(playerState);
        }
    }

    public void SetNextState(MyGrid grid, Ball ball)
    {
        nextState = GetState(grid, ball);
        if(!q.ContainsState(nextState))
        {
            PlayerState playerState = new PlayerState(nextState.Item1, nextState.Item2, nextState.Item3, nextState.Item4);
            q.AddPlayerState(playerState);
        }
        if(!policy.ContainsState(nextState))
        {
            PlayerState playerState = new PlayerState(nextState.Item1, nextState.Item2, nextState.Item3, nextState.Item4);
            policy.AddPlayerState(playerState);
        }
        if(!model.ContainsState(currentState))
        {
            PlayerState playerState = new PlayerState(currentState.Item1, currentState.Item2, currentState.Item3, currentState.Item4);
            model.AddPlayerState(playerState);
        }
    }

    public void SelectAction()
    {
        Action bestAction = GetBestAction();
        policy.EpsilonGreedyUpdate(currentState, bestAction);
        currentAction = policy.ChooseAction(currentState);
    }

    private Action GetBestAction()
    {
        if(q.ContainsState(currentState))
        {
            return q.GetBestAction(currentState);
        }
        else
        {
            PlayerState playerState = new PlayerState(currentState.Item1, currentState.Item2, currentState.Item3, currentState.Item4);
            q.AddPlayerState(playerState);
            policy.AddPlayerState(playerState);
            return GetBestAction();
        }
    }

    // Update is called once per frame
    public void Move()
    {
        switch(currentAction)
        {
            case Action.IDLE:
            {
                movement = 0.0f;
                break;
            }
            case Action.UP:
            {
                movement = 1.0f;
                break;
            }
            case Action.DOWN:
            {
                movement = -1.0f;
                break;
            }
        }
        rb.velocity = new Vector2(rb.velocity.x, movement * speed);
    }

    public void Reset()
    {
        rb.velocity = Vector2.zero;
        // startPosition = new Vector2(startPosition.x, UnityEngine.Random.Range(1, 20));
        transform.position = startPosition;
    }

    private void OnCollisionEnter2D(Collision2D collider)
    {
        if(collider.gameObject.CompareTag("Ball"))
        {
            hitBallObservers();
        }
    }

    public void UpdateAllTau()
    {
        model.IncreaseAllTau();
        model.ResetTau(currentState, currentAction);
    }

    public void QUpdate(float reward, bool isEndState)
    {
        if(!isEndState)
        {
            float maxQ = GetMaxQ(nextState);
            // Q[currentState][currentAction] += 0.01f * (reward + 1.0f * maxQ - Q[currentState][currentAction]);
            q.Update(currentState, currentAction, maxQ, reward, isEndState);
        }
        else
            // Q[currentState][currentAction] += 0.01f * (reward - Q[currentState][currentAction]);
            q.Update(currentState, currentAction, 0.0f, reward, isEndState);
        // currentState = nextState;
    }

    public void UpdateModel(float reward)
    {
        model.Update(currentState, currentAction, nextState, reward);
        currentState = nextState;
    }

    public void RunSimulation()
    {
        Tuple<PlayerState, PlayerState, float, float> simulationResult = model.GetRandomPlayerStateAndReward();
        if(simulationResult != null)
        {
            State currentSimulatedState = simulationResult.Item1.GetHashedState();
            State nextSimulatedState = simulationResult.Item2.GetHashedState();
            // print(currentSimulatedState);
            // print(nextSimulatedState);
            float simulationTau = simulationResult.Item4;
            float simulationReward = simulationResult.Item3 + kappa * Mathf.Sqrt(simulationTau);
            if(!simulationResult.Item1.isTerminal)
            {
                float maxQ = GetMaxQ(nextSimulatedState);
                q.Update(currentSimulatedState, currentAction, maxQ, simulationReward, simulationResult.Item1.isTerminal);
            }
            else
                q.Update(currentSimulatedState, currentAction, 0.0f, simulationReward, simulationResult.Item1.isTerminal);
        }
    }

    private float GetMaxQ(State nextState)
    {
        if(q.ContainsState(nextState))
        {
            return q.GetMaxQ(nextState);
        }
        else
        {
            PlayerState playerState = new PlayerState(nextState.Item1, nextState.Item2, nextState.Item3, nextState.Item4);
            policy.AddPlayerState(playerState);
            q.AddPlayerState(playerState);
            model.AddPlayerState(playerState);
            return GetMaxQ(nextState);
        }
    }

    public float GetQ()
    {
        return q.GetQ(currentState, currentAction);
    }

    public Action GetCurrentAction()
    {
        return currentAction;
    }

    public void SaveData(string policyName, string qName, string modelName)
    {
        Serializer.WriteToBinaryFile<Policy>("Assets/Resources/" + policyName + ".txt", policy);
        Serializer.WriteToBinaryFile<ActionValue>("Assets/Resources/" + qName +".txt", q);
        Serializer.WriteToBinaryFile<Model>("Assets/Resources/" + modelName, model);
    }

    public void LoadData(string policyName, string qName, string modelName)
    {
        if(System.IO.File.Exists("Assets/Resources/" + qName + ".txt"))
        {
            //Load Q and Policy and initialize the StateDictionary
            policy = Serializer.ReadFromBinaryFile<Policy>("Assets/Resources/" + policyName + ".txt");
            q = Serializer.ReadFromBinaryFile<ActionValue>("Assets/Resources/" + qName + ".txt");
            model = Serializer.ReadFromBinaryFile<Model>("Assets/Resources/" + modelName + ".txt");
            policy.InitializeStateDictionary();
            q.InitializeStateDictionary();
            model.InitializeStateDictionary();
        }
        else
        {
            policy = new Policy();
            q = new ActionValue();
            model = new Model();
        }
    }
}
