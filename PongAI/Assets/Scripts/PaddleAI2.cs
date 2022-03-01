using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PaddleAI2 : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject ball;
    public GameObject upWall;
    public GameObject downWall;
    public GameObject yLimit1;
    public GameObject yLimit2;
    public float speed = 5.0f;
    public bool isPlayer1;
    private float movement;
    private string[] actionSpace = {"up", "down", "idle"};
    private string[] ballConditions = {"safe", "unsafe"};
    private Vector2[] ballVelocities = new Vector2[]{new Vector2(-5.0f, -5.0f),
                                                    new Vector2(-5.0f, 5.0f),
                                                    new Vector2(5.0f, -5.0f),
                                                    new Vector2(5.0f, 5.0f),
                                                    new Vector2(0.0f, 0.0f)};
    private float moveReward = 0.0f;
    private float goalConcededReward = -100.0f;
    private float hitReward = 50.0f;
    private List<Tuple<Vector3, Vector3, string, Vector2>> stateSpace = new List<Tuple<Vector3, Vector3, string, Vector2>>();
    private Dictionary<Tuple<Vector3, Vector3, string, Vector2>, Dictionary<string, float>> Q = new Dictionary<Tuple<Vector3, Vector3, string, Vector2>, Dictionary<string, float>>();
    private Dictionary<Tuple<Vector3, Vector3, string, Vector2>, Dictionary<string, float>> policy = new Dictionary<Tuple<Vector3, Vector3, string, Vector2>, Dictionary<string, float>>();
    private Tuple<Vector3, Vector3, string, Vector2> currentState;
    private Tuple<Vector3, Vector3, string, Vector2> previousState;
    private float currentReward = 0.0f;
    private string currentAction;
    private Vector3 startPosition;
    private bool hasHitBall = false;
    private bool hasConcededGoal = false;
    private float currentX;
    private float updateTimer = 0.1f;
    private float currentTime = 0.0f;
    private float screenWidth;
    private float screenHeight;
    private int episodeIndex = 0;
    private float currentBallXVelocity;
    private Ball ballScript;
    private MyGrid grid;
    // Start is called before the first frame update
    void Start()
    {
        ballScript = ball.GetComponent<Ball>();
        grid = GameObject.Find("Plane").GetComponent<MyGrid>();
        grid.Initialize();
        startPosition = transform.position;
        currentX = transform.position.x;
        if(isPlayer1)
        {
            screenWidth = Vector3.Distance(transform.position, GameObject.Find("Player2").transform.position);
        }
        else
        {
            screenWidth = Vector3.Distance(transform.position, GameObject.Find("Player1").transform.position);
            // print(screenWidth);
        }
        screenHeight = Vector3.Distance(upWall.transform.position, downWall.transform.position);
        // print(screenHeight);
        // GetStateSpace();
        // InitializeQ();
        // GetRandomPolicy();
        // LoadPolicy();
        Reset();
    }

    private void GetStateSpace()
    {
        // ballCellSize = ball.GetComponent<Ball>().speed * updateTimer;
        foreach(Node node1 in grid.grid)
        {
            foreach(Node node2 in grid.grid)
            {
                int roundedX = Mathf.RoundToInt(transform.position.x);
                if(roundedX == node2.worldPosition.x)
                {
                    foreach(string condition in ballConditions)
                    {
                        foreach(Vector2 ballVelocity in ballVelocities)
                        {
                            Tuple<Vector3, Vector3, string, Vector2> state = Tuple.Create<Vector3, Vector3, string, Vector2>
                                                                                                        (node1.worldPosition,
                                                                                                        node2.worldPosition, condition,
                                                                                                        ballVelocity);
                            // print(state);
                            stateSpace.Add(state);
                        }
                    }
                }
            }
        }

        // paddleCellSize = speed * updateTimer;
    }

    private void InitializeQ()
    {
        foreach(Tuple<Vector3, Vector3, string, Vector2> state in stateSpace)
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
        foreach(Tuple<Vector3, Vector3, string, Vector2> state in stateSpace)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ball"))
        {
            float ballX = ball.transform.position.x;
            float paddleX = transform.position.x + transform.localScale.x / 2.0f - 0.01f;
            // print(paddleX);
            bool outOfBounds = false;
            if(isPlayer1)
            {
                outOfBounds = ballX < paddleX;
            }
            else
            {
                outOfBounds = ballX > paddleX;
            }
            if(currentBallXVelocity + ballScript.rb.velocity.x <= 0.01f && !outOfBounds)
            {
                print("Hit");
                hasHitBall = true;
            }
            else
            {
                hasConcededGoal = true;
            }

            
            UpdateCycle();
        }
    }

    private Tuple<Vector3, Vector3, string, Vector2> UpdateState()
    {
        // float ballX = 0.0f;
        // if(isPlayer1)
        // {
        //     ballX = Mathf.Abs(transform.position.x + ball.transform.position.x) / ballCellSize;
        // }
        // else
        // {
        //     ballX = (transform.position.x - ball.transform.position.x) / ballCellSize;
        // }
        // float ballYCell = (upWall.transform.position.y - ball.transform.position.y) / ballCellSize;
        // Tuple<int, int> ballCell = Tuple.Create<int, int>((int)ballX, (int)ballYCell);
        // // print((int)ballX + ", " + (int)ballY);

        // float paddleY = (upWall.transform.position.y - transform.position.y) / paddleCellSize;
        // int paddleCell = (int)paddleY;

        // print(paddleCell);
        Node ballNode = grid.GetNode(ball.transform.position.x, ball.transform.position.y);
        Node paddleNode = grid.GetNode(transform.position.x, transform.position.y);
        Vector3 ballCell = ballNode.worldPosition;
        Vector3 paddleCell = paddleNode.worldPosition;
        Vector2 ballVelocity = new Vector2(Mathf.Floor(ballScript.rb.velocity.x), Mathf.Floor(ballScript.rb.velocity.y));
        // print(ballVelocity);

        float limitUp = upWall.transform.position.y - yLimit1.transform.position.y;
        float limitDown = upWall.transform.position.y - yLimit2.transform.position.y;
        float ballY = (upWall.transform.position.y - ball.transform.position.y);
        string discretizedAngle = "";

        if((ballY >= limitUp && ballY <= limitDown) || hasHitBall)
        {
            discretizedAngle = "safe";
        }
        else
        {
            discretizedAngle = "unsafe";
        }

        Tuple<Vector3, Vector3, string, Vector2> newState = Tuple.Create<Vector3, Vector3, string, Vector2>
                                                                (ballCell, paddleCell, discretizedAngle, ballVelocity);
        return newState;
    }

    private float GetReward()
    {
        if(hasConcededGoal)
        {
            // print(-(goalConcededReward * HeuristicReward()));
            return goalConcededReward;
        }
        else if(hasHitBall)
        {
            hasHitBall = false;
            return hitReward;
        }
        else if(currentState.Item3 == "safe")
        {
            return 0.0f;
        }
        else
        {
            return -HeuristicReward();
        }
    }

    private float HeuristicReward()
    {
        // 
        // float ballY = upWall.transform.position.y - ball.transform.position.y;
        // float limitUp = upWall.transform.position.y - yLimit1.transform.position.y;
        // float limitDown = upWall.transform.position.y - yLimit2.transform.position.y;
        // float reward = 0.0f;
        // bool isUp = false;
        // bool isDown = false;
        // if(ballY < limitUp)
        // {
        //     isUp = true;
        // }
        // if(ballY > limitDown)
        // {
        //     isDown = true;
        // }

        // if(isUp)
        // {
        //     reward = ballY - limitUp;
        // }
        // else if(isDown)
        // {
        //     reward = limitDown - ballY;
        // }
        // else
        // {
        //     // print("Aligned");
        //     reward = -Mathf.Abs(limitUp - ballY);
        // }
        // return Vector3.Distance(transform.position, ball.transform.position);
        float ballY = upWall.transform.position.y - ball.transform.position.y;
        float paddleY = upWall.transform.position.y - transform.position.y;
        return Mathf.Abs(paddleY - ballY);
    }

    public void ConcededGoal()
    {
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

    private float GetMaxQInState(Tuple<Vector3, Vector3, string, Vector2> state)
    {
        try
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
        catch(Exception)
        {
            Dictionary<string, float> actionsInState = new Dictionary<string, float>();
            foreach(string action in actionSpace)
            {
                actionsInState.Add(action, 0.0f);
            }
            Q.Add(state, actionsInState);

            int n_actions = actionSpace.Length;
            Dictionary<string, float> actionsProbs = new Dictionary<string, float>();
            foreach(string action in actionSpace)
            {
                actionsProbs.Add(action, 1.0f / n_actions);
            }
            try
            {
                policy.Add(state, actionsProbs);
            }
            catch(Exception){}
            return GetMaxQInState(state);
        }
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
        // print(ball.transform.position.x.ToString("F1"));
        currentTime += Time.deltaTime;
        if(currentTime > updateTimer)
        {
            // Node node = grid.GetNode(ball.transform.position.x, ball.transform.position.y);
            // print(node.worldPosition);
            currentBallXVelocity = ballScript.rb.velocity.x;
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
        rb.velocity = Vector2.zero;
        transform.position = startPosition;
        if(episodeIndex % 10 == 0)
        {
            print("Writing policy");
            WritePolicy();
        }
        episodeIndex++;
        // float ballX = 0.0f;
        // if(isPlayer1)
        // {
        //     ballX = Mathf.Abs(transform.position.x + ball.transform.position.x) / ballCellSize;
        // }
        // else
        // {
        //     ballX = (transform.position.x - ball.transform.position.x) / ballCellSize;
        // }
        // float ballY = (upWall.transform.position.y - ball.transform.position.y) / ballCellSize;
        // Tuple<int, int> ballCell = Tuple.Create<int, int>((int)ballX, (int)ballY);

        // float paddleY = (upWall.transform.position.y - transform.position.y) / paddleCellSize;
        // int paddleCell = (int)paddleY;
        Node ballNode = grid.GetNode(ball.transform.position.x, ball.transform.position.y);
        Node paddleNode = grid.GetNode(transform.position.x, transform.position.y);
        Vector3 ballCell = ballNode.worldPosition;
        Vector3 paddleCell = paddleNode.worldPosition;
        Vector2 ballVelocity = new Vector2(Mathf.Floor(ballScript.rb.velocity.x), Mathf.Floor(ballScript.rb.velocity.y));
        // print(ballVelocity);
        previousState = Tuple.Create<Vector3, Vector3, string, Vector2>(ballCell, paddleCell, "safe", ballVelocity);
        hasConcededGoal = false;
        hasHitBall = false;
        // isAligned = false;
        currentAction = "up";
        currentX = startPosition.x;
        currentTime = 0.0f;
        UpdateCycle();
    }

    private void WritePolicy()
    {
        string path = "Assets/Resources/policy" + (isPlayer1 ? "1.txt" : "2.txt");
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path);
        foreach(Tuple<Vector3, Vector3, string, Vector2> state in policy.Keys)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string line = state.ToString() + "{";
            stringBuilder.Append(line);
            Dictionary<string, float> actionProbs = policy[state];
            foreach(string action in actionProbs.Keys)
            {
                float actionProb = actionProbs[action];
                stringBuilder.Append(action+":"+actionProb.ToString() + ",");
            }
            stringBuilder.Append("}");
            writer.WriteLine(stringBuilder.ToString());
        }
        writer.Close();
        //Re-import the file to update the reference in the editor
        // AssetDatabase.ImportAsset(path); 
        // TextAsset asset = (TextAsset)Resources.Load("policy" + (isPlayer1 ? "1" : "2"));
        //Print the text from the file
        // Debug.Log(asset.text);
    }

    private void LoadPolicy()
    {
        policy = new Dictionary<Tuple<Vector3, Vector3, string, Vector2>, Dictionary<string, float>>();
        string path = "Assets/Resources/policy" + (isPlayer1 ? "1.txt" : "2.txt");
        AssetDatabase.ImportAsset(path); 
        TextAsset asset = (TextAsset)Resources.Load("policy" + (isPlayer1 ? "1" : "2"));
        string policyString = asset.text;
        List<string> policyChunks = new List<string>(policyString.Split('\n'));
        policyChunks.Remove(policyChunks[policyChunks.Count - 1]);
        foreach(string policyChunk in policyChunks)
        {
            string[] stateAndActions = policyChunk.Split('{');
            string stateString = stateAndActions[0];
            stateString = stateString.Substring(1, stateString.Length - 2); //skipping the 2 parentheses
            string actionsString = stateAndActions[1];
            actionsString = actionsString.Substring(0, actionsString.Length - 2); //skipping the closing curly brackets and the last comma
            string[] stateComponents = stateString.Split(',');
            string[] actionComponents = actionsString.Split(',');
            Vector3 ballCell = Vector3.zero;
            Vector3 paddleCell = Vector3.zero;
            string discretizedAngle = "";
            Vector2 ballVelocity = Vector2.zero;
            int i = 0;
            foreach(string stateComponent in stateComponents)
            {
                // print(stateComponent.Trim());
                string component = stateComponent.Trim();
                try
                {
                    float value = 0.0f;
                    if(component.Contains('('))
                    {
                        value = float.Parse(component.Substring(1, component.Length - 1));
                    }
                    else if(component.Contains(')'))
                    {
                        value = float.Parse(component.Substring(0, component.Length - 1));
                    }
                    else
                    {
                        value = float.Parse(component);
                    }
                    value /= 10.0f;
                    if(i < 3)
                    {
                        switch(i)
                        {
                            case 0:
                                ballCell.x = value;
                                break;
                            case 1:
                                ballCell.y = value;
                                break;
                            case 2:
                                ballCell.z = value;
                                break;
                        }
                    }
                    else if(i >= 3 && i < 6)
                    {
                        switch(i)
                        {
                            case 3:
                                paddleCell.x = value;
                                break;
                            case 4:
                                paddleCell.y = value;
                                break;
                            case 5:
                                paddleCell.z = value;
                                break;    
                        }
                    }
                    else if(i >= 7 && i < 9)
                    {
                        switch(i)
                        {
                            case 7:
                                ballVelocity.x = value;
                                break;
                            case 8:
                                ballVelocity.y = value;
                                break;
                        }
                    }
                    print(value);
                    i++;
                } catch(Exception)
                {
                    if(i == 6)
                    {
                        discretizedAngle = stateComponent;
                    }
                    i++;
                    // print(stateComponent);
                }
            }
            Dictionary<string, float> actions = new Dictionary<string, float>();
            for(int j = 0; j < 2 * actionSpace.Length; j+= 2)
            {
                string actionComponent = actionComponents[j];
                // print(actionComponent);
                string action = actionComponent.Split(':')[0];
                float actionProb = (float)double.Parse("0,"+actionComponents[j + 1], System.Globalization.NumberStyles.AllowDecimalPoint);
                // print(actionProb);
                actions.Add(action, actionProb);
            }
            Tuple<Vector3, Vector3, string, Vector2> state = Tuple.Create<Vector3, Vector3, string, Vector2>
                                                                        (ballCell, paddleCell, discretizedAngle, ballVelocity);
            try
            {
                policy.Add(state, actions);
            }
            catch(Exception)
            {
                continue;
            }
        }
    }
}
