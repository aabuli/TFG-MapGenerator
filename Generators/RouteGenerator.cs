using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RouteGenerator : MonoBehaviour
{
    [Header("Generators")]
    private Grid grid;
    private ZonesGenerator zoneGen;
    private SpecialRoomsGenerator specialGen;

    [Header("Lists")]
    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> routeOfZones = new List<GameObject>();
    [SerializeField] private List<int> zonesOrder = new List<int>();

    [Header("Bools")]
    [SerializeField] private bool ShowGraph = false;
    [SerializeField] private bool ShowMinRoute = false;

    public void GenerateRoute()
    {
        Reset();
        grid = GetComponent<GridGenerator>().GetGrid();
        zoneGen = GetComponent<ZonesGenerator>();
        specialGen = GetComponent<SpecialRoomsGenerator>();
        GenerateRoomNodes();
        routeOfZones = GenerateZoneRoute();
        MakeZonesOrder();

        specialGen.GenerateSpecialRooms();
    }

    private void GenerateRoomNodes()
    {
        int zones = transform.childCount;

        for (int i = zones - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).tag == "Zones" && transform.GetChild(i).name != "Zone 0")
            {
                nodes = new List<GameObject>();
                int rooms = transform.GetChild(i).childCount;
                if (rooms > 0)
                {
                    for (int j = rooms - 1; j >= 0; j--)
                    {
                        if (transform.GetChild(i).GetChild(j).tag == "Rooms")
                        {

                            if (!nodes.Contains(transform.GetChild(i).GetChild(j).gameObject))
                                nodes.Add(transform.GetChild(i).GetChild(j).gameObject);
                        }
                    }

                    GameObject _firstRoom = transform.GetChild(i).GetComponent<Zone>().GetFirstZoneRoom();

                    transform.GetChild(i).GetComponent<Zone>().SetRouteRoomList(GenerateMinRoute(nodes, _firstRoom));
                    OpenRoomDoors(transform.GetChild(i).GetComponent<Zone>().GetRouteRoomList());
                }
            }
        }
    }

    public List<GameObject> GenerateMinRoute(List<GameObject> _nodes, GameObject _firstRoom = null, GameObject _lastRoom = null)
    {
        List<GameObject> routeOfNodes = new List<GameObject>();
        List<GameObject> unreached = new List<GameObject>();
        List<GameObject> reached = new List<GameObject>();

        for (int i = 0; i < _nodes.Count; i++)
        {
            unreached.Add(_nodes[i].gameObject);
        }

        if (_firstRoom == null)
        {
            reached.Add(unreached[0]);
            unreached.RemoveAt(0);
        }
        else
        {
            reached.Add(unreached.Find(x => x == _firstRoom));
            unreached.RemoveAt(unreached.FindIndex(x => x == _firstRoom));
        }

        while (unreached.Count > 0)
        {
            float record = 10000000;
            int reachedIndex = 0;
            int unreachedIndex = 0;

            for (int i = 0; i < reached.Count; i++)
            {
                for (int j = 0; j < unreached.Count; j++)
                {
                    bool isNeighbour = false;

                    for (int l = 0; l < reached[i].GetComponent<Room>().GetNeighboursList().Count; l++)
                    {
                        if (reached[i].GetComponent<Room>().GetNeighboursList()[l].gameObject.name == unreached[j].name)
                            isNeighbour = true;
                    }

                    if (isNeighbour)
                    {
                        float dis = Vector3.Distance(reached[i].GetComponent<Room>().GetRoomCenterPos(), unreached[j].GetComponent<Room>().GetRoomCenterPos());
                        if (dis < record)
                        {
                            record = dis;
                            reachedIndex = i;
                            unreachedIndex = j;
                        }
                    }
                }
            }

            if (unreached[unreachedIndex] == _lastRoom)
            {
                return routeOfNodes;
            }

            routeOfNodes.Add(reached[reachedIndex]);
            routeOfNodes.Add(unreached[unreachedIndex]);
            reached.Add(unreached[unreachedIndex]);
            unreached.RemoveAt(unreachedIndex);
        }
        return routeOfNodes;
    }

    private List<GameObject> GenerateZoneRoute()
    {
        List<GameObject> _nodes = new List<GameObject>();
        for (int i = 0; i < transform.Find("Pointers").childCount; i++)
        {
            _nodes.Add(transform.Find("Pointers").GetChild(i).gameObject);
        }

        List<GameObject> routeOfNodes = new List<GameObject>();
        List<GameObject> unreached = new List<GameObject>();
        List<GameObject> reached = new List<GameObject>();

        for (int i = 0; i < _nodes.Count; i++)
        {
            unreached.Add(_nodes[i].gameObject);
        }

        float max = 9999999;
        int nearZone = 0;

        for (int i = 0; i < unreached.Count; i++)
        {
            if (Vector3.Distance(unreached[i].transform.position, Vector3.zero) < max)
            {
                max = Vector3.Distance(unreached[i].transform.position, Vector3.zero);
                nearZone = i;
            }
        }

        reached.Add(unreached[nearZone]);
        unreached.RemoveAt(nearZone);

        if (_nodes.Count == 1)
            routeOfNodes.Add(reached[0]);

        while (unreached.Count > 0)
        {
            float record = 0;
            int reachedIndex = 0;
            int unreachedIndex = 0;

            for (int i = 0; i < reached.Count; i++)
            {
                for (int j = 0; j < unreached.Count; j++)
                {
                    bool isNeighbour = false;

                    for (int l = 0; l < reached[i].GetComponent<Pointers>().GetZoneNieghbours().Count; l++)
                    {
                        if ("Pointer " + reached[i].GetComponent<Pointers>().GetZoneNieghbours()[l] == unreached[j].name)
                            isNeighbour = true;
                    }

                    if (isNeighbour)
                    {
                        float dis = Vector3.Distance(reached[i].GetComponent<Pointers>().transform.position, unreached[j].GetComponent<Pointers>().transform.position);
                        if (dis > record)
                        {
                            record = dis;
                            reachedIndex = i;
                            unreachedIndex = j;
                        }
                    }
                }
            }

            routeOfNodes.Add(reached[reachedIndex]);
            routeOfNodes.Add(unreached[unreachedIndex]);
            reached.Add(unreached[unreachedIndex]);
            unreached.RemoveAt(unreachedIndex);
        }
        return routeOfNodes;
    }

    void OpenRoomDoors(List<GameObject> _nodeRoute)
    {
        for (int i = 0; i < _nodeRoute.Count; i += 2)
        {
            List<GameObject> possibleRoomCells = new List<GameObject>();
            List<GameObject> possibleNeighbourRoomCells = new List<GameObject>();

            possibleRoomCells = FillPossibleRoomCells(_nodeRoute, i);
            possibleNeighbourRoomCells = FillNeighbourPossibleRoomCells(_nodeRoute, i);

            int rand = Random.Range(0, possibleRoomCells.Count);
            int numNeighbour = 0;

            Vector3 nodeDir = Vector3.zero;
            Vector3 neighbourNodePos = Vector3.zero;

            if (possibleRoomCells.Count > 0)
            {
                Vector3 actualNodePos = grid.GetGridPosition(possibleRoomCells[rand].transform.position.x,
                                                            possibleRoomCells[rand].transform.position.y);

                #region Find in which direction the neighbour is located
                for (int l = 0; l < possibleNeighbourRoomCells.Count; l++)
                {
                    neighbourNodePos = grid.GetGridPosition(possibleNeighbourRoomCells[l].transform.position.x,
                                                    possibleNeighbourRoomCells[l].transform.position.y);

                    nodeDir = actualNodePos - neighbourNodePos;

                    if (nodeDir.x == 1 && nodeDir.y == 0 || nodeDir.x == -1 && nodeDir.y == 0 ||
                        nodeDir.y == 1 && nodeDir.x == 0 || nodeDir.y == -1 && nodeDir.x == 0)
                    {
                        numNeighbour = l;
                        break;
                    }
                }
                #endregion

                #region Open doors

                #region Right
                if (nodeDir.x == 1 && nodeDir.y == 0)
                {
                    possibleRoomCells[rand].GetComponent<RoomCell>().OpenDoor(3, true);
                    possibleNeighbourRoomCells[numNeighbour].GetComponent<RoomCell>().OpenDoor(2, true);

                    if (!possibleRoomCells[rand].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject))
                        possibleRoomCells[rand].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject);

                    if (!possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleRoomCells[rand].transform.parent.gameObject))
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleRoomCells[rand].transform.parent.gameObject);
                }
                #endregion
                #region Left
                else if (nodeDir.x == -1 && nodeDir.y == 0)
                {
                    possibleRoomCells[rand].GetComponent<RoomCell>().OpenDoor(2, true);
                    possibleNeighbourRoomCells[numNeighbour].GetComponent<RoomCell>().OpenDoor(3, true);

                    if (!possibleRoomCells[rand].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject))
                        possibleRoomCells[rand].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject);

                    if (!possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleRoomCells[rand].transform.parent.gameObject))
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleRoomCells[rand].transform.parent.gameObject);
                }
                #endregion
                #region Up
                else if (nodeDir.y == 1 && nodeDir.x == 0)
                {

                    possibleRoomCells[rand].GetComponent<RoomCell>().OpenDoor(1, true);
                    possibleNeighbourRoomCells[numNeighbour].GetComponent<RoomCell>().OpenDoor(0, true);

                    if (!possibleRoomCells[rand].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject))
                        possibleRoomCells[rand].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject);

                    if (!possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleRoomCells[rand].transform.parent.gameObject))
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleRoomCells[rand].transform.parent.gameObject);
                }
                #endregion
                #region Down
                else if (nodeDir.y == -1 && nodeDir.x == 0)
                {
                    possibleRoomCells[rand].GetComponent<RoomCell>().OpenDoor(0, true);
                    possibleNeighbourRoomCells[numNeighbour].GetComponent<RoomCell>().OpenDoor(1, true);

                    if (!possibleRoomCells[rand].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject))
                        possibleRoomCells[rand].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleNeighbourRoomCells[numNeighbour].transform.parent.gameObject);

                    if (!possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().GetNeighboursOpenDoors().Contains(
                        possibleRoomCells[rand].transform.parent.gameObject))
                        possibleNeighbourRoomCells[numNeighbour].transform.parent.GetComponent<Room>().AddNeighboursOpenDoor(
                            possibleRoomCells[rand].transform.parent.gameObject);
                }
                #endregion

                #endregion
            }
        }
    }

    private List<GameObject> FillPossibleRoomCells(List<GameObject> _nodeRoute, int _i)
    {
        List<GameObject> _possibleRoomCells = new List<GameObject>();

        for (int j = 0; j < _nodeRoute[_i].transform.childCount; j++)
        {
            List<int> roomNeighboursList = new List<int>();

            for (int i = 0; i < _nodeRoute[_i].transform.GetChild(j).GetComponent<RoomCell>().GetNeighboursList().Length; i++)
            {
                if (_nodeRoute[_i].transform.GetChild(j).GetComponent<RoomCell>().GetNeighboursList()[i] != 0)
                    roomNeighboursList.Add(_nodeRoute[_i].transform.GetChild(j).GetComponent<RoomCell>().GetNeighboursList()[i]);
            }

            for (int l = 0; l < roomNeighboursList.Count; l++)
            {
                if (roomNeighboursList[l] == _nodeRoute[_i + 1].GetComponent<Room>().GetRoomNumber())
                {
                    _possibleRoomCells.Add(_nodeRoute[_i].transform.GetChild(j).gameObject);
                }
            }
        }
        return _possibleRoomCells;
    }

    private List<GameObject> FillNeighbourPossibleRoomCells(List<GameObject> _nodeRoute, int _i)
    {
        List<GameObject> _possibleNeighbourRoomCells = new List<GameObject>();

        for (int j = 0; j < _nodeRoute[_i + 1].transform.childCount; j++)
        {
            List<int> roomNeighboursList = new List<int>();

            for (int i = 0; i < _nodeRoute[_i + 1].transform.GetChild(j).GetComponent<RoomCell>().GetNeighboursList().Length; i++)
            {
                if (_nodeRoute[_i + 1].transform.GetChild(j).GetComponent<RoomCell>().GetNeighboursList()[i] != 0)
                    roomNeighboursList.Add(_nodeRoute[_i + 1].transform.GetChild(j).GetComponent<RoomCell>().GetNeighboursList()[i]);
            }

            for (int l = 0; l < roomNeighboursList.Count; l++)
            {
                if (roomNeighboursList[l] == _nodeRoute[_i].GetComponent<Room>().GetRoomNumber())
                {
                    _possibleNeighbourRoomCells.Add(_nodeRoute[_i + 1].transform.GetChild(j).gameObject);
                }
            }
        }
        return _possibleNeighbourRoomCells;
    }

    private void MakeZonesOrder()
    {
        foreach (GameObject go in routeOfZones)
        {
            if (!zonesOrder.Contains(go.GetComponent<Pointers>().GetPointerNumber()))
            {
                zonesOrder.Add(go.GetComponent<Pointers>().GetPointerNumber());
            }
        }
    }

    public List<int> GetZonesOrder()
    {
        MakeZonesOrder();
        return zonesOrder;
    }

    public void Reset()
    {
        nodes.Clear();
        routeOfZones.Clear();
    }

    private void OnDrawGizmos()
    {
        if (ShowGraph)
        {
            Gizmos.color = Color.red;
            int zones = transform.childCount;

            for (int i = zones - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).tag == "Zones" && transform.GetChild(i).name != "Zone 0")
                {
                    int rooms = transform.GetChild(i).childCount;

                    for (int j = rooms - 1; j >= 0; j--)
                    {
                        if (transform.GetChild(i).GetChild(j).tag == "Rooms")
                        {

                            Gizmos.DrawSphere(transform.GetChild(i).GetChild(j).GetComponent<Room>().GetRoomCenterPos(), 2f);
                            for (int x = 0; x < transform.GetChild(i).GetChild(j).GetComponent<Room>().GetNeighboursList().Count; x++)
                            {
                                if (transform.GetChild(i).GetChild(j).GetComponent<Room>().GetNeighboursList()[x].name != "Room 999")
                                    Gizmos.DrawLine(transform.GetChild(i).GetChild(j).GetComponent<Room>().GetRoomCenterPos(),
                                        transform.GetChild(i).GetChild(j).GetComponent<Room>().GetNeighboursList()[x].GetComponent<Room>().GetRoomCenterPos());
                            }
                        }
                    }
                }
            }
        }

        if (ShowMinRoute)
        {
            Gizmos.color = Color.green;
            int zones = transform.childCount;

            for (int i = zones - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).tag == "Zones" && transform.GetChild(i).name != "Zone 0")
                {
                    for (int j = 0; j < transform.GetChild(i).GetComponent<Zone>().GetRouteRoomList().Count; j += 2)
                    {
                        Gizmos.DrawLine(transform.GetChild(i).GetComponent<Zone>().GetRouteRoomList()[j].GetComponent<Room>().GetRoomCenterPos(),
                            transform.GetChild(i).GetComponent<Zone>().GetRouteRoomList()[j + 1].GetComponent<Room>().GetRoomCenterPos());
                    }
                    int rooms = transform.GetChild(i).childCount;

                    for (int j = rooms - 1; j >= 0; j--)
                    {
                        if (transform.GetChild(i).GetChild(j).tag == "Rooms")
                        {
                            Gizmos.DrawSphere(transform.GetChild(i).GetChild(j).GetComponent<Room>().GetRoomCenterPos(), 2f);
                        }
                    }
                }
            }
        }
    }
}
