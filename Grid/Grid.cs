using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private int cols;
    private int rows;

    public Grid(int width, int height, int cols, int rows)
    {
        this.width = width;
        this.height = height;
        this.cols = cols;
        this.rows = rows;
    }

    public Vector3 DrawGrid(int x, int y)
    {
        return new Vector3(x, y) * (width * height);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * (width * height) + new Vector3((width * height),
            (width * height)) * .5f;
    }

    public Vector3 GetGridPosition(float x, float y)
    {
        return new Vector3(x, y) / (width * height);
    }

    public int GetCols()
    {
        return cols;
    }

    public int GetRows()
    {
        return rows;
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetWidth()
    {
        return width;
    }

}
