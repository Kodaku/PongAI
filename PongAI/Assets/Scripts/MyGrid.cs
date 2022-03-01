using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour
{
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] m_grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    public void Initialize()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter + nodeRadius);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter + nodeRadius);
        CreateGrid();
    }

    void CreateGrid()
    {
        // print("Create m_grid");
        m_grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;
        // print(worldBottomLeft);

        for(int x = 0; x < gridSizeX; x++)
        {
            for(int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                // print(worldPoint);
                m_grid[x, y] = new Node(worldPoint);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 0.0f));

        if(m_grid != null)
        {
            foreach(Node node in m_grid)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

    public Node GetNode(float x, float y)
    {
        x = Mathf.RoundToInt(x);
        y = Mathf.RoundToInt(y);
        // print(x + ", " + y);
        foreach(Node node in m_grid)
        {
            if(node.worldPosition.x == x && node.worldPosition.y == y)
            {
                return node;
            }
        }
        return null;
    }

    public Node[,] grid
    {
        get { return m_grid; }
    }
}
