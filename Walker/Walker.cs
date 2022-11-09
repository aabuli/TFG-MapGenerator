using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Walker : MonoBehaviour
{
    private Grid grid;

    private bool stick;
    private bool moving;
    private bool colored;
    private int zoneNumber = 0;
    private int rnd;
    private int[] dirX = new int[4] { 1, 0, -1, 0 };
    private int[] dirY = new int[4] { 0, 1, 0, -1 };
    private float speed = .01f;

    void Start()
    {
        grid = FindObjectOfType<GridGenerator>().GetGrid();
    }

    private void Update()
    {
        if (!stick)
        {
            StartCoroutine(Move());
        }
        if (stick && !colored)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            colored = true;
        }
    }

    private IEnumerator Move()
    {
        if (!moving)
        {
            moving = true;
            rnd = Random.Range(0, dirX.Length);

            Vector3 newDir = grid.GetWorldPosition(
                    (int)grid.GetGridPosition(transform.position.x, transform.position.y).x + dirX[rnd],
                    (int)grid.GetGridPosition(transform.position.x, transform.position.y).y + dirY[rnd]);

            if (MoveIsValid(newDir))
                transform.position = newDir;

            yield return new WaitForSeconds(speed);

            moving = false;
        }
    }
    private bool MoveIsValid(Vector3 _newDir)
    {

        if (grid.GetGridPosition(_newDir.x, _newDir.y).x < 0 ||
            grid.GetGridPosition(_newDir.x, _newDir.y).x >= grid.GetCols() ||
            grid.GetGridPosition(_newDir.x, _newDir.y).y < 0 ||
            grid.GetGridPosition(_newDir.x, _newDir.y).y >= grid.GetRows())
            return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, _newDir - transform.position, Vector3.Distance(transform.position, _newDir));

        if (hit.collider == null)
        {
            return true;
        }
        else if (hit.collider.tag != "goal")
        {
            return true;
        }
        else
        {
            stick = true;
            transform.tag = "goal";
            return false;
        }

    }

    public void SetWalkerStick(bool _stick)
    {
        this.stick = _stick;
    }
    public bool IsSticked()
    {
        return stick;
    }

    public int GetZoneNumber()
    {
        return zoneNumber;
    }
    public void SetZoneNumber(int num)
    {
        zoneNumber = num;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && gameObject.activeSelf)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }
}
