using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{

    [Header("Grid Options")]
    private int width = 3;
    private int height = 3;
    [SerializeField] private int cols;
    [SerializeField] private int rows;
    [Space]
    [SerializeField] private bool showGrid = true;
    private Grid grid;

    public void MakeGrid()
    {
        grid = new Grid(width, height, cols, rows);
    }

    public void DeleteGrid()
    {
        grid = null;
    }

    public Grid GetGrid()
    {
        return grid;
    }

    private void OnValidate()
    {
        MakeGrid();
    }
    void OnDrawGizmos()
    {
        if (showGrid)
        {
            if (grid != null)
            {
                Gizmos.color = Color.white;

                for (int x = 0; x < grid.GetCols(); x++)
                {
                    for (int y = 0; y < grid.GetRows(); y++)
                    {
                        Gizmos.DrawLine(grid.DrawGrid(x, y), grid.DrawGrid(x, y + 1));
                        Gizmos.DrawLine(grid.DrawGrid(x, y), grid.DrawGrid(x + 1, y));
                    }
                }

                Gizmos.DrawLine(grid.DrawGrid(0, rows), grid.DrawGrid(cols, rows));
                Gizmos.DrawLine(grid.DrawGrid(cols, 0), grid.DrawGrid(cols, rows));
            }
        }
    }
}
