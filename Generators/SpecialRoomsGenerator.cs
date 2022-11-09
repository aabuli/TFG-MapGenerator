using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialRoomsGenerator : MonoBehaviour
{
    private RouteGenerator routeGen;
    private RoomGenerator roomGen;
    private ZonesGenerator zoneGen;
    private Grid grid;

    private int[,] arrayIntGrid;
    private int[,] arrayZoneGrid;

    private List<int> zonesConnected = new List<int>();
    private List<int> connectionsToIgnore = new List<int>();
    private List<Room> possibleSaveRoomsList = new List<Room>();
    private List<Vector3> _roomsPositionTaken = new List<Vector3>();
    private List<int> zonesOrder = new List<int>();

    [Space]
    [Header("Numbers")]
    [SerializeField][Range(6, 12)] private int numberOfRoomsPerSaveRoom = 8;

    [Header("Special Rooms Icons")]
    [SerializeField] private Texture bossIcon = null;
    [SerializeField] private Texture[] lockIcon = new Texture[6];
    [SerializeField] private Texture[] keyIcon = new Texture[6];

    public void GenerateSpecialRooms()
    {
        Reset();
        grid = GetComponent<GridGenerator>().GetGrid();
        routeGen = GetComponent<RouteGenerator>();
        roomGen = GetComponent<RoomGenerator>();
        zoneGen = GetComponent<ZonesGenerator>();
        arrayIntGrid = roomGen.GetArrayIntGrid();
        arrayZoneGrid = roomGen.GetArrayZoneGrid();
        zonesOrder = routeGen.GetZonesOrder();
        FillZonesConexions();
        OpenTransitionDoors();
        RemoveUnopenRooms();
    }

    public bool GenerateTransitionRooms()
    {
        foreach (int connection in zonesConnected)
        {

            Transform zone = transform.Find("Zone " + connection.ToString().ToCharArray()[0]);
            List<Vector3> possibleRooms = new List<Vector3>();
            List<RoomCell> previousRooms = new List<RoomCell>();

            foreach (Transform room in zone.transform)
            {
                foreach (Transform roomCell in room.transform)
                {
                    if (roomCell.tag == "RoomCell")
                    {
                        RoomCell _roomCell = roomCell.GetComponent<RoomCell>();

                        #region Up
                        if ((int)_roomCell.GetRoomCellGridPos().y + 1 < roomGen.GetArrayZoneGrid().GetLength(1) &&
                            roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x,
                            (int)_roomCell.GetRoomCellGridPos().y + 1] == 0 &&
                            !_roomsPositionTaken.Contains(new Vector3((int)_roomCell.GetRoomCellGridPos().x,
                            (int)_roomCell.GetRoomCellGridPos().y + 1)))
                        {
                            if (CheckSides((int)_roomCell.GetRoomCellGridPos().x,
                                (int)_roomCell.GetRoomCellGridPos().y + 1,
                                int.Parse(connection.ToString().ToCharArray()[1].ToString()), _roomCell))
                            {
                                if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                {
                                    possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x,
                                    (int)_roomCell.GetRoomCellGridPos().y + 1));

                                    _roomCell.SetTransitionDown(true);
                                    previousRooms.Add(_roomCell);
                                }
                            }
                        }
                        #endregion
                        #region Down
                        if ((int)_roomCell.GetRoomCellGridPos().y - 1 >= 0 &&
                            roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x,
                            (int)_roomCell.GetRoomCellGridPos().y - 1] == 0 &&
                            !_roomsPositionTaken.Contains(new Vector3((int)_roomCell.GetRoomCellGridPos().x,
                            (int)_roomCell.GetRoomCellGridPos().y - 1)))
                        {
                            if (CheckSides((int)_roomCell.GetRoomCellGridPos().x,
                                (int)_roomCell.GetRoomCellGridPos().y - 1,
                                int.Parse(connection.ToString().ToCharArray()[1].ToString()), _roomCell))
                            {
                                if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                {
                                    possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x,
                                        (int)_roomCell.GetRoomCellGridPos().y - 1));

                                    _roomCell.SetTransitionUp(true);
                                    previousRooms.Add(_roomCell);
                                }
                            }
                        }
                        #endregion
                        #region Right
                        if ((int)_roomCell.GetRoomCellGridPos().x + 1 < roomGen.GetArrayZoneGrid().GetLength(0) &&
                            roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x + 1,
                            (int)_roomCell.GetRoomCellGridPos().y] == 0)
                        {
                            if (CheckSides((int)_roomCell.GetRoomCellGridPos().x + 1,
                                (int)_roomCell.GetRoomCellGridPos().y,
                                int.Parse(connection.ToString().ToCharArray()[1].ToString()), _roomCell))
                            {
                                if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                {
                                    possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x + 1,
                                    (int)_roomCell.GetRoomCellGridPos().y));

                                    _roomCell.SetTransitionLeft(true);
                                    previousRooms.Add(_roomCell);
                                }
                            }
                        }
                        #endregion
                        #region Left
                        if ((int)_roomCell.GetRoomCellGridPos().x - 1 >= 0 &&
                            roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x - 1,
                            (int)_roomCell.GetRoomCellGridPos().y] == 0)
                        {
                            if (CheckSides((int)_roomCell.GetRoomCellGridPos().x - 1,
                                (int)_roomCell.GetRoomCellGridPos().y,
                                int.Parse(connection.ToString().ToCharArray()[1].ToString()), _roomCell))
                            {
                                if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                {
                                    possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x - 1,
                                        (int)_roomCell.GetRoomCellGridPos().y));

                                    _roomCell.SetTransitionRight(true);
                                    previousRooms.Add(_roomCell);
                                }
                            }
                        }
                        #endregion
                    }
                }
            }

            if (possibleRooms.Count > 0)
            {
                int rnd = Random.Range(0, possibleRooms.Count);
                _roomsPositionTaken.Add(possibleRooms[rnd]);

                roomGen.SpawnNewRoom((int)possibleRooms[rnd].x, (int)possibleRooms[rnd].y, 0, transitionRoom: true,
                    transitionUp: previousRooms[rnd].IsTransitionUp(), transitionDown: previousRooms[rnd].IsTransitionDown(),
                    transitionRight: previousRooms[rnd].IsTransitionRight(), transitionLeft: previousRooms[rnd].IsTransitionLeft());
            }
            else
            {
                zone = transform.Find("Zone " + connection.ToString().ToCharArray()[1]);
                possibleRooms.Clear();
                previousRooms.Clear();

                foreach (Transform room in zone.transform)
                {
                    foreach (Transform roomCell in room.transform)
                    {
                        if (roomCell.tag == "RoomCell")
                        {
                            RoomCell _roomCell = roomCell.GetComponent<RoomCell>();

                            #region Up
                            if ((int)_roomCell.GetRoomCellGridPos().y + 1 < roomGen.GetArrayZoneGrid().GetLength(1) &&
                                roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x,
                                (int)_roomCell.GetRoomCellGridPos().y + 1] == 0)
                            {
                                if (CheckSides((int)_roomCell.GetRoomCellGridPos().x,
                                    (int)_roomCell.GetRoomCellGridPos().y + 1,
                                    int.Parse(connection.ToString().ToCharArray()[0].ToString()), _roomCell))
                                {
                                    if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                    {
                                        possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x,
                                        (int)_roomCell.GetRoomCellGridPos().y + 1));

                                        _roomCell.SetTransitionDown(true);
                                        previousRooms.Add(_roomCell);
                                    }
                                }
                            }
                            #endregion
                            #region Down
                            if ((int)_roomCell.GetRoomCellGridPos().y - 1 >= 0 &&
                                roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x,
                                (int)_roomCell.GetRoomCellGridPos().y - 1] == 0)
                            {
                                if (CheckSides((int)_roomCell.GetRoomCellGridPos().x,
                                    (int)_roomCell.GetRoomCellGridPos().y - 1,
                                    int.Parse(connection.ToString().ToCharArray()[0].ToString()), _roomCell))
                                {
                                    if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                    {
                                        possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x,
                                            (int)_roomCell.GetRoomCellGridPos().y - 1));

                                        _roomCell.SetTransitionUp(true);
                                        previousRooms.Add(_roomCell);
                                    }
                                }
                            }
                            #endregion
                            #region Right
                            if ((int)_roomCell.GetRoomCellGridPos().x + 1 < roomGen.GetArrayZoneGrid().GetLength(0) &&
                                roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x + 1,
                                (int)_roomCell.GetRoomCellGridPos().y] == 0)
                            {
                                if (CheckSides((int)_roomCell.GetRoomCellGridPos().x + 1,
                                    (int)_roomCell.GetRoomCellGridPos().y,
                                    int.Parse(connection.ToString().ToCharArray()[0].ToString()), _roomCell))
                                {
                                    if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                    {
                                        possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x + 1,
                                        (int)_roomCell.GetRoomCellGridPos().y));

                                        _roomCell.SetTransitionLeft(true);
                                        previousRooms.Add(_roomCell);
                                    }
                                }
                            }
                            #endregion
                            #region Left
                            if ((int)_roomCell.GetRoomCellGridPos().x - 1 >= 0 &&
                                roomGen.GetArrayZoneGrid()[(int)_roomCell.GetRoomCellGridPos().x - 1,
                                (int)_roomCell.GetRoomCellGridPos().y] == 0)
                            {
                                if (CheckSides((int)_roomCell.GetRoomCellGridPos().x - 1,
                                    (int)_roomCell.GetRoomCellGridPos().y,
                                    int.Parse(connection.ToString().ToCharArray()[0].ToString()), _roomCell))
                                {
                                    if (NumberOfDoorsAlreadyOpen(_roomCell) < 2)
                                    {
                                        possibleRooms.Add(new Vector3((int)_roomCell.GetRoomCellGridPos().x - 1,
                                        (int)_roomCell.GetRoomCellGridPos().y));

                                        _roomCell.SetTransitionRight(true);
                                        previousRooms.Add(_roomCell);
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }

                if (possibleRooms.Count > 0)
                {

                    int rnd = Random.Range(0, possibleRooms.Count);
                    _roomsPositionTaken.Add(possibleRooms[rnd]);

                    roomGen.SpawnNewRoom((int)possibleRooms[rnd].x, (int)possibleRooms[rnd].y, 0, transitionRoom: true,
                        transitionUp: previousRooms[rnd].IsTransitionUp(), transitionDown: previousRooms[rnd].IsTransitionDown(),
                        transitionRight: previousRooms[rnd].IsTransitionRight(), transitionLeft: previousRooms[rnd].IsTransitionLeft());
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    void FillZonesConexions()
    {
        foreach (int zoneNumber in zonesOrder)
        {
            Pointers _pointer = transform.Find("Pointers").Find("Pointer " + zoneNumber).GetComponent<Pointers>();

            for (int i = 0; i < _pointer.GetZoneNieghbours().Count; i++)
            {
                int num = int.Parse(_pointer.GetPointerNumber().ToString() + _pointer.GetZoneNieghbours()[i].ToString());
                int numReversed = int.Parse(_pointer.GetZoneNieghbours()[i].ToString() + _pointer.GetPointerNumber().ToString());

                if (!connectionsToIgnore.Contains(num))
                {
                    zonesConnected.Add(num);
                    connectionsToIgnore.Add(numReversed);
                }
            }
        }
    }

    private bool CheckSides(int _x, int _y, int _neighbourZoneNumber, RoomCell _actualRoomCell)
    {
        if (_x + 1 < roomGen.GetArrayZoneGrid().GetLength(0) &&
        roomGen.GetArrayZoneGrid()[_x + 1, _y] == _neighbourZoneNumber)
        {
            if (NumberOfDoorsAlreadyOpen(_actualRoomCell) < 2)
                _actualRoomCell.SetTransitionRight(true);
            return true;
        }
        else if (_x - 1 >= 0 &&
        roomGen.GetArrayZoneGrid()[_x - 1, _y] == _neighbourZoneNumber)
        {
            if (NumberOfDoorsAlreadyOpen(_actualRoomCell) < 2)
                _actualRoomCell.SetTransitionLeft(true);
            return true;
        }
        else if (_y + 1 < roomGen.GetArrayZoneGrid().GetLength(1) &&
        roomGen.GetArrayZoneGrid()[_x, _y + 1] == _neighbourZoneNumber)
        {
            if (NumberOfDoorsAlreadyOpen(_actualRoomCell) < 2)
                _actualRoomCell.SetTransitionUp(true);
            return true;
        }
        else if (_y - 1 >= 0 &&
        roomGen.GetArrayZoneGrid()[_x, _y - 1] == _neighbourZoneNumber)
        {
            if (NumberOfDoorsAlreadyOpen(_actualRoomCell) < 2)
                _actualRoomCell.SetTransitionDown(true);
            return true;
        }

        return false;
    }

    private int NumberOfDoorsAlreadyOpen(RoomCell _rCell)
    {
        int num = 0;
        if (_rCell.IsTransitionUp())
            num++;
        if (_rCell.IsTransitionDown())
            num++;
        if (_rCell.IsTransitionRight())
            num++;
        if (_rCell.IsTransitionLeft())
            num++;

        return num;
    }

    public void OpenTransitionDoors()
    {
        Transform _zone0 = transform.Find("Zone 0");

        foreach (Transform room in _zone0)
        {

            foreach (Transform roomCell in room.transform)
            {
                if (roomCell.tag == "RoomCell")
                {
                    RoomCell _roomCell = roomCell.GetComponent<RoomCell>();

                    #region Left
                    if (_roomCell.IsTransitionLeft())
                    {
                        _roomCell.OpenDoor(3, true);
                        Transform transitionZone1 = transform.Find("Zone " + _roomCell.GetZonesForTransitionRoom()[3]);
                        Transform transitionRoom1 = transitionZone1.Find("Room " + _roomCell.GetNeighboursList()[3]);

                        foreach (Transform neighbourRoomCell in transitionRoom1)
                        {
                            RoomCell _neighbourRoomCell = neighbourRoomCell.GetComponent<RoomCell>();

                            for (int z = 0; z < _neighbourRoomCell.GetNeighboursList().Length; z++)
                            {
                                if (_neighbourRoomCell.GetNeighboursList()[z] == _roomCell.GetRoomNumber())
                                {
                                    _neighbourRoomCell.OpenDoor(2, true);
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                    #region Right
                    if (_roomCell.IsTransitionRight())
                    {
                        _roomCell.OpenDoor(2, true);
                        Transform transitionZone1 = transform.Find("Zone " + _roomCell.GetZonesForTransitionRoom()[2]);
                        Transform transitionRoom1 = transitionZone1.Find("Room " + _roomCell.GetNeighboursList()[2]);

                        foreach (Transform neighbourRoomCell in transitionRoom1)
                        {
                            RoomCell _neighbourRoomCell = neighbourRoomCell.GetComponent<RoomCell>();

                            for (int z = 0; z < _neighbourRoomCell.GetNeighboursList().Length; z++)
                            {
                                if (_neighbourRoomCell.GetNeighboursList()[z] == _roomCell.GetRoomNumber())
                                {
                                    _neighbourRoomCell.OpenDoor(3, true);
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                    #region Down
                    if (_roomCell.IsTransitionDown())
                    {
                        _roomCell.OpenDoor(1, true);
                        Transform transitionZone1 = transform.Find("Zone " + _roomCell.GetZonesForTransitionRoom()[1]);
                        Transform transitionRoom1 = transitionZone1.Find("Room " + _roomCell.GetNeighboursList()[1]);

                        foreach (Transform neighbourRoomCell in transitionRoom1)
                        {
                            RoomCell _neighbourRoomCell = neighbourRoomCell.GetComponent<RoomCell>();

                            for (int z = 0; z < _neighbourRoomCell.GetNeighboursList().Length; z++)
                            {
                                if (_neighbourRoomCell.GetNeighboursList()[z] == _roomCell.GetRoomNumber())
                                {
                                    _neighbourRoomCell.OpenDoor(0, true);
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                    #region Up
                    if (_roomCell.IsTransitionUp())
                    {
                        _roomCell.OpenDoor(0, true);
                        Transform transitionZone1 = transform.Find("Zone " + _roomCell.GetZonesForTransitionRoom()[0]);
                        Transform transitionRoom1 = transitionZone1.Find("Room " + _roomCell.GetNeighboursList()[0]);

                        foreach (Transform neighbourRoomCell in transitionRoom1)
                        {
                            RoomCell _neighbourRoomCell = neighbourRoomCell.GetComponent<RoomCell>();

                            for (int z = 0; z < _neighbourRoomCell.GetNeighboursList().Length; z++)
                            {
                                if (_neighbourRoomCell.GetNeighboursList()[z] == _roomCell.GetRoomNumber())
                                {
                                    _neighbourRoomCell.OpenDoor(1, true);
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }
    }

    public void GenerateHorizontalPath()
    {
        arrayIntGrid = roomGen.GetArrayIntGrid();
        arrayZoneGrid = roomGen.GetArrayZoneGrid();

        foreach (int zoneNumber in zonesOrder)
        {
            Transform zone = transform.Find("Zone " + zoneNumber);
            Pointers _pointer = transform.Find("Pointers").transform.Find("Pointer " + zoneNumber).gameObject.GetComponent<Pointers>();

            if (_pointer != null)
            {
                foreach (int neighbourZoneNumber in _pointer.GetZoneNieghbours())
                {
                    int min = 999999999;
                    GameObject bestRoomCell = null;

                    foreach (Transform room in zone.transform)
                    {
                        foreach (Transform roomCell in room.transform)
                        {
                            if (roomCell.tag == "RoomCell")
                            {
                                int nSteps = 0;
                                RoomCell _roomCell = roomCell.GetComponent<RoomCell>();

                                do
                                {
                                    if ((int)_roomCell.GetRoomCellGridPos().x + 1 < arrayIntGrid.GetLength(0) - 1)
                                        nSteps++;

                                } while ((int)_roomCell.GetRoomCellGridPos().x + nSteps < arrayIntGrid.GetLength(0) - 1 &&
                                    arrayIntGrid[(int)_roomCell.GetRoomCellGridPos().x + nSteps, (int)_roomCell.GetRoomCellGridPos().y] == 0);

                                if (arrayZoneGrid[(int)_roomCell.GetRoomCellGridPos().x + nSteps, (int)_roomCell.GetRoomCellGridPos().y] == neighbourZoneNumber)
                                {
                                    if (nSteps > 1 && nSteps < min)
                                    {
                                        min = nSteps;
                                        bestRoomCell = roomCell.gameObject;
                                    }
                                }
                            }
                        }
                    }
                    if (bestRoomCell != null)
                    {
                        min--;

                        if (min > 1)
                        {
                            do
                            {
                                min--;
                                roomGen.SpawnNewRoom((int)bestRoomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x + min,
                                    (int)bestRoomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y, zoneNumber);

                            } while (min > 1);
                        }
                    }
                }
            }
        }
    }

    public bool GenerateBossRooms()
    {
        List<GameObject> listOfPossibleRooms = new List<GameObject>();

        for (int i = 0; i < zonesOrder.Count; i++)
        {
            Transform zone = transform.Find("Zone " + zonesOrder[i]);
            listOfPossibleRooms.Clear();

            foreach (Transform room in zone)
            {
                Room _room = room.GetComponent<Room>();

                if (_room != null)
                {
                    if ((_room.GetWidth() * _room.GetHeight()) >= 4 &&
                        (_room.GetWidth() * _room.GetHeight()) <= 9 && _room.GetWidth() > 1)
                    {
                        int numOfDoors = _room.GetNumberOfDoors();

                        var isNextToBossRoom = _room.GetNeighboursList().Where(x => x.name == "Room 999").SingleOrDefault();
                        if (numOfDoors <= 2 && isNextToBossRoom == null)
                        {
                            listOfPossibleRooms.Add(room.gameObject);
                        }
                    }
                }
            }

            if (listOfPossibleRooms.Count == 0)
            {
                foreach (Transform room in zone)
                {
                    Room _room = room.GetComponent<Room>();
                    if (_room != null)
                    {
                        if ((_room.GetWidth() * _room.GetHeight()) >= 4 &&
                        (_room.GetWidth() * _room.GetHeight()) <= 6 && _room.GetWidth() > 1)
                        {
                            int numOfDoors = _room.GetNumberOfDoors();

                            var isNextToBossRoom = _room.GetNeighboursList().Where(x => x.name == "Room 999").SingleOrDefault();
                            if (numOfDoors == 3 && isNextToBossRoom == null)
                            {
                                listOfPossibleRooms.Add(room.gameObject);
                            }
                        }
                    }
                }
            }

            if (listOfPossibleRooms.Count > 0 && bossIcon != null)
            {
                Transform pointers = transform.Find("Pointers");

                if (i == 0)
                {
                    int num = 0;

                    if (zoneGen.GetNumberofZones() > 1)
                    {
                        Transform _pointer = pointers.Find("Pointer " + zonesOrder[i + 1]);
                        float minDistance = 9999999f;

                        for (int j = 0; j < listOfPossibleRooms.Count; j++)
                        {
                            float distance = Vector2.Distance(_pointer.position, listOfPossibleRooms[j].GetComponent<Room>().GetRoomCenterPos());
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                num = j;
                            }
                        }
                    }
                    else
                    {
                        float minDistance = 0;

                        for (int j = 0; j < listOfPossibleRooms.Count; j++)
                        {
                            float distance = Vector2.Distance(Vector2.zero, listOfPossibleRooms[j].GetComponent<Room>().GetRoomCenterPos());
                            if (distance > minDistance)
                            {
                                minDistance = distance;
                                num = j;
                            }
                        }
                    }

                    listOfPossibleRooms[num].GetComponent<Room>().SetRoomIcon(bossIcon);
                    foreach (Transform roomCell in listOfPossibleRooms[num].transform)
                    {
                        if (roomCell.tag == "RoomCell")
                        {
                            roomCell.GetComponent<RoomCell>().SetBossRoom(true);
                        }
                    };
                }
                else
                {
                    int num = 0;
                    float maxDistance = 0;
                    Transform _pointer = pointers.Find("Pointer " + zonesOrder[i - 1]);

                    for (int j = 0; j < listOfPossibleRooms.Count; j++)
                    {
                        float distance = Vector2.Distance(_pointer.position, listOfPossibleRooms[j].GetComponent<Room>().GetRoomCenterPos());

                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            num = j;
                        }
                    }

                    listOfPossibleRooms[num].GetComponent<Room>().SetRoomIcon(bossIcon);
                    foreach (Transform roomCell in listOfPossibleRooms[num].transform)
                    {
                        if (roomCell.tag == "RoomCell")
                        {
                            roomCell.GetComponent<RoomCell>().SetBossRoom(true);
                        }
                    };
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public void GenerateSaveRooms()
    {
        for (int z = 0; z < zonesOrder.Count; z++)
        {
            Transform zone = transform.Find("Zone " + zonesOrder[z]);
            possibleSaveRoomsList.Clear();
            int numOfSaveRooms = zone.childCount / numberOfRoomsPerSaveRoom;
            Room bossRoom = null;
            Room roomNextToBoss = null;

            if (numOfSaveRooms < 1)
                numOfSaveRooms = 1;

            #region SaveRoom next to Boss
            foreach (Transform _bossRoom in zone.transform)
            {
                foreach (Transform roomCell in _bossRoom)
                {
                    if (roomCell.GetComponent<RoomCell>().IsBossRoom())
                    {
                        bossRoom = _bossRoom.GetComponent<Room>();
                        break;
                    }
                }
            }

            if (bossRoom != null)
            {
                for (int i = 0; i < bossRoom.GetNeighboursOpenDoors().Count; i++)
                {
                    if (bossRoom.GetNeighboursOpenDoors()[i].GetComponent<Room>().IsBeforeBoss())
                        roomNextToBoss = bossRoom.GetNeighboursOpenDoors()[i].GetComponent<Room>();
                }

                if (roomNextToBoss == null)
                {
                    float min = 99999999f;
                    int index = 0;

                    for (int i = 0; i < bossRoom.GetNeighboursOpenDoors().Count; i++)
                    {
                        float distance = Vector2.Distance(bossRoom.GetNeighboursOpenDoors()[i].GetComponent<Room>().GetRoomCenterPos(),
                            zone.GetComponent<Zone>().GetFirstZoneRoom().GetComponent<Room>().GetRoomCenterPos());

                        if (distance < min)
                        {
                            min = distance;
                            index = i;
                        }
                    }

                    roomNextToBoss = bossRoom.GetNeighboursOpenDoors()[index].GetComponent<Room>();
                }

                GameObject suitableNeighbourRoom = null;
                float minDistance = 99999999f;
                int bestRoomNumber = 0;

                foreach (Transform _room in zone.transform)
                {
                    if (RoomSuitableForSaveRoom(_room.GetComponent<Room>(), true))
                        possibleSaveRoomsList.Add(_room.GetComponent<Room>());
                }

                if (possibleSaveRoomsList.Count > 0)
                {
                    for (int i = 0; i < possibleSaveRoomsList.Count; i++)
                    {
                        if (Vector2.Distance(possibleSaveRoomsList[i].GetRoomCenterPos(), roomNextToBoss.GetRoomCenterPos()) < minDistance &&
                            !_roomsPositionTaken.Contains(possibleSaveRoomsList[i].GetRoomCenterPos()))
                        {
                            minDistance = Vector2.Distance(possibleSaveRoomsList[i].GetRoomCenterPos(), roomNextToBoss.GetRoomCenterPos());
                            bestRoomNumber = i;
                        }
                    }
                    suitableNeighbourRoom = possibleSaveRoomsList[bestRoomNumber].gameObject;
                    _roomsPositionTaken.Add(suitableNeighbourRoom.GetComponent<Room>().GetRoomCenterPos());
                    SpawnSaveRoom(suitableNeighbourRoom, zone.GetComponent<Zone>());
                    numOfSaveRooms--;
                }
            }
            #endregion

            if (numOfSaveRooms > 0)
            {
                int nGroup = zone.transform.childCount / numOfSaveRooms;
                int num = 0;
                List<Room> roomList = new List<Room>();

                foreach (Transform room in zone.transform)
                {
                    roomList.Add(room.GetComponent<Room>());
                }

                roomList = roomList.OrderBy(
                   x => Vector2.Distance(bossRoom.GetRoomCenterPos(), x.GetRoomCenterPos())
                  ).ToList();

                roomList.Reverse();

                if (numOfSaveRooms > 1)
                    roomList.RemoveRange(roomList.Count - nGroup, nGroup);

                nGroup = roomList.Count / numOfSaveRooms;

                for (int i = 0; i < numOfSaveRooms; i++)
                {

                    if (num < roomList.Count - 1)
                        num = i * nGroup;

                    do
                    {
                        if (num < roomList.Count - 1)
                            num++;
                        else
                            break;

                    } while (!RoomSuitableForSaveRoom(roomList[num].GetComponent<Room>()) &&
                    _roomsPositionTaken.Contains(roomList[num].GetComponent<Room>().GetRoomCenterPos()));

                    _roomsPositionTaken.Add(roomList[num].GetComponent<Room>().GetRoomCenterPos());
                    SpawnSaveRoom(roomList[num].gameObject, zone.GetComponent<Zone>());

                }
            }
        }
    }

    private bool RoomSuitableForSaveRoom(Room _room, bool beforeBoos = false)
    {
        foreach (Transform roomCell in _room.transform)
        {
            if (roomCell.GetComponent<RoomCell>().IsBossRoom() || roomCell.GetComponent<RoomCell>().IsSaveRoom() ||
                roomCell.GetComponent<RoomCell>().IsTeleportRoom() || roomCell.GetComponent<RoomCell>().IsSaveRoom() ||
                roomCell.GetComponent<RoomCell>().IsTransitionRoom())
            {
                return false;
            }
        }

        if (beforeBoos)
        {
            if (HasSpaceForARoom(_room) && _room.IsBeforeBoss())
            {
                return true;
            }
        }
        else
        {
            if (HasSpaceForARoom(_room))
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnSaveRoom(GameObject _go, Zone _zone)
    {
        bool roomSpawned = false;
        int num = 1;

        while (IsRoomNumberAlreadyPicked(num, _zone.GetZoneNumber()))
        {
            num++;
        }

        foreach (Transform roomCell in _go.transform)
        {
            if (roomCell.GetComponent<RoomCell>())
            {
                RoomCell _roomCell = roomCell.GetComponent<RoomCell>();
                int[] neighbours = _roomCell.GetNeighboursList();

                if (neighbours != null || neighbours.Length == 0)
                {
                    if (neighbours[2] == 0 && !roomSpawned)
                    {
                        if ((int)_roomCell.GetRoomCellGridPos().x + 1 < arrayIntGrid.GetLength(0) &&
                            arrayIntGrid[(int)_roomCell.GetRoomCellGridPos().x + 1,
                            (int)_roomCell.GetRoomCellGridPos().y] == 0)
                        {
                            roomGen.SpawnNewRoom((int)_roomCell.GetRoomCellGridPos().x + 1, (int)_roomCell.GetRoomCellGridPos().y,
                                _zone.GetZoneNumber(), roomNumber: num, saveRoom: true, transitionLeft: true);
                            roomSpawned = true;
                        }
                    }
                    if (neighbours[3] == 0 && !roomSpawned)
                    {
                        if ((int)_roomCell.GetRoomCellGridPos().x - 1 > 0 &&
                                        arrayIntGrid[(int)_roomCell.GetRoomCellGridPos().x - 1,
                                        (int)_roomCell.GetRoomCellGridPos().y] == 0)
                        {
                            roomGen.SpawnNewRoom((int)_roomCell.GetRoomCellGridPos().x - 1, (int)_roomCell.GetRoomCellGridPos().y,
                                _zone.GetZoneNumber(), roomNumber: num, saveRoom: true, transitionRight: true);
                            roomSpawned = true;
                        }
                    }
                }
            }
        }
    }

    private bool SpawnTeleportRoom(GameObject _go, Zone _zone)
    {
        bool roomSpawned = false;
        int num = 1;

        while (IsRoomNumberAlreadyPicked(num, _zone.GetZoneNumber()))
        {
            num++;
        }

        foreach (Transform roomCell in _go.transform)
        {
            if (roomCell.GetComponent<RoomCell>())
            {
                RoomCell _roomCell = roomCell.GetComponent<RoomCell>();
                int[] neighbours = _roomCell.GetNeighboursList();

                if (neighbours != null || neighbours.Length == 0)
                {
                    if (neighbours[2] == 0 && !roomSpawned)
                    {
                        if ((int)_roomCell.GetRoomCellGridPos().x + 1 < arrayIntGrid.GetLength(0) &&
                            arrayIntGrid[(int)_roomCell.GetRoomCellGridPos().x + 1,
                            (int)_roomCell.GetRoomCellGridPos().y] == 0)
                        {
                            roomGen.SpawnNewRoom((int)_roomCell.GetRoomCellGridPos().x + 1, (int)_roomCell.GetRoomCellGridPos().y,
                                _zone.GetZoneNumber(), roomNumber: num, teleportRoom: true, transitionLeft: true);
                            return true;

                        }
                    }
                    if (neighbours[3] == 0 && !roomSpawned)
                    {
                        if ((int)_roomCell.GetRoomCellGridPos().x - 1 > 0 &&
                                        arrayIntGrid[(int)_roomCell.GetRoomCellGridPos().x - 1,
                                        (int)_roomCell.GetRoomCellGridPos().y] == 0)
                        {
                            roomGen.SpawnNewRoom((int)_roomCell.GetRoomCellGridPos().x - 1, (int)_roomCell.GetRoomCellGridPos().y,
                                _zone.GetZoneNumber(), roomNumber: num, teleportRoom: true, transitionRight: true);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void GenerateZonesFirstRooms()
    {
        Transform zone1 = transform.Find("Zone " + zonesOrder[0]);
        List<GameObject> deadEnds = new List<GameObject>();

        foreach (Transform room in zone1)
        {
            if (room.GetComponent<Room>().GetNeighboursOpenDoors().Count == 1 && !room.GetChild(0).GetComponent<RoomCell>().IsBossRoom()
                && !room.GetChild(0).GetComponent<RoomCell>().IsTransitionRoom() && !room.GetChild(0).GetComponent<RoomCell>().IsSaveRoom()
                && !room.GetChild(0).GetComponent<RoomCell>().IsSecretRoom() && !room.GetChild(0).GetComponent<RoomCell>().IsTeleportRoom())
            {
                deadEnds.Add(room.gameObject);
            }
        }

        float minDistance = 9999999999;
        foreach (GameObject room in deadEnds)
        {
            if (Vector2.Distance(room.GetComponent<Room>().GetRoomCenterPos(), new Vector2(
                transform.Find("Pointers").Find("Pointer " + zonesOrder[0]).transform.position.x, 0)) < minDistance)
            {
                minDistance = Vector2.Distance(room.GetComponent<Room>().GetRoomCenterPos(), Vector2.zero);
                zone1.GetComponent<Zone>().SetFirstZoneRoom(room);
            }
        }

        for (int i = 1; i < zonesOrder.Count; i++)
        {
            GameObject actualZone = transform.Find("Zone " + zonesOrder[i]).gameObject;
            GameObject nextZone = null;
            if (i + 1 < zonesOrder.Count - 1)
                nextZone = transform.Find("Zone " + zonesOrder[i + 1]).gameObject;

            List<Room> possibleRooms = new List<Room>();

            foreach (Transform room in actualZone.transform)
            {
                for (int j = 0; j < room.GetComponent<Room>().GetNeighboursList().Count; j++)
                {
                    if (room.GetComponent<Room>().GetNeighboursList()[j].name == "Room 999")
                    {
                        possibleRooms.Add(room.GetComponent<Room>());
                    }
                }
            }

            if (possibleRooms.Count == 1)
            {
                actualZone.GetComponent<Zone>().SetFirstZoneRoom(possibleRooms[0].gameObject);
            }
            else if (nextZone != null)
            {
                possibleRooms = possibleRooms.OrderBy(
                       x => Vector2.Distance(transform.Find("Pointers").Find("Pointer " +
                       nextZone.GetComponent<Zone>().GetZoneNumber()).transform.position, x.GetRoomCenterPos())
                      ).ToList();

                possibleRooms.Reverse();

                actualZone.GetComponent<Zone>().SetFirstZoneRoom(possibleRooms[0].gameObject);
            }
            else if (nextZone == null)
            {
                possibleRooms = possibleRooms.OrderBy(
                       x => Vector2.Distance(transform.Find("Pointers").Find("Pointer " +
                       zonesOrder[i - 1]).transform.position, x.GetRoomCenterPos())
                      ).ToList();

                possibleRooms.Reverse();

                actualZone.GetComponent<Zone>().SetFirstZoneRoom(possibleRooms[0].gameObject);
            }
        }
    }

    private bool HasSpaceForARoom(Room _room)
    {
        foreach (Transform roomCell in _room.transform)
        {

            int[] neighbours = roomCell.GetComponent<RoomCell>().GetNeighboursList();

            if (neighbours[2] == 0)
            {
                if ((int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x + 1 < arrayIntGrid.GetLength(0) &&
                    arrayIntGrid[(int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x + 1,
                    (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y] == 0 &&
                    !_roomsPositionTaken.Contains(_room.GetRoomCenterPos()))
                {
                    return true;
                }
            }

            if (neighbours[3] == 0)
            {
                if ((int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x - 1 > 0 &&
                    arrayIntGrid[(int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x - 1,
                    (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y] == 0 &&
                    !_roomsPositionTaken.Contains(_room.GetRoomCenterPos()))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetRoomsBeforeBoss()
    {
        foreach (int zoneNumber in zonesOrder)
        {
            Zone _zone = transform.Find("Zone " + zoneNumber).GetComponent<Zone>();
            List<GameObject> roomList = new List<GameObject>();
            List<GameObject> beforeBossList = new List<GameObject>();
            Room bossRoom = null;
            foreach (Transform room in _zone.transform)
            {
                roomList.Add(room.gameObject);
                if (room.GetChild(0) != null && room.GetChild(0).GetComponent<RoomCell>().IsBossRoom())
                {
                    bossRoom = room.GetComponent<Room>();
                }
            }

            beforeBossList = routeGen.GenerateMinRoute(roomList, _zone.GetFirstZoneRoom(), bossRoom.gameObject);

            for (int i = 0; i < beforeBossList.Count; i++)
            {
                beforeBossList[i].GetComponent<Room>().SetIsBeforeBoss(true);
            }
        }
    }

    public void GenerateGateRooms()
    {
        if (transform.Find("Zone 0").childCount > 0)
        {
            foreach (Transform transitionRoom in transform.Find("Zone 0").GetChild(0))
            {
                Zone zone1 = null;
                Zone zone2 = null;
                Room room1 = null;
                Room room2 = null;

                for (int z = 0; z < transitionRoom.GetComponent<RoomCell>().GetNeighboursList().Length; z++)
                {
                    if (transitionRoom.GetComponent<RoomCell>().GetNeighboursList()[z] != 0 && zone1 == null)
                    {
                        zone1 = transform.Find("Zone " + transitionRoom.GetComponent<RoomCell>().GetZonesForTransitionRoom()[z]).GetComponent<Zone>();
                        room1 = zone1.transform.Find("Room " + transitionRoom.GetComponent<RoomCell>().GetNeighboursList()[z]).GetComponent<Room>();
                        continue;
                    }
                    if (transitionRoom.GetComponent<RoomCell>().GetNeighboursList()[z] != 0 && zone1 != null)
                    {
                        zone2 = transform.Find("Zone " + transitionRoom.GetComponent<RoomCell>().GetZonesForTransitionRoom()[z]).GetComponent<Zone>();
                        room2 = zone2.transform.Find("Room " + transitionRoom.GetComponent<RoomCell>().GetNeighboursList()[z]).GetComponent<Room>();
                        break;
                    }
                }

                GameObject deeperZone = null;
                Room deeperRoom = null;
                int zoneIndex = 0;

                if (zonesOrder.FindIndex(x => x == zone1.GetZoneNumber()) > zonesOrder.FindIndex(x => x == zone2.GetZoneNumber()))
                {
                    deeperZone = zone1.gameObject;
                    deeperRoom = room1.GetComponent<Room>();
                    zoneIndex = zonesOrder.FindIndex(x => x == zone1.GetZoneNumber());
                }
                else
                {
                    deeperZone = zone2.gameObject;
                    deeperRoom = room2.GetComponent<Room>();
                    zoneIndex = zonesOrder.FindIndex(x => x == zone2.GetZoneNumber());
                }

                foreach (Transform roomCell in deeperRoom.transform)
                {
                    roomCell.GetComponent<RoomCell>().SetGateRoom(true);
                }

                if (zoneIndex > 0)
                    deeperRoom.SetRoomIcon(lockIcon[zonesOrder[zoneIndex - 1] - 1]);
            }
        }
    }

    public void GenerateKeyRooms()
    {
        foreach (Transform zone in transform)
        {
            if (zone.tag == "Zones" && zone.name != "Zone 0" && zone.name != "Zone " + zonesOrder[zonesOrder.Count - 1])
            {
                foreach (Transform room in zone.transform)
                {
                    if (room.GetChild(0).GetComponent<RoomCell>().IsBossRoom())
                    {
                        Room _room = room.GetComponent<Room>();
                        if (_room.GetNeighboursOpenDoors().Count > 1)
                        {
                            if (!_room.GetNeighboursOpenDoors()[1].GetComponent<Room>().IsBeforeBoss() &&
                                _room.GetNeighboursOpenDoors()[1].GetComponent<Room>().GetNeighboursOpenDoors().Count == 1 &&
                                !_room.GetNeighboursOpenDoors()[1].transform.GetChild(0).GetComponent<RoomCell>().IsSaveRoom() &&
                                !_room.GetNeighboursOpenDoors()[1].transform.GetChild(0).GetComponent<RoomCell>().IsTransitionRoom())
                            {
                                _room.GetNeighboursOpenDoors()[1].GetComponent<Room>().SetRoomIcon(keyIcon[0]);
                            }
                            else
                            {
                                _room.SetRoomIcon(keyIcon[1]);
                            }
                        }
                        else
                        {
                            _room.SetRoomIcon(keyIcon[1]);
                        }
                    }
                }
            }
        }
    }

    public void GenerateTeleportRooms()
    {
        int randomPointer = Random.Range(0, 2);
        Transform mainPointer = transform.Find("Pointers").Find("Pointer " + zonesOrder[randomPointer]);
        Transform mainZone = transform.Find("Zone " + zonesOrder[randomPointer]);
        Zone secondTeleportZone = null;
        float maxDistance = 0f;

        if (mainPointer != null)
        {
            foreach (Transform pointer in transform.Find("Pointers"))
            {
                if (pointer.GetComponent<Pointers>().GetPointerNumber() != zonesOrder[1])
                {
                    if (Vector2.Distance(mainPointer.position, pointer.transform.position) > maxDistance)
                    {
                        maxDistance = Vector2.Distance(mainPointer.position, pointer.transform.position);
                        secondTeleportZone = transform.Find("Zone " + pointer.GetComponent<Pointers>().GetPointerNumber()).GetComponent<Zone>();
                    }
                }
            }
        }

        if (secondTeleportZone != null)
        {
            int rand = 0;
            do
            {
                do
                {
                    rand++;
                } while (!RoomSuitableForSaveRoom(secondTeleportZone.transform.GetChild(rand).GetComponent<Room>()) &&
                _roomsPositionTaken.Contains(secondTeleportZone.transform.GetChild(rand).GetComponent<Room>().GetRoomCenterPos()));
            }
            while (!SpawnTeleportRoom(secondTeleportZone.transform.GetChild(rand).gameObject, secondTeleportZone));

            rand = 0;

            do
            {
                do
                {
                    rand++;
                } while (!RoomSuitableForSaveRoom(mainZone.GetChild(rand).GetComponent<Room>()) &&
            _roomsPositionTaken.Contains(mainZone.GetChild(rand).GetComponent<Room>().GetRoomCenterPos()));
            }
            while (!SpawnTeleportRoom(mainZone.GetChild(rand).gameObject, mainZone.GetComponent<Zone>()));
        }

    }

    private Room GetLastRoom(Room _room)
    {

        if (_room.GetNeighboursOpenDoors().Count == 1)
        {
            return _room;
        }

        Room _neighbour = null;

        for (int i = 0; i < _room.GetNeighboursOpenDoors().Count; i++)
        {
            if (_room.GetNeighboursOpenDoors()[i].GetComponent<Room>().IsBeforeBoss() == false &&
                _room.GetNeighboursOpenDoors()[i] != _room.gameObject &&
                !_room.GetNeighboursOpenDoors()[i].transform.GetChild(0).GetComponent<RoomCell>().IsSaveRoom() &&
                !_room.GetNeighboursOpenDoors()[i].transform.GetChild(0).GetComponent<RoomCell>().IsTransitionRoom())
            {
                _neighbour = _room.GetNeighboursOpenDoors()[i].GetComponent<Room>();
            }
        }

        return GetLastRoom(_neighbour);
    }

    private bool IsRoomNumberAlreadyPicked(int num, int zoneNumber)
    {
        Transform zoneTransform = transform.Find("Zone " + zoneNumber);
        if (zoneTransform.Find("Room " + num) != null)
            return true;

        foreach (Transform room in transform)
        {
            if (room.GetComponent<RoomCell>())
            {
                if (room.GetComponent<RoomCell>().GetRoomNumber() == num &&
                    room.GetComponent<RoomCell>().GetZoneNumber() == zoneNumber)
                    return true;
            }
        }
        return false;
    }

    public void RemoveUnopenRooms()
    {
        foreach (Transform zone in transform)
        {
            if (zone.tag == "Zones" && zone.name != "Zone 0")
            {
                foreach (Transform room in zone.transform)
                {
                    if (room.GetComponent<Room>().GetNeighboursOpenDoors().Count == 0)
                    {
                        RoomCell _cell = room.GetChild(0).GetComponent<RoomCell>();

                        if (_cell.IsTransitionLeft())
                            _cell.OpenDoor(3, true);
                        else if (_cell.IsTransitionRight())
                            _cell.OpenDoor(2, true);

                        Transform _zone = transform.Find("Zone " + _cell.GetZoneNumber());

                        for (int j = 0; j < _cell.GetNeighboursList().Length; j++)
                        {
                            if (_cell.GetNeighboursList()[j] != 0)
                            {
                                foreach (Transform roomCell in _zone.Find("Room " + _cell.GetNeighboursList()[j]))
                                {
                                    for (int i = 0; i < roomCell.GetComponent<RoomCell>().GetNeighboursList().Length; i++)
                                    {
                                        if (roomCell.GetComponent<RoomCell>().GetNeighboursList()[i] == _cell.GetRoomNumber())
                                        {
                                            if (_cell.IsTransitionLeft())
                                                roomCell.GetComponent<RoomCell>().OpenDoor(2, true);
                                            else if (_cell.IsTransitionRight())
                                                roomCell.GetComponent<RoomCell>().OpenDoor(3, true);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Texture GetBossIcon()
    {
        return bossIcon;
    }

    public void Reset()
    {
        zonesOrder.Clear();
        zonesConnected.Clear();
        connectionsToIgnore.Clear();
        possibleSaveRoomsList.Clear();
        _roomsPositionTaken.Clear();
    }
}
