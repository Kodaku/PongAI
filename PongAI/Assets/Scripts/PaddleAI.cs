using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleAI : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject ball;
    public GameObject upWall;
    public GameObject downWall;
    public GameObject paddleLimitUp;
    public GameObject paddleLimitDown;
    public float speed = 5.0f;
    public bool isPlayer1;
    // public GameObject safePart;
    public GameObject[] unsafeParts;
    private float movement;
    private string[] actionSpace = {"up", "down", "idle"};
    private string[] ballPositions = {"east", "middle-east", "middle", "middle-west", "west"};
    private string[] paddlePositions = {"up", "middle-up", "middle", "middle-down", "down"};
    private string[] ballStates = {"safe", "unsafe"};
    // private float moveReward = 0.0f;
    private float hitReward = 1.0f;
    // private float alignedReward = 1.0f;
    private float goalConcededReward = -100.0f;
    private List<Tuple<string, string, string, string>> stateSpace = new List<Tuple<string, string, string, string>>();
    private Dictionary<Tuple<string, string, string, string>, Dictionary<string, float>> Q = new Dictionary<Tuple<string, string, string, string>, Dictionary<string, float>>();
    private Dictionary<Tuple<string, string, string, string>, Dictionary<string, float>> policy = new Dictionary<Tuple<string, string, string, string>, Dictionary<string, float>>();
    private Tuple<string, string, string, string> currentState;
    private Tuple<string, string, string, string> previousState;
    private float currentReward = 0.0f;
    private string currentAction;
    private Vector3 startPosition;
    private bool hasHitBall = false;
    private bool hasConcededGoal = false;
    // private bool isCollisionSafe = false;
    // private bool hasCollided = false;
    private float updateTimer = 0.2f;
    private float currentTime = 0.0f;
    private float screenWidth;
    private float screenHeight;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        // print(paddleLimitUp.transform.position.y);
        GetStateSpace();
        InitializeQ();
        GetRandomPolicy();
        Reset();
        if(isPlayer1)
        {
            screenWidth = Vector3.Distance(transform.position, GameObject.Find("Player2").transform.position);
        }
        else
        {
            screenWidth = Vector3.Distance(transform.position, GameObject.Find("Player1").transform.position);
            print(screenWidth);
        }
        screenHeight = Vector3.Distance(upWall.transform.position, downWall.transform.position);
    }

    private void GetStateSpace()
    {
        foreach(string ballXPosition in ballPositions)
        {
            foreach(string ballState in ballStates)
            {
                foreach(string paddlePosition in paddlePositions)
                {
                    foreach(string ballYPosition in paddlePositions)
                    {
                        Tuple<string, string, string, string> state = Tuple.Create<string, string, string, string>(ballXPosition, paddlePosition, ballYPosition, ballState);
                        stateSpace.Add(state);
                    }
                }
            }
        }
    }

    private void InitializeQ()
    {
        foreach(Tuple<string, string, string, string> state in stateSpace)
        {
            Dictionary<string, float> actionsInState = new Dictionary<string, float>();
            foreach(string action in actionSpace)
            {
                actionsInState.Add(action, 0.0f);
            }
            Q.Add(state, actionsInState);
        }
    }

    private void GetRandomPolicy()
    {
        int n_actions = actionSpace.Length;
        foreach(Tuple<string, string, string, string> state in stateSpace)
        {
            Dictionary<string, float> actionsProbs = new Dictionary<string, float>();
            foreach(string action in actionSpace)
            {
                actionsProbs.Add(action, 1.0f / n_actions);
            }
            policy.Add(state, actionsProbs);
        }
    }

    public Dictionary<string, float> GetEpsilonGreedy(float epsilon, string bestAction)
    {
        Dictionary<string, float> actionsProbs = new Dictionary<string, float>();

        int numberOfActions = actionSpace.Length;

        foreach(string action in actionSpace)
        {
            if(action == bestAction)
            {
                actionsProbs.Add(action, 1.0f - epsilon + (epsilon / numberOfActions));
            }
            else
            {
                actionsProbs.Add(action, epsilon / numberOfActions);
            }
        }

        return actionsProbs;
    }
    private string ChooseAction()
    {
        float sum = 0;
        Dictionary<string, float> actions = policy[currentState];
        List<float> distribution = new List<float>();
        List<string> actionsKeys = new List<string>();
        foreach(string action in actions.Keys)
        {
            actionsKeys.Add(action);
            distribution.Add(actions[action]);
        }
        // first change shape of your distribution probablity array
        // we need it to be cumulative, that is:
        // if you have [0.1, 0.2, 0.3, 0.4] 
        // we need     [0.1, 0.3, 0.6, 1  ] instead
        List<float> cumulative = distribution.Select(c => {
            var result = c + sum;
            sum += c;
            return result;
        }).ToList();
        // now generate random double. It will always be in range from 0 to 1
        float r = UnityEngine.Random.value;
        // now find first index in our cumulative array that is greater or equal generated random value
        int idx = cumulative.BinarySearch(r);
        // if exact match is not found, List.BinarySearch will return index of the first items greater than passed value, but in specific form (negative)
        // we need to apply ~ to this negative value to get real index
        if (idx < 0)
            idx = ~idx; 
        if (idx > cumulative.Count - 1)
        idx = cumulative.Count - 1; // very rare case when probabilities do not sum to 1 becuase of double precision issues (so sum is 0.999943 and so on)
        // print(idx);
        // return item at given index
        return actionsKeys[idx];
    }

    public void HandleBallCollision()
    {
        float limitUp = upWall.transform.position.y - paddleLimitUp.transform.position.y;
        float limitDown = upWall.transform.position.y - paddleLimitDown.transform.position.y;
        float ballY = upWall.transform.position.y - ball.transform.position.y;

        if(ballY >= limitUp && ballY <= limitDown)
        {
            print("Hit");
            hasHitBall = true;
        }
        
        UpdateCycle();
    }

    private Tuple<string, string, string, string> UpdateState()
    {
        Vector3 ballPosition = ball.transform.position;
        float ballX = 0.0f;
        if(isPlayer1)
        {
            ballX = Mathf.Abs(transform.position.x + ball.transform.position.x);
        }
        else
        {
            ballX = (transform.position.x - ball.transform.position.x);
        }
        float ballY = upWall.transform.position.y - ballPosition.y;
        float paddleY = upWall.transform.position.y - transform.position.y;
        string discretizedBallXPosition = "";
        string discretizedBallYPosition = "";
        string discretizedPaddlePosition = "";
        if(ballX <= screenWidth / 4.0f)
        {
            discretizedBallXPosition = "east";
        }
        else if(ballX > (screenWidth / 4.0f) && ballX <= screenWidth * (3.0f / 8.0f))
        {
            discretizedBallXPosition = "middle-east";
        }
        else if(ballX > screenWidth * (3.0f / 8.0f) && ballX <= screenWidth * (5.0f / 8.0f))
        {
            discretizedBallXPosition = "middle";
        }
        else if(ballX > screenWidth * (5.0f / 8.0f) && ballX <= screenWidth * (3.0f / 4.0f))
        {
            discretizedBallXPosition = "middle-west";
        }
        else
        {
            discretizedBallXPosition = "west";
        }

        if(paddleY <= screenHeight / 4.0f)
        {
            discretizedPaddlePosition = "up";
        }
        else if(paddleY > (screenHeight / 4.0f) && paddleY <= screenHeight * (3.0f / 8.0f))
        {
            discretizedPaddlePosition = "middle-up";
        }
        else if(paddleY > screenHeight * (3.0f / 8.0f) && paddleY <= screenHeight * (5.0f / 8.0f))
        {
            discretizedPaddlePosition = "middle";
        }
        else if(paddleY > screenHeight * (5.0f / 8.0f) && paddleY <= screenHeight * (3.0f / 4.0f))
        {
            discretizedPaddlePosition = "middle-down";
        }
        else
        {
            discretizedPaddlePosition = "down";
        }

        if(ballY <= screenHeight / 4.0f)
        {
            discretizedBallYPosition = "up";
        }
        else if(ballY > (screenHeight / 4.0f) && ballY <= screenHeight * (3.0f / 8.0f))
        {
            discretizedBallYPosition = "middle-up";
        }
        else if(ballY > screenHeight * (3.0f / 8.0f) && ballY <= screenHeight * (5.0f / 8.0f))
        {
            discretizedBallYPosition = "middle";
        }
        else if(ballY > screenHeight * (5.0f / 8.0f) && ballY <= screenHeight * (3.0f / 4.0f))
        {
            discretizedBallYPosition = "middle-down";
        }
        else
        {
            discretizedBallYPosition = "down";
        }

        //Get the angle between the paddle and the ball
        float limitUp = upWall.transform.position.y - paddleLimitUp.transform.position.y;
        float limitDown = upWall.transform.position.y - paddleLimitDown.transform.position.y;
        string discretizedAngle = "";

        if(ballY >= limitUp && ballY <= limitDown)
        {
            discretizedAngle = "safe";
        }
        else
        {
            discretizedAngle = "unsafe";
        }

        Tuple<string, string, string, string> newState = Tuple.Create<string, string, string, string>(discretizedBallXPosition, discretizedPaddlePosition, discretizedBallYPosition, discretizedAngle);
        
        // print(newState[0] + " " + newState[1]);
        return newState;
    }

    private float GetReward()
    {
        if(hasConcededGoal)
        {
            // print(-(goalConcededReward * HeuristicReward()));
            return -(goalConcededReward * HeuristicReward());
        }
        else if(hasHitBall)
        {
            hasHitBall = false;
            return hitReward + HeuristicReward();
        }
        else
        {
            return HeuristicReward();
        }
    }

    private float HeuristicReward()
    {
        float ballY = upWall.transform.position.y - ball.transform.position.y;
        float limitUp = upWall.transform.position.y - paddleLimitUp.transform.position.y;
        float limitDown = upWall.transform.position.y - paddleLimitDown.transform.position.y;
        float reward = 0.0f;
        bool isUp = false;
        bool isDown = false;
        if(ballY < limitUp)
        {
            isUp = true;
        }
        if(ballY > limitDown)
        {
            isDown = true;
        }

        if(isUp)
        {
            reward = ballY - limitUp;
        }
        else if(isDown)
        {
            reward = limitDown - ballY;
        }
        else
        {
            // print("Aligned");
            reward = Mathf.Abs(limitUp - ballY);
        }

        return reward;
    }

    public void ConcededGoal()
    {
        // print("Goal");
        hasConcededGoal = true;
        UpdateCycle();
    }

    public void UpdateCycle()
    {
        currentState = UpdateState();
        currentReward = GetReward();
        float maxQInState = GetMaxQInState(currentState);
        Q[previousState][currentAction] += + 0.01f * (currentReward + 1.0f * maxQInState - Q[previousState][currentAction]);
        previousState = currentState;
    }

    private float GetMaxQInState(Tuple<string, string, string, string> state)
    {
        float maxQ = -Mathf.Infinity;
        Dictionary<string, float> qInState = Q[state];
        foreach(string action in qInState.Keys)
        {
            float qValue = qInState[action];
            if(maxQ < qValue)
            {
                maxQ = qValue;
            }
        }

        return maxQ;
    }

    private string GetBestAction()
    {
        // print(currentState.Item1 + " " + currentState.Item2);
        Dictionary<string, float> availableActions = Q[currentState];
        string bestAction = "";
        float maxActionValue = -Mathf.Infinity;
        foreach(string action in availableActions.Keys)
        {
            float actionValue = availableActions[action];
            if(actionValue > maxActionValue)
            {
                maxActionValue = actionValue;
                bestAction = action;
            }
        }

        return bestAction;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime > updateTimer)
        {
            currentTime = 0.0f;
            string bestAction = GetBestAction();
            policy[currentState] = GetEpsilonGreedy(0.1f, bestAction);
            currentAction = ChooseAction();
            switch(currentAction)
            {
                case "idle":
                    movement = 0.0f;
                    break;
                case "up":
                    movement = 1.0f;
                    break;
                case "down":
                    movement = -1.0f;
                    break;
            }
            UpdateCycle();
        }
        
        rb.velocity = new Vector2(movement * rb.velocity.x, movement * speed);
        // UpdateCycle();
    }

    public void Reset()
    {
        previousState = Tuple.Create<string, string, string, string>("middle", "middle", "middle", "safe");
        rb.velocity = Vector2.zero;
        transform.position = startPosition;
        hasConcededGoal = false;
        hasHitBall = false;
        currentAction = "up";
        currentTime = 0.0f;
        UpdateCycle();
    }
}
