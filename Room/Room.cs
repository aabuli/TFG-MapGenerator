using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Room : MonoBehaviour
{
    [SerializeField] private int roomNumber;
    [Space]
    [SerializeField] private int width = 0;
    [SerializeField] private int height = 0;
    [Space]
    [SerializeField] private List<GameObject> neighbours = new List<GameObject>();
    [SerializeField] private List<GameObject> neighboursOpenDoors = new List<GameObject>();

    private bool beforeBoos = false;
    private Grid grid = null;
    [SerializeField] private Texture roomIcon = null;

    #region Getters
    public Vector3 GetRoomCenterPos()
    {
        return GetRoomCentrerPos();
    }

    public int GetRoomNumber()
    {
        return roomNumber;
    }

    public List<GameObject> GetNeighboursList()
    {
        return neighbours;
    }

    private Vector3 GetRoomCentrerPos()
    {
        Vector3 centrerPos = new Vector3(0, 0, 0);

        int totalChilds = 0;
        foreach (Transform roomCell in transform)
        {
            if (roomCell.tag == "RoomCell")
            {
                totalChilds++;
                centrerPos += roomCell.position;
            }
        }

        return (centrerPos / totalChilds) + new Vector3(4.5f, 4.5f);
    }

    public int GetHeight()
    {
        if (height == 0)
            CalculateWidthAndHeight();

        return height;
    }

    public int GetWidth()
    {
        if (width == 0)
            CalculateWidthAndHeight();

        return width;
    }

    public int GetNumberOfDoors()
    {
        int numOfDoors = 0;
        foreach (Transform roomCell in transform)
        {
            if (roomCell.tag == "RoomCell")
                numOfDoors += roomCell.GetComponent<RoomCell>().GetNumberOfDoorsOpen();
        }
        return numOfDoors;
    }

    public List<GameObject> GetNeighboursOpenDoors()
    {
        return neighboursOpenDoors;
    }
    #endregion

    #region Setters
    public void SetRoomNumber(int _num)
    {
        roomNumber = _num;
    }

    public void SetRoomIcon(Texture _icon)
    {
        roomIcon = _icon;
    }

    public void SetIsBeforeBoss(bool _b)
    {
        beforeBoos = _b;
    }
    #endregion

    #region Adders
    public void AddNeighbour(GameObject _neighbour)
    {
        neighbours.Add(_neighbour);
    }

    public void AddNeighboursOpenDoor(GameObject _go)
    {
        neighboursOpenDoors.Add(_go);
    }
    #endregion

    #region Removers
    private void RemoveNeighbour(GameObject _neighbour)
    {
        neighbours.Remove(_neighbour);
    }

    public void RemoveRoom()
    {
        foreach (GameObject neighbour in neighbours)
        {
            neighbour.GetComponent<Room>().RemoveNeighbour(gameObject);

            foreach (Transform roomCell in neighbour.transform)
            {

                for (int i = 0; i < roomCell.GetComponent<RoomCell>().GetNeighboursList().Length; i++)
                {
                    if (roomCell.GetComponent<RoomCell>().GetNeighboursList()[i] == roomNumber)
                    {
                        roomCell.GetComponent<RoomCell>().RemoveNeighbour(i);
                    }
                }
            }
        }

        foreach (Transform roomCell in transform)
        {
            var go = roomCell.GetComponent<RoomCell>().GetWalker();
            if (go != null)
            {
                MapGenerator mapGen = FindObjectOfType<MapGenerator>();
                if (mapGen != null)
                    mapGen.RemoveNodeList(go);

                DestroyImmediate(go);
            }
        }


        DestroyImmediate(gameObject);
    }
    #endregion

    public void CalculateWidthAndHeight()
    {
        height = 0;
        width = 0;

        List<int> vectorList = new List<int>();

        foreach (Transform roomCell in transform)
        {
            if (!vectorList.Contains((int)roomCell.transform.position.y))
            {
                vectorList.Add((int)roomCell.transform.position.y);
                height++;
            }
        }

        vectorList.Clear();

        foreach (Transform roomCell in transform)
        {
            if (!vectorList.Contains((int)roomCell.transform.position.x))
            {
                vectorList.Add((int)roomCell.transform.position.x);
                width++;
            }
        }

    }

    public void ClearNeighboursOpenDoors()
    {
        neighboursOpenDoors.Clear();
    }

    public bool IsBeforeBoss()
    {
        return beforeBoos;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 0, 0.20F);

        if (grid == null)
            grid = FindObjectOfType<MapGenerator>().GetComponent<GridGenerator>().GetGrid();
        else
            foreach (Transform roomCell in transform)
            {
                if (roomCell.name != "Room Icon")
                    Gizmos.DrawCube(grid.GetWorldPosition((int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x,
                    (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y), new Vector3(9, 9, 0));
            }

    }

    private void OnDrawGizmos()
    {
        if (roomIcon != null)
        {
            if (roomIcon.name == "BossIcon" || roomIcon.name == "Key3")
                Gizmos.DrawGUITexture(new Rect(GetRoomCenterPos().x + 5.0f, GetRoomCenterPos().y + 4.5f, -10f, -10f), roomIcon);
            else if (roomIcon.name == "BossAndKey")
                Gizmos.DrawGUITexture(new Rect(GetRoomCenterPos().x + 10.0f, GetRoomCenterPos().y + 4.5f, -20f, -10f), roomIcon);
            else
                Gizmos.DrawGUITexture(new Rect(GetRoomCenterPos().x + 4.5f, GetRoomCenterPos().y + 4.5f, -10f, -10f), roomIcon);
        }
    }
}
