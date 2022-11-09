using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(GridGenerator))]
[RequireComponent(typeof(MapGenerator))]
[RequireComponent(typeof(RoomGenerator))]
[ExecuteInEditMode]
public class ZonesGenerator : MonoBehaviour
{
    private Grid grid;
    private RoomGenerator roomGen;
    public Sprite pointerIcon;
    private int[,] zoneGrid;

    [Space]
    [Range(1, 6)] public int numberOfZones = 1;
    private int oldMark = 1;
    private int newMark = 9;

    [Header("List")]
    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> pointersList = new List<GameObject>();
    private List<GameObject> ListZone1 = new List<GameObject>();
    private List<GameObject> ListZone2 = new List<GameObject>();
    private List<GameObject> ListZone3 = new List<GameObject>();
    private List<GameObject> ListZone4 = new List<GameObject>();
    private List<GameObject> ListZone5 = new List<GameObject>();
    private List<GameObject> ListZone6 = new List<GameObject>();

    [Space]
    [SerializeField]
    private Color[] zoneColors = new Color[6]{
        Color.green,
        Color.cyan,
        Color.yellow,
        Color.gray,
        Color.magenta,
        Color.red
    };

    [Space]
    [SerializeField][Range(0f, 1f)] private float zoneBackgroundColorAlpha = 0.2f;

    [Header("Bools")]
    private bool pointersSpawned;

    public void GenerateZones()
    {
        Reset();
        grid = FindObjectOfType<GridGenerator>().GetGrid();
        nodes = FindObjectOfType<MapGenerator>().GetNodeList();
        roomGen = FindObjectOfType<RoomGenerator>();

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].GetComponent<SpriteRenderer>().color = Color.white;
        }

        // 1.- Add the pointers the first time
        if (!pointersSpawned)
        {
            pointersList = AddPointers();
            pointersSpawned = true;
        }

        // 2.- Reajust all pointers (K-means)
        for (int i = 0; i < 5; i++)
        {
            AssignZones();
            ColorZones();
            AjustPointers();
        }

        // 3.- Fill Zones list
        MakeZoneLists();

        // 4.- Reassign incorrect nodes
        UnassignWrongNodes();
        CorrectWrongNodes();

        // 5.- Clean and hide pointers and walkers
        HideWalkersAndPointers();

        roomGen.GenerateRooms();
    }

    private List<GameObject> AddPointers()
    {
        List<GameObject> pointersList = new List<GameObject>();
        GameObject pointers = new GameObject();
        pointers.transform.parent = transform;
        pointers.name = "Pointers";

        for (int i = 0; i < numberOfZones; i++)
        {
            GameObject pointer = new GameObject();
            pointer.transform.parent = pointers.transform;
            pointer.AddComponent<Pointers>();
            pointer.GetComponent<Pointers>().SetPointerNumber(i + 1);
            pointer.name = "Pointer " + pointer.GetComponent<Pointers>().GetPointerNumber();
            pointer.AddComponent<SpriteRenderer>();
            pointer.GetComponent<SpriteRenderer>().sprite = pointerIcon;
            pointer.GetComponent<SpriteRenderer>().color = Color.blue;
            pointer.GetComponent<SpriteRenderer>().sortingOrder = 1;
            pointer.transform.localScale = new Vector3(40, 40, 0);
            pointer.transform.position = new Vector3(
                grid.GetWorldPosition(Random.Range(1, grid.GetCols()), Random.Range(1, grid.GetRows())).x,
                grid.GetWorldPosition(Random.Range(1, grid.GetCols()), Random.Range(1, grid.GetRows())).y);
            pointersList.Add(pointer);
        }

        return pointersList;
    }

    private void AjustPointers()
    {
        for (int i = 0; i < pointersList.Count; i++)
        {
            float distancesX = 0;
            float distancesY = 0;
            int numberNodes = 0;

            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].GetComponent<Walker>().GetZoneNumber() == i + 1)
                {
                    distancesX += nodes[j].GetComponent<Walker>().transform.position.x;
                    distancesY += nodes[j].GetComponent<Walker>().transform.position.y;
                    numberNodes++;
                }
            }

            if (distancesX == 0 || distancesY == 0 || numberNodes == 0)
            {
                GenerateZones();
                break;
            }
            else
            {
                pointersList[i].transform.position = new Vector3(distancesX / numberNodes, distancesY / numberNodes); // ERROR!
            }

        }
    }

    private void AssignZones()
    {
        bool[] _allPointersHaveChilds = new bool[pointersList.Count];
        // Repeat the assignation of zones until all the pointer have at least one child
        do
        {
            _allPointersHaveChilds = new bool[pointersList.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                int nearPointer = 0;
                float maxDistance = 999999f;
                for (int j = 0; j < pointersList.Count; j++)
                {
                    //Calculate Distance to each pointer
                    float distance = Vector3.Distance(nodes[i].transform.position, pointersList[j].transform.position);
                    if (distance < maxDistance)
                    {
                        maxDistance = distance;
                        nearPointer = j;
                    }
                }
                nodes[i].GetComponent<Walker>().SetZoneNumber(pointersList[nearPointer].GetComponent<Pointers>().GetPointerNumber());
                _allPointersHaveChilds[nearPointer] = true;
            }

            if (!AreAllElementsTrue(_allPointersHaveChilds))
            {
                Reset();
                pointersList = AddPointers();
                pointersSpawned = true;
            }


        } while (!AreAllElementsTrue(_allPointersHaveChilds));

    }

    private void ColorZones()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            switch (nodes[i].GetComponent<Walker>().GetZoneNumber())
            {
                case 1:
                    nodes[i].GetComponent<SpriteRenderer>().color = zoneColors[0];
                    break;
                case 2:
                    nodes[i].GetComponent<SpriteRenderer>().color = zoneColors[1];
                    break;
                case 3:
                    nodes[i].GetComponent<SpriteRenderer>().color = zoneColors[2];
                    break;
                case 4:
                    nodes[i].GetComponent<SpriteRenderer>().color = zoneColors[3];
                    break;
                case 5:
                    nodes[i].GetComponent<SpriteRenderer>().color = zoneColors[4];
                    break;
                case 6:
                    nodes[i].GetComponent<SpriteRenderer>().color = zoneColors[5];
                    break;
            }
        }
    }

    private List<GameObject> GetNearestNodeToPointer(List<GameObject> list, GameObject pointer)
    {
        return list.OrderBy(
            x => Vector2.Distance(x.transform.position, pointer.transform.position)
            ).ToList();
    }

    private void MakeZoneLists()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            switch (nodes[i].GetComponent<Walker>().GetZoneNumber())
            {
                case 1:
                    ListZone1.Add(nodes[i]);
                    break;
                case 2:
                    ListZone2.Add(nodes[i]);
                    break;
                case 3:
                    ListZone3.Add(nodes[i]);
                    break;
                case 4:
                    ListZone4.Add(nodes[i]);
                    break;
                case 5:
                    ListZone5.Add(nodes[i]);
                    break;
                case 6:
                    ListZone6.Add(nodes[i]);
                    break;
            }
        }
    }

    private void UnassignWrongNodes()
    {
        for (int i = 0; i < numberOfZones; i++)
        {
            zoneGrid = new int[grid.GetCols(), grid.GetRows()];

            #region Map each zone to an int array
            switch (i)
            {
                case 0:
                    foreach (GameObject node in ListZone1)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        zoneGrid[(int)nodePos.x, (int)nodePos.y] = oldMark;
                    }
                    break;
                case 1:
                    foreach (GameObject node in ListZone2)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        zoneGrid[(int)nodePos.x, (int)nodePos.y] = oldMark;
                    }
                    break;
                case 2:
                    foreach (GameObject node in ListZone3)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        zoneGrid[(int)nodePos.x, (int)nodePos.y] = oldMark;
                    }
                    break;
                case 3:
                    foreach (GameObject node in ListZone4)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        zoneGrid[(int)nodePos.x, (int)nodePos.y] = oldMark;
                    }
                    break;
                case 4:
                    foreach (GameObject node in ListZone5)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        zoneGrid[(int)nodePos.x, (int)nodePos.y] = oldMark;
                    }
                    break;
                case 5:
                    foreach (GameObject node in ListZone6)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        zoneGrid[(int)nodePos.x, (int)nodePos.y] = oldMark;
                    }
                    break;
            }
            #endregion

            GameObject nearestNode;

            #region Do the FloodFill to each ZoneList
            switch (i)
            {
                case 0:
                    nearestNode = GetNearestNodeToPointer(ListZone1, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, oldMark, newMark);
                    break;
                case 1:
                    nearestNode = GetNearestNodeToPointer(ListZone2, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, oldMark, newMark);
                    break;
                case 2:
                    nearestNode = GetNearestNodeToPointer(ListZone3, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, oldMark, newMark);
                    break;
                case 3:
                    nearestNode = GetNearestNodeToPointer(ListZone4, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, oldMark, newMark);
                    break;
                case 4:
                    nearestNode = GetNearestNodeToPointer(ListZone5, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, oldMark, newMark);
                    break;
                case 5:
                    nearestNode = GetNearestNodeToPointer(ListZone6, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, oldMark, newMark);
                    break;
            }
            #endregion

            #region Unassign "unpainted" nodes (floating nodes)
            switch (i)
            {
                case 0:
                    foreach (GameObject node in ListZone1)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        if (zoneGrid[(int)nodePos.x, (int)nodePos.y] == oldMark)
                        {
                            node.GetComponent<Walker>().SetZoneNumber(newMark);
                            node.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    break;
                case 1:
                    foreach (GameObject node in ListZone2)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        if (zoneGrid[(int)nodePos.x, (int)nodePos.y] == oldMark)
                        {
                            node.GetComponent<Walker>().SetZoneNumber(newMark);
                            node.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    break;
                case 2:
                    foreach (GameObject node in ListZone3)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        if (zoneGrid[(int)nodePos.x, (int)nodePos.y] == oldMark)
                        {
                            node.GetComponent<Walker>().SetZoneNumber(newMark);
                            node.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    break;
                case 3:
                    foreach (GameObject node in ListZone4)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        if (zoneGrid[(int)nodePos.x, (int)nodePos.y] == oldMark)
                        {
                            node.GetComponent<Walker>().SetZoneNumber(newMark);
                            node.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    break;
                case 4:
                    foreach (GameObject node in ListZone5)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        if (zoneGrid[(int)nodePos.x, (int)nodePos.y] == oldMark)
                        {
                            node.GetComponent<Walker>().SetZoneNumber(newMark);
                            node.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    break;
                case 5:
                    foreach (GameObject node in ListZone6)
                    {
                        Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                        if (zoneGrid[(int)nodePos.x, (int)nodePos.y] == oldMark)
                        {
                            node.GetComponent<Walker>().SetZoneNumber(newMark);
                            node.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    break;
            }
            #endregion
        }

    }

    private void CorrectWrongNodes()
    {
        for (int i = 0; i < numberOfZones; i++)
        {

            //New Grid each loop with the wrongnodes and each zone
            zoneGrid = new int[grid.GetCols(), grid.GetRows()];

            //Write the wrong nodes in the grid
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].GetComponent<Walker>().GetZoneNumber() == newMark)
                {
                    Vector3 nodePos = grid.GetGridPosition(nodes[j].transform.position.x, nodes[j].transform.position.y);
                    zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                }
            }

            #region Write each node to an int array
            switch (i)
            {
                case 0:
                    foreach (GameObject node in nodes)
                    {
                        if (node.GetComponent<Walker>().GetZoneNumber() == 1)
                        {
                            Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                            zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                        }
                    }
                    break;
                case 1:
                    foreach (GameObject node in nodes)
                    {
                        if (node.GetComponent<Walker>().GetZoneNumber() == 2)
                        {
                            Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                            zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                        }
                    }
                    break;
                case 2:
                    foreach (GameObject node in nodes)
                    {
                        if (node.GetComponent<Walker>().GetZoneNumber() == 3)
                        {
                            Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                            zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                        }
                    }
                    break;
                case 3:
                    foreach (GameObject node in nodes)
                    {
                        if (node.GetComponent<Walker>().GetZoneNumber() == 4)
                        {
                            Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                            zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                        }
                    }
                    break;
                case 4:
                    foreach (GameObject node in nodes)
                    {
                        if (node.GetComponent<Walker>().GetZoneNumber() == 5)
                        {
                            Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                            zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                        }
                    }
                    break;
                case 5:
                    foreach (GameObject node in nodes)
                    {
                        if (node.GetComponent<Walker>().GetZoneNumber() == 6)
                        {
                            Vector3 nodePos = grid.GetGridPosition(node.transform.position.x, node.transform.position.y);
                            zoneGrid[(int)nodePos.x, (int)nodePos.y] = newMark;
                        }
                    }
                    break;
            }
            #endregion

            #region FloodFill the grid to reassign the wrong nodes
            GameObject nearestNode;
            switch (i)
            {
                case 0:
                    nearestNode = GetNearestNodeToPointer(ListZone1, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, newMark, 1);
                    break;
                case 1:
                    nearestNode = GetNearestNodeToPointer(ListZone2, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, newMark, 2);
                    break;
                case 2:
                    nearestNode = GetNearestNodeToPointer(ListZone3, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, newMark, 3);
                    break;
                case 3:
                    nearestNode = GetNearestNodeToPointer(ListZone4, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, newMark, 4);
                    break;
                case 4:
                    nearestNode = GetNearestNodeToPointer(ListZone5, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, newMark, 5);
                    break;
                case 5:
                    nearestNode = GetNearestNodeToPointer(ListZone6, pointersList[i])[0];
                    FloodFill(
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).x,
                        (int)grid.GetGridPosition(nearestNode.transform.position.x, nearestNode.transform.position.y).y,
                        zoneGrid, newMark, 6);
                    break;
            }
            #endregion

            #region Reassign the zonenumber to each node.
            for (int x = 0; x < zoneGrid.GetLength(0); x++)
            {
                for (int y = 0; y < zoneGrid.GetLength(1); y++)
                {

                    if (zoneGrid[x, y] == i + 1)
                    {
                        foreach (GameObject node in nodes)
                        {
                            var nodePos = grid.GetGridPosition((int)node.transform.position.x, (int)node.transform.position.y);
                            if (new Vector3((int)nodePos.x, (int)nodePos.y) == new Vector3(x, y))
                            {
                                node.GetComponent<Walker>().SetZoneNumber(i + 1);
                            }

                        }
                    }
                }
            }
            #endregion
        }

        ColorZones();
    }

    private void FloodFill(int x, int y, int[,] arr, int OldInt, int newInt)
    {
        if (x >= 0 && x < grid.GetCols() && y >= 0 && y < grid.GetRows())
        {
            //check if the node has the oldInt
            if (arr[x, y] == OldInt)
            {
                //Change the value int of the first node
                arr[x, y] = newInt;

                //Repeat for each neighbour of that node
                FloodFill(x + 1, y, arr, OldInt, newInt);
                FloodFill(x - 1, y, arr, OldInt, newInt);
                FloodFill(x, y + 1, arr, OldInt, newInt);
                FloodFill(x, y - 1, arr, OldInt, newInt);
            }
        }
    }

    private int NearestZone(int x, int y, int[,] arr, int prevInt)
    {
        int nearest = 0;
        if (x >= 0 && x < grid.GetCols() && y >= 0 && y < grid.GetRows())
        {
            //check if the node has the oldColor
            if (arr[x, y] == prevInt)
            {
                //Repeat for each neighbour of that node
                nearest = NearestZone(x + 1, y, arr, prevInt);
                if (nearest != 0) return nearest;
                nearest = NearestZone(x - 1, y, arr, prevInt);
                if (nearest != 0) return nearest;
                nearest = NearestZone(x, y + 1, arr, prevInt);
                if (nearest != 0) return nearest;
                nearest = NearestZone(x, y - 1, arr, prevInt);
                if (nearest != 0) return nearest;
            }
            else if (arr[x, y] != 0)
            {
                nearest = arr[x, y];
            }
        }
        return nearest;
    }

    public List<GameObject> GetNodeListWithZones()
    {
        return nodes;
    }

    public int GetNumberofZones()
    {
        return numberOfZones;
    }

    bool AreAllElementsTrue(bool[] _boolArray)
    {
        for (int i = 0; i < _boolArray.Length; i++)
        {
            if (_boolArray[i] != true)
            {
                return false;
            }
        }

        return true;
    }

    private void HideWalkersAndPointers()
    {
        int zones = transform.childCount;

        for (int i = zones - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name == "Walkers" || transform.GetChild(i).name == "Pointers")
            {
                transform.GetChild(i).gameObject.SetActive(false);
                transform.GetChild(i).gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }
    }

    public Color[] GetZoneColorArray()
    {
        return zoneColors;
    }

    public float GetRoomAlphaColor()
    {
        return zoneBackgroundColorAlpha;
    }
    public void Reset()
    {
        ListZone1.Clear();
        ListZone2.Clear();
        ListZone3.Clear();
        ListZone4.Clear();
        ListZone5.Clear();
        ListZone6.Clear();
        pointersList.Clear();
        pointersSpawned = false;

        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name != "Walkers")
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }

}
