using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid
{
    private Cell[,] grid;
    private int m_rows;
    private int m_columns;

    public MyGrid(int rows, int columns)
    {
        m_rows = rows;
        m_columns = columns;
        PrepareGrid();
    }

    private void PrepareGrid()
    {
        grid = new Cell[m_rows, m_columns];
        for(int row = 0; row < m_rows; row++)
        {
            for(int column = 0; column < m_columns; column++)
            {
                Cell cell = new Cell(row, column);
                grid[row, column] = cell;
            }
        }
    }

    public Cell CellAt(int row, int column)
    {
        if(row >= 0 && row < m_rows && column >= 0 && column < m_columns)
        {
            return grid[row, column];
        }
        return null;
    }

    public Cell WorldPointToCell(Vector3 worldPoint)
    {
        // Debug.Log(worldPoint);
        int row = Mathf.RoundToInt(worldPoint.y);
        int column = Mathf.RoundToInt(worldPoint.x);
        return grid[row, column];
    }

    public void SetCellAt(int row, int column, Cell cell)
    {
        if(row >= 0 && row < m_rows && column >= 0 && column < m_columns)
        {
            grid[row, column] = cell;
        }
    }

    public int rows
    {
        get { return m_rows; }
        set { m_rows = value; }
    }

    public int columns
    {
        get { return m_columns; }
        set { m_columns = value; }
    }

    
}
