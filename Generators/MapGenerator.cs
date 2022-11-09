using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(GridGenerator))]
[RequireComponent(typeof(ZonesGenerator))]
[RequireComponent(typeof(RoomGenerator))]
[RequireComponent(typeof(RouteGenerator))]
[RequireComponent(typeof(SpecialRoomsGenerator))]
public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject walker;
    GameObject walkers;

    [Header("Generators")]
    private Grid grid;
    private ZonesGenerator zoneGen;

    [Space]
    [SerializeField] private int numberOfWalkers = 200;

    [Header("List")]
    private List<GameObject> nodes = new List<GameObject>();

    [Header("Bools")]
    private bool allWalkers;
    private bool allSticked;
    private bool working;

    public void StartGeneration()
    {
        grid = GetComponent<GridGenerator>().GetGrid();
        zoneGen = GetComponent<ZonesGenerator>();

        Reset();

        walkers = new GameObject();
        walkers.transform.parent = transform;
        walkers.name = "Walkers";

        Vector3 goalPos = grid.GetWorldPosition(grid.GetCols() / 2, grid.GetRows() / 2);
        SetGoal(goalPos.x, goalPos.y);

        SpawnWalkers();
    }

    private void Update()
    {
        if (allWalkers && !allSticked)
        {
            AreSticked();
        }
        if (allSticked && !working)
        {
            working = true;
            zoneGen.GenerateZones();
        }

    }

    public void SetGoal(float _x, float _y)
    {
        GameObject goal = Instantiate(walker, new Vector3(_x, _y), Quaternion.identity, walkers.transform);
        goal.GetComponent<Walker>().SetWalkerStick(true);
        goal.transform.tag = "goal";
        nodes.Add(goal);
    }

    public void SpawnWalkers()
    {
        for (int i = 0; i < numberOfWalkers; i++)
        {
            int rnd = Random.Range(0, 4);
            int x = 0;
            int y = 0;

            switch (rnd)
            {
                case 0:
                    x = Random.Range(0, grid.GetCols());
                    y = 0;
                    break;

                case 1:
                    x = Random.Range(0, grid.GetCols());
                    y = grid.GetRows() - 1;
                    break;

                case 2:
                    x = 0;
                    y = Random.Range(0, grid.GetRows());
                    break;

                case 3:
                    x = grid.GetCols() - 1;
                    y = Random.Range(0, grid.GetRows());
                    break;
            }

            nodes.Add(Instantiate(walker, grid.GetWorldPosition(x, y), Quaternion.identity, walkers.transform));
        }
        allWalkers = true;
    }

    void AreSticked()
    {
        if (!working)
        {
            working = true;
            int stickNumber = 0;

            foreach (GameObject walker in nodes)
            {
                if (walker.GetComponent<Walker>().IsSticked())
                {
                    stickNumber++;
                }
            }
            working = false;
            if (stickNumber == numberOfWalkers + 1) allSticked = true;
        }
    }

    public List<GameObject> GetNodeList()
    {
        return nodes;
    }

    public void AddNodeList(GameObject _node)
    {
        nodes.Add(_node);
    }

    public void RemoveNodeList(GameObject _node)
    {
        nodes.Remove(_node);
    }

    public GameObject GetWalkerPrefab()
    {
        return walker;
    }

    public void Reset()
    {
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }

        nodes.Clear();
        allSticked = false;
        allWalkers = false;
        working = false;
    }

}
