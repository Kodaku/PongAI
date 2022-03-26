using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject worldPrefab;
    public GameObject ballPrefab;
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    private GameObject currentWorld;
    private MyGrid grid;
    private Ball ball;
    private Player player1;
    private Player player2;
    private float updateTimer = 0.1f;
    private float currentUpdateTimer = 0.0f;
    private bool isPlayer1HitPresent = false;
    private bool isPlayer2HitPresent = false;
    private DataManager dataManager = new DataManager();
    private Debugger debugger;
    // Start is called before the first frame update
    void Start()
    {
        grid = new MyGrid(20, 33);

        debugger = GameObject.Find("Debugger").GetComponent<Debugger>();

        DisplayGrid();

        Cell player1Cell = grid.CellAt(16, 2);
        Cell player2Cell = grid.CellAt(16, 31);

        Vector2 player1Position = new Vector2(player1Cell.column, player1Cell.row);
        Vector2 player2Position = new Vector2(player2Cell.column, player2Cell.row);

        GameObject player1Instance = Instantiate(player1Prefab, player1Position, Quaternion.identity);
        GameObject player2Instance = Instantiate(player2Prefab, player2Position, Quaternion.identity);
        player1Instance.name = "Player1";
        player2Instance.name = "Player2";

        player1 = player1Instance.GetComponent<Player>();
        player2 = player2Instance.GetComponent<Player>();

        Cell ballCell = grid.CellAt(10, 16);
        Vector2 ballPosition = new Vector2(ballCell.column, ballCell.row);

        GameObject ballInstance = Instantiate(ballPrefab, ballPosition, Quaternion.identity);

        ball = ballInstance.GetComponent<Ball>();
        ball.Initialize();

        player1.Initialize();
        player2.Initialize();

        player1.LoadData("policy1", "q1", "model1");
        player2.LoadData("policy2", "q2", "model2");

        player1.SetCurrentState(grid, ball);
        player2.SetCurrentState(grid, ball);

        player1.hitBallObservers += OnPlayer1HitBall;
        player2.hitBallObservers += OnPlayer2HitBall;
    }

    private void DisplayGrid()
    {
        currentWorld = Instantiate(worldPrefab, Vector3.zero, Quaternion.identity);
        for(int row = 0; row < grid.rows; row++)
        {
            for(int column = 0; column < grid.columns; column++)
            {
                Cell cell = grid.CellAt(row, column);
                Vector2 cellPosition = new Vector2(cell.column, cell.row);
                GameObject newCell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                newCell.transform.parent = currentWorld.transform;
            }
        }
    }

    private void OnPlayer1HitBall()
    {
        isPlayer1HitPresent = true;
        player1.SetNextState(grid, ball);
        player1.QUpdate(1.0f, false);
        // player1.UpdateModel(1.0f);
        dataManager.IncreaseHitCount(isPlayer1:true);
        isPlayer1HitPresent = false;
    }

    private void OnPlayer2HitBall()
    {
        isPlayer2HitPresent = true;
        player2.SetNextState(grid, ball);
        player2.QUpdate(1.0f, false);
        // player2.UpdateModel(1.0f);
        dataManager.IncreaseHitCount(isPlayer1:false);
        isPlayer2HitPresent = false;
    }

    // Update is called once per frame
    void Update()
    {
        currentUpdateTimer += Time.deltaTime;
        if(currentUpdateTimer >= updateTimer)
        {
            Cell ballCell = grid.WorldPointToCell(ball.transform.position);
            Cell player1Cell = grid.WorldPointToCell(player1.transform.position);
            Cell player2Cell = grid.WorldPointToCell(player2.transform.position);

            player1.SelectAction();
            player2.SelectAction();

            float currentQ1 = player1.GetQ();
            float currentQ2 = player2.GetQ();

            dataManager.UpdateTimer(currentUpdateTimer);
            dataManager.SetMinMaxQValues(currentQ1, isPlayer1:true);
            dataManager.SetMinMaxQValues(currentQ1, isPlayer1:false);
            dataManager.SetQValues(currentQ1, currentQ2);

            debugger.SetColors(currentQ1, dataManager.GetMinQValue(isPlayer1:false), dataManager.GetMaxQValue(isPlayer1:false), isPlayer1:false);
            debugger.SpawnMove(player2, ballCell, isPlayer1:false);

            player1.Move();
            player2.Move();

            if(!isPlayer1HitPresent)
                player1.SetNextState(grid, ball);
            if(!isPlayer2HitPresent)
                player2.SetNextState(grid, ball);

            // player1.UpdateAllTau();
            // player2.UpdateAllTau();

            if((ballCell.column <= player1Cell.column && (ballCell.row < player1Cell.row - 1 || ballCell.row > player1Cell.row + 1)) ||
                (ballCell.column >= player2Cell.column && (ballCell.row < player2Cell.row - 1 || ballCell.row > player2Cell.row + 1)))
            {
                if((ballCell.column <= player1Cell.column && (ballCell.row < player1Cell.row - 1 || ballCell.row > player1Cell.row + 1)))
                {
                    dataManager.Player2Scored();

                    float reward = Mathf.RoundToInt(-100.0f - Vector2.Distance(player1.transform.position, ball.transform.position) * 10.0f);
                    player1.QUpdate(reward, true);
                    // player1.UpdateModel(reward);
                    player2.QUpdate(0.0f, true);
                    // player2.UpdateModel(0.0f);
                }
                else if((ballCell.column >= player2Cell.column && (ballCell.row < player2Cell.row - 1 || ballCell.row > player2Cell.row + 1)))
                {
                    dataManager.Player1Scored();

                    float reward = Mathf.RoundToInt(-100.0f - Vector2.Distance(player2.transform.position, ball.transform.position) * 10.0f);
                    player2.QUpdate(reward, true);
                    // player2.UpdateModel(reward);
                    player1.QUpdate(0.0f, true);
                    // player1.UpdateModel(0.0f);
                }

                Reset();
            }
            else
            {
                player1.QUpdate(0.0f, false);
                // player1.UpdateModel(0.0f);
                player2.QUpdate(0.0f, false);
                // player2.UpdateModel(0.0f);
            }

            // for(int i = 0; i < 50; i++)
            // {
            //     player1.RunSimulation();
            //     player2.RunSimulation();
            // }

            currentUpdateTimer = 0.0f;
        }
    }

    private void Reset()
    {
        player1.SaveData("policy1", "q1", "model1");
        player2.SaveData("policy2", "q2", "model2");

        dataManager.RegisterObservation();
        dataManager.Reset();

        debugger.Reset();

        ball.Reset();

        player1.Reset();
        player2.Reset();

        Destroy(currentWorld);

        DisplayGrid();
    }
}
