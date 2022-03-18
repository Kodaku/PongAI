using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private int m_row;
    private int m_column;

    public Cell(int row, int column)
    {
        m_row = row;
        m_column = column;
    }

    public int row
    {
        get { return m_row; }
        set { m_row = value; }
    }

    public int column
    {
        get { return m_column; }
        set { m_column = value; }
    }
}
