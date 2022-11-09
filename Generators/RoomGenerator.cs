using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;

    [Header("Generators")]
    private MapGenerator mapGen;
    private ZonesGenerator zoneGen;
    private RouteGenerator routeGen;
    private SpecialRoomsGenerator specialGen;
    private Grid grid;

    [Header("Zones")]
    private List<GameObject> zoneNodes;
    private int numberOfZones;

    [Header("Arrays")]
    private int[,] arrayIntGrid;
    private int[,] arrayColsHeight;
    private int[,] arrayZoneNumber;
    private bool[,] arrayBoolGrid;

    [Space]
    [SerializeField][Range(3, 10)] int minNumberofRoomsPerZone = 5;
    [SerializeField][Range(0, 16)] int maxSizePerRoom = 12;

    [Space]
    [SerializeField] bool showRoomNumbers;

    public void GenerateRooms()
    {
        Reset();

        grid = GetComponent<GridGenerator>().GetGrid();
        mapGen = GetComponent<MapGenerator>();
        zoneGen = GetComponent<ZonesGenerator>();
        routeGen = GetComponent<RouteGenerator>();
        specialGen = GetComponent<SpecialRoomsGenerator>();
        zoneNodes = zoneGen.GetNodeListWithZones();
        numberOfZones = zoneGen.GetNumberofZones();
        arrayIntGrid = new int[grid.GetCols(), grid.GetRows()];
        arrayColsHeight = new int[grid.GetCols(), grid.GetRows()];
        arrayZoneNumber = new int[grid.GetCols(), grid.GetRows()];
        arrayBoolGrid = new bool[grid.GetCols(), grid.GetRows()];

        FillArrays();

        SpawnRoomCells();
        OrganizeRoomsAndZones();
        CheckMinimumNumberOfRooms();

        SetRoomsNeighbours();

        routeGen.GenerateRoute();
    }

    void FillArrays()
    {
        for (int i = 0; i < numberOfZones; i++)
        {
            // For each Zone make a bool grid
            for (int j = 0; j < zoneNodes.Count; j++)
            {
                if (zoneNodes[j].GetComponent<Walker>().GetZoneNumber() == i + 1)
                {
                    arrayBoolGrid[
                        (int)grid.GetGridPosition(zoneNodes[j].transform.position.x, zoneNodes[j].transform.position.y).x,
                        (int)grid.GetGridPosition(zoneNodes[j].transform.position.x, zoneNodes[j].transform.position.y).y
                        ] = true;
                }
            }

            // Fill ArrayColsHeight
            arrayColsHeight = ColsHeightNumberMap(arrayBoolGrid);
            bool[,] _arrayBoolGrid = (bool[,])arrayBoolGrid.Clone();
            int roomNumber = 1;

            #region Fill Room Numbers Left to Right
            for (int x = 0; x < _arrayBoolGrid.GetLength(0); x++)
            {
                for (int y = 0; y < _arrayBoolGrid.GetLength(1); y++)
                {
                    if (_arrayBoolGrid[x, y])
                    {
                        #region If ColsHeight is 1
                        if (arrayColsHeight[x, y] == 1)
                        {
                            int rows = 0;
                            do
                            {
                                rows++;
                            } while (x + rows < arrayColsHeight.GetLength(0) && arrayColsHeight[x + rows, y] == 1);

                            for (int r = 0; r < rows; r++)
                            {
                                arrayIntGrid[x + r, y] = roomNumber;
                                _arrayBoolGrid[x + r, y] = false;
                            }
                            roomNumber++;
                            arrayColsHeight = ColsHeightNumberMap(_arrayBoolGrid);
                        }
                        #endregion

                        #region If the height of the col greatter than 1
                        else if (arrayColsHeight[x, y] > 1)
                        {
                            int rows = 0;
                            bool[] neighbourRows = new bool[arrayColsHeight[x, y]];
                            do
                            {
                                for (int r = 0; r < arrayColsHeight[x, y]; r++)
                                {
                                    if (x + rows < arrayColsHeight.GetLength(0) &&
                                        arrayColsHeight[x + rows, y + r] >= arrayColsHeight[x, y + r] &&
                                        arrayColsHeight[x, y] * rows < maxSizePerRoom)
                                    {
                                        neighbourRows[r] = true;
                                    }
                                    else
                                    {
                                        neighbourRows[r] = false;
                                    }
                                }
                                if (AreAllElementsTrue(neighbourRows))
                                    rows++;
                            } while (AreAllElementsTrue(neighbourRows));

                            //If the room only has 1 col skip it, else assign room number
                            for (int c = 0; c < arrayColsHeight[x, y]; c++)
                            {
                                for (int r = 0; r < rows; r++)
                                {
                                    if (rows > 1)
                                    {
                                        arrayIntGrid[x + r, y + c] = roomNumber;
                                        _arrayBoolGrid[x + r, y + c] = false;
                                    }
                                    else
                                    {
                                        arrayIntGrid[x + r, y + c] = roomNumber;
                                        _arrayBoolGrid[x + r, y + c] = true;

                                    }
                                }
                            }

                            // In case of skiped cols 
                            y += arrayColsHeight[x, y];
                            if (y == arrayIntGrid.GetLength(1))
                            {
                                y--;
                            }

                            roomNumber++;
                            arrayColsHeight = ColsHeightNumberMap(_arrayBoolGrid);
                        }
                        #endregion
                    }

                }
            }
            #endregion

            #region Fill Room Numbers Right to Left
            for (int x = _arrayBoolGrid.GetLength(0) - 1; x >= 0; x--)
            {
                for (int y = _arrayBoolGrid.GetLength(1) - 1; y >= 0; y--)
                {
                    if (_arrayBoolGrid[x, y])
                    {
                        #region If the height of the col greatter than 1
                        if (arrayColsHeight[x, y] > 1)
                        {
                            int rows = 0;
                            bool[] neighbourRows = new bool[arrayColsHeight[x, y]];
                            do
                            {
                                for (int r = arrayColsHeight[x, y] - 1; r >= 0; r--)
                                {
                                    if (x - rows >= 0 &&
                                        arrayColsHeight[x - rows, y - r] >= arrayColsHeight[x, y - r] &&
                                        arrayColsHeight[x, y] * rows < maxSizePerRoom)
                                    {
                                        neighbourRows[r] = true;
                                    }
                                    else
                                    {
                                        neighbourRows[r] = false;
                                    }
                                }
                                if (AreAllElementsTrue(neighbourRows))
                                    rows++;
                            } while (AreAllElementsTrue(neighbourRows));


                            for (int c = arrayColsHeight[x, y] - 1; c >= 0; c--)
                            {
                                for (int r = rows - 1; r >= 0; r--)
                                {
                                    arrayIntGrid[x - r, y - c] = roomNumber;
                                    _arrayBoolGrid[x - r, y - c] = false;
                                }
                            }

                            roomNumber++;
                            arrayColsHeight = ColsHeightNumberMap(_arrayBoolGrid);
                        }
                        #endregion
                    }
                }
            }
            #endregion

            // Make a map of each node zone number
            for (int z = 0; z < arrayBoolGrid.GetLength(0); z++)
            {
                for (int s = 0; s < arrayBoolGrid.GetLength(1); s++)
                {
                    if (arrayBoolGrid[z, s])
                    {
                        arrayZoneNumber[z, s] = i + 1;
                    }
                }
            }

            arrayBoolGrid = new bool[grid.GetCols(), grid.GetRows()];
        }
    }

    void SpawnRoomCells()
    {
        GameObject room;
        if (arrayIntGrid != null)
        {
            for (int x = 0; x < arrayIntGrid.GetLength(0); x++)
            {
                for (int y = 0; y < arrayIntGrid.GetLength(1); y++)
                {
                    if (arrayIntGrid[x, y] != 0)
                    {
                        room = Instantiate(
                            roomPrefab, new Vector3(grid.GetWorldPosition(x, y).x - 4.5f,
                            grid.GetWorldPosition(x, y).y - 4.5f), Quaternion.identity, gameObject.transform
                            );

                        Color color = new Color(
                            zoneGen.GetZoneColorArray()[arrayZoneNumber[x, y] - 1].r,
                            zoneGen.GetZoneColorArray()[arrayZoneNumber[x, y] - 1].g,
                            zoneGen.GetZoneColorArray()[arrayZoneNumber[x, y] - 1].b,
                            zoneGen.GetRoomAlphaColor());

                        room.GetComponent<RoomCell>().SetColor(color);
                        room.GetComponent<RoomCell>().SetZoneNumber(arrayZoneNumber[x, y]);
                        room.GetComponent<RoomCell>().SetRoomNumber(arrayIntGrid[x, y]);

                        UpdateRoomsWallsDoorsAndNeighbours(x, y, room);
                    }
                }
            }
        }
    }

    private void OrganizeRoomsAndZones()
    {
        CreateZones();

        CreateRooms();

        ResetRoomNeighbours();
    }

    private void CreateZones()
    {
        for (int j = 0; j <= zoneGen.GetNumberofZones(); j++)
        {
            if (transform.Find("Zone " + j) == null)
            {
                GameObject goZone = new GameObject();
                goZone.name = "Zone " + j;
                goZone.transform.parent = gameObject.transform;
                goZone.transform.tag = "Zones";
                goZone.AddComponent<Zone>();
                goZone.GetComponent<Zone>().SetZoneNumber(j);
            }
        }
    }

    private void CreateRooms()
    {
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).gameObject.GetComponent<RoomCell>())
            {
                GameObject goZone = GameObject.Find("Zone " + transform.GetChild(i).gameObject.GetComponent<RoomCell>().GetZoneNumber());
                Transform goRoom = goZone.transform.Find("Room " + transform.GetChild(i).gameObject.GetComponent<RoomCell>().GetRoomNumber());

                if (goRoom != null)
                {
                    transform.GetChild(i).gameObject.transform.parent = goRoom;
                }
                else
                {
                    GameObject go = new GameObject();
                    go.name = "Room " + (transform.GetChild(i).gameObject.GetComponent<RoomCell>().GetRoomNumber());
                    go.transform.parent = goZone.transform;
                    
                    if (transform.GetChild(i).gameObject.GetComponent<RoomCell>().IsSaveRoom() ||
                        transform.GetChild(i).gameObject.GetComponent<RoomCell>().IsTeleportRoom())
                    {
                        go.transform.tag = "SpecialRooms";
                    }
                    else
                    {
                        go.transform.tag = "Rooms";
                    }
                    
                    go.AddComponent<Room>();
                    go.GetComponent<Room>().SetRoomNumber(transform.GetChild(i).gameObject.GetComponent<RoomCell>().GetRoomNumber());
                    if (transform.GetChild(i).gameObject.GetComponent<RoomCell>().IsBossRoom())
                        go.GetComponent<Room>().SetRoomIcon(specialGen.GetBossIcon());
                    transform.GetChild(i).gameObject.transform.parent = go.transform;
                }
            }
        }
    }

    public GameObject SpawnNewRoom(int _x, int _y, int zoneNumber, int roomNumber = 0, bool saveRoom = false,
        bool bossRoom = false, bool transitionRoom = false, bool transitionUp = false, bool transitionDown = false,
        bool transitionRight = false, bool transitionLeft = false, bool gateRoom = false, bool teleportRoom = false)
    {
        Vector3 pos = new Vector3(grid.GetWorldPosition(_x, _y).x - 4.5f, grid.GetWorldPosition(_x, _y).y - 4.5f);
        GameObject roomCell = Instantiate(roomPrefab, pos, Quaternion.identity, transform);

        Color color;
        if (saveRoom)
            color = Color.red;
        else if (transitionRoom)
        {
            color = Color.green;
            roomNumber = 999;
            zoneNumber = 0;
        }
        else if (teleportRoom)
            color = Color.blue;
        else
        {
            color = new Color(
                zoneGen.GetZoneColorArray()[zoneNumber - 1].r,
                zoneGen.GetZoneColorArray()[zoneNumber - 1].g,
                zoneGen.GetZoneColorArray()[zoneNumber - 1].b,
                zoneGen.GetRoomAlphaColor());

        }

        if (roomNumber == 0)
        {
            int num = 0;

            do
            {
                num++;
            } while (transform.Find("Zone " + zoneNumber).Find("Room " + num));

            roomNumber = num;
        }

        roomCell.GetComponent<RoomCell>().SetColor(color);
        roomCell.GetComponent<RoomCell>().SetZoneNumber(zoneNumber);
        roomCell.GetComponent<RoomCell>().SetRoomNumber(roomNumber);
        roomCell.GetComponent<RoomCell>().SetTransitionRoom(transitionRoom);
        roomCell.GetComponent<RoomCell>().SetTransitionUp(transitionUp);
        roomCell.GetComponent<RoomCell>().SetTransitionDown(transitionDown);
        roomCell.GetComponent<RoomCell>().SetTransitionRight(transitionRight);
        roomCell.GetComponent<RoomCell>().SetTransitionLeft(transitionLeft);
        roomCell.GetComponent<RoomCell>().SetSaveRoom(saveRoom);
        roomCell.GetComponent<RoomCell>().SetTeleportRoom(teleportRoom);
        roomCell.GetComponent<RoomCell>().SetBossRoom(bossRoom);
        roomCell.GetComponent<RoomCell>().SetGateRoom(gateRoom);

        return roomCell;
    }


    private int[,] ColsHeightNumberMap(bool[,] _boolMap)
    {
        int[,] _arrayColsHeight = new int[_boolMap.GetLength(0), _boolMap.GetLength(1)];

        for (int x = 0; x < _boolMap.GetLength(0); x++)
        {
            for (int y = 0; y < _boolMap.GetLength(1); y++)
            {
                if (_boolMap[x, y])
                {
                    int height = 0;
                    do
                    {
                        height++;
                    } while (y + height < _boolMap.GetLength(1) && _boolMap[x, y + height]);

                    for (int h = 0; h < height; h++)
                    {
                        _arrayColsHeight[x, y + h] = height;
                    }
                    y += height - 1;
                }

            }
        }
        return _arrayColsHeight;
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

    void SetRoomsNeighbours()
    {
        foreach (Transform zone in transform)
        {
            if (zone.tag == "Zones")
            {
                foreach (Transform room in zone.transform)
                {
                    if (room.tag == "Rooms")
                    {
                        foreach (Transform roomCell in room.transform)
                        {
                            if (roomCell.tag == "RoomCell")
                            {
                                // Look if we have neighbours of each roomCell
                                for (int i = 0; i < roomCell.GetComponent<RoomCell>().GetNeighboursList().Length; i++)
                                {
                                    if (roomCell.GetComponent<RoomCell>().GetNeighboursList()[i] != 0)
                                    {
                                        // If we have neighbours we will look if that neighbour is already register,
                                        // if not, we will add it
                                        if (room.GetComponent<Room>().GetNeighboursList().Count == 0)
                                        {
                                            if (roomCell.GetComponent<RoomCell>().IsTransitionRoom())
                                            {
                                                for (int z = 0; z < roomCell.GetComponent<RoomCell>().GetZonesForTransitionRoom().Length; z++)
                                                {
                                                    Transform _zone = transform.Find("Zone " + roomCell.GetComponent<RoomCell>().GetZonesForTransitionRoom()[z]);
                                                    if (roomCell.GetComponent<RoomCell>().GetNeighboursList()[z] != 0)
                                                        room.GetComponent<Room>().AddNeighbour(_zone.Find("Room " +
                                                        roomCell.GetComponent<RoomCell>().GetNeighboursList()[z]).gameObject);
                                                }
                                            }
                                            else
                                            {
                                                room.GetComponent<Room>().AddNeighbour(zone.Find("Room " +
                                                    roomCell.GetComponent<RoomCell>().GetNeighboursList()[i]).gameObject);
                                            }
                                        }
                                        else if (room.GetComponent<Room>().GetNeighboursList().Count > 0)
                                        {
                                            bool neighbourIsAlreadyIn = false;
                                            for (int j = 0; j < room.GetComponent<Room>().GetNeighboursList().Count; j++)
                                            {
                                                if (room.GetComponent<Room>().GetNeighboursList()[j].name ==
                                                    "Room " + roomCell.GetComponent<RoomCell>().GetNeighboursList()[i])
                                                {
                                                    neighbourIsAlreadyIn = true;
                                                }
                                            }
                                            if (!neighbourIsAlreadyIn)
                                            {
                                                if (roomCell.GetComponent<RoomCell>().IsTransitionRoom())
                                                {
                                                    for (int z = 0; z < roomCell.GetComponent<RoomCell>().GetZonesForTransitionRoom().Length; z++)
                                                    {
                                                        Transform _zone = transform.Find("Zone " + roomCell.GetComponent<RoomCell>().GetZonesForTransitionRoom()[z]);
                                                        if (zone.Find("Room " + roomCell.GetComponent<RoomCell>().GetNeighboursList()[z]) != null)
                                                            room.GetComponent<Room>().AddNeighbour(_zone.Find("Room " +
                                                            roomCell.GetComponent<RoomCell>().GetNeighboursList()[z]).gameObject);
                                                    }
                                                }
                                                else
                                                {
                                                    if (roomCell.GetComponent<RoomCell>().GetNeighboursList()[i] == 999)
                                                    {
                                                        int _zoneNumber = 0;
                                                        if ((int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x + 1 < grid.GetRows() &&
                                                            arrayIntGrid[(int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x + 1,
                                                            (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y] == 999)
                                                        {
                                                            _zoneNumber = arrayZoneNumber[(int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x + 1,
                                                            (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y];
                                                        }
                                                        else if ((int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x - 1 > 0 &&
                                                            arrayIntGrid[(int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x - 1,
                                                           (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y] == 999)
                                                        {
                                                            _zoneNumber = arrayZoneNumber[(int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().x - 1,
                                                            (int)roomCell.GetComponent<RoomCell>().GetRoomCellGridPos().y];
                                                        }
                                                        Transform _zone = transform.Find("Zone " + _zoneNumber);
                                                        if (_zone != null)
                                                        {
                                                            room.GetComponent<Room>().AddNeighbour(_zone.Find("Room " +
                                                        roomCell.GetComponent<RoomCell>().GetNeighboursList()[i]).gameObject);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Transform _room = zone.Find("Room " + roomCell.GetComponent<RoomCell>().GetNeighboursList()[i]);
                                                        if (_room != null)
                                                            room.GetComponent<Room>().AddNeighbour(zone.Find("Room " +
                                                            roomCell.GetComponent<RoomCell>().GetNeighboursList()[i]).gameObject);
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
            }
        }
    }

    void CheckMinimumNumberOfRooms()
    {
        int childs = transform.childCount;
        childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).tag == "Zones" && transform.GetChild(i).name != "Zone 0")
            {
                if (transform.GetChild(i).childCount < minNumberofRoomsPerZone)
                {
                    zoneGen.GenerateZones();
                }
            }
        }
    }

    void UpdateRoomsWallsDoorsAndNeighbours(int _x, int _y, GameObject _room)
    {
        #region Check up
        if (_y + 1 < arrayIntGrid.GetLength(1))
        {
            if (arrayIntGrid[_x, _y + 1] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x, _y + 1] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(0, true);
            }
            else if (arrayIntGrid[_x, _y + 1] != arrayIntGrid[_x, _y] && arrayZoneNumber[_x, _y + 1] == arrayZoneNumber[_x, _y])
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[0] != arrayIntGrid[_x, _y + 1])
                    _room.GetComponent<RoomCell>().AddNeighbour(0, arrayIntGrid[_x, _y + 1]);
            }
            else if (arrayIntGrid[_x, _y + 1] != 0 && arrayZoneNumber[_x, _y + 1] != arrayZoneNumber[_x, _y])
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x, _y + 1]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x, _y + 1]);
                    }
                }
            }
        }
        #endregion

        #region Check down
        if (_y > 0)
        {
            if (arrayIntGrid[_x, _y - 1] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x, _y - 1] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(1, true);
            }
            else if (arrayIntGrid[_x, _y - 1] != arrayIntGrid[_x, _y] && arrayZoneNumber[_x, _y - 1] == arrayZoneNumber[_x, _y])
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[1] != arrayIntGrid[_x, _y - 1])
                    _room.GetComponent<RoomCell>().AddNeighbour(1, arrayIntGrid[_x, _y - 1]);
            }
            else if (arrayIntGrid[_x, _y - 1] != 0 && arrayZoneNumber[_x, _y - 1] != arrayZoneNumber[_x, _y])
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x, _y - 1]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x, _y - 1]);
                    }
                }
            }
        }
        #endregion

        #region Check right
        if (_x + 1 < arrayIntGrid.GetLength(0))
        {
            if (arrayIntGrid[_x + 1, _y] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x + 1, _y] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(2, true);
            }
            else if (arrayIntGrid[_x + 1, _y] != arrayIntGrid[_x, _y] && arrayZoneNumber[_x + 1, _y] == arrayZoneNumber[_x, _y])
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[2] != arrayIntGrid[_x + 1, _y])
                    _room.GetComponent<RoomCell>().AddNeighbour(2, arrayIntGrid[_x + 1, _y]);
            }
            else if (arrayIntGrid[_x + 1, _y] != 0 && arrayZoneNumber[_x + 1, _y] != arrayZoneNumber[_x, _y])
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x + 1, _y]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x + 1, _y]);
                    }
                }
            }
        }
        #endregion

        #region Check left
        if (_x > 0)
        {
            if (arrayIntGrid[_x - 1, _y] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x - 1, _y] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(3, true);
            }
            else if (arrayIntGrid[_x - 1, _y] != arrayIntGrid[_x, _y] && arrayZoneNumber[_x - 1, _y] == arrayZoneNumber[_x, _y])
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[3] != arrayIntGrid[_x - 1, _y])
                    _room.GetComponent<RoomCell>().AddNeighbour(3, arrayIntGrid[_x - 1, _y]);
            }
            else if (arrayIntGrid[_x - 1, _y] != 0 && arrayZoneNumber[_x - 1, _y] != arrayZoneNumber[_x, _y])
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x - 1, _y]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x - 1, _y]);
                    }
                }
            }
        }
        #endregion
    }

    void UpdateRoomsWallsDoorsAndNeighbours(GameObject _room)
    {
        grid = GetComponent<GridGenerator>().GetGrid();
        int _x = (int)grid.GetGridPosition(_room.transform.position.x, _room.transform.position.y).x;
        int _y = (int)grid.GetGridPosition(_room.transform.position.x, _room.transform.position.y).y;
        arrayIntGrid[_x, _y] = _room.GetComponent<RoomCell>().GetRoomNumber();
        arrayZoneNumber[_x, _y] = _room.GetComponent<RoomCell>().GetZoneNumber();

        #region Check up
        if (_y + 1 < arrayIntGrid.GetLength(1))
        {
            if (arrayIntGrid[_x, _y + 1] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x, _y + 1] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(0, true);
            }
            else if (arrayIntGrid[_x, _y + 1] != arrayIntGrid[_x, _y] &&
                arrayZoneNumber[_x, _y + 1] == arrayZoneNumber[_x, _y] && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x, _y + 1] == 999 && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x, _y] == 999 && arrayZoneNumber[_x, _y + 1] > 0 &&
                _room.GetComponent<RoomCell>().IsTransitionUp())
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[0] != arrayIntGrid[_x, _y + 1])
                {
                    if (!_room.GetComponent<RoomCell>().IsSaveRoom() &&
                       !_room.GetComponent<RoomCell>().IsTeleportRoom() &&
                       !_room.GetComponent<RoomCell>().IsSecretRoom())
                    {
                        _room.GetComponent<RoomCell>().AddNeighbour(0, arrayIntGrid[_x, _y + 1]);
                    }

                    if (arrayIntGrid[_x, _y] == 999)
                        _room.GetComponent<RoomCell>().SetZonesForTransitionRoom(0, arrayZoneNumber[_x, _y + 1]);
                }
            }
            else if (arrayIntGrid[_x, _y + 1] != 0 && arrayZoneNumber[_x, _y + 1] != arrayZoneNumber[_x, _y] &&
                arrayIntGrid[_x, _y] != 999)
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x, _y + 1]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x, _y + 1]);
                    }
                }
            }

        }
        #endregion

        #region Check down
        if (_y > 0)
        {
            if (arrayIntGrid[_x, _y - 1] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x, _y - 1] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(1, true);
            }
            else if (arrayIntGrid[_x, _y - 1] != arrayIntGrid[_x, _y] &&
                arrayZoneNumber[_x, _y - 1] == arrayZoneNumber[_x, _y] && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x, _y - 1] == 999 && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x, _y] == 999 && arrayZoneNumber[_x, _y - 1] > 0 &&
                _room.GetComponent<RoomCell>().IsTransitionDown())
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[1] != arrayIntGrid[_x, _y - 1])
                {
                    if (!_room.GetComponent<RoomCell>().IsSaveRoom() &&
                       !_room.GetComponent<RoomCell>().IsTeleportRoom() &&
                       !_room.GetComponent<RoomCell>().IsSecretRoom())
                    {
                        _room.GetComponent<RoomCell>().AddNeighbour(1, arrayIntGrid[_x, _y - 1]);
                    }

                    if (arrayIntGrid[_x, _y] == 999)
                        _room.GetComponent<RoomCell>().SetZonesForTransitionRoom(1, arrayZoneNumber[_x, _y - 1]);
                }
            }
            else if (arrayIntGrid[_x, _y - 1] != 0 && arrayZoneNumber[_x, _y - 1] != arrayZoneNumber[_x, _y] && arrayIntGrid[_x, _y] != 999)
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x, _y - 1]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x, _y - 1]);
                    }
                }
            }
        }
        #endregion

        #region Check right
        if (_x + 1 < arrayIntGrid.GetLength(0))
        {
            if (arrayIntGrid[_x + 1, _y] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x + 1, _y] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(2, true);
            }
            else if ((arrayIntGrid[_x + 1, _y] != arrayIntGrid[_x, _y] &&
                arrayZoneNumber[_x + 1, _y] == arrayZoneNumber[_x, _y]) && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x + 1, _y] == 999 && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x, _y] == 999 && arrayZoneNumber[_x + 1, _y] > 0 &&
                _room.GetComponent<RoomCell>().IsTransitionRight())
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[2] != arrayIntGrid[_x + 1, _y])
                {
                    if ((!_room.GetComponent<RoomCell>().IsSaveRoom() &&
                       !_room.GetComponent<RoomCell>().IsTeleportRoom() &&
                       !_room.GetComponent<RoomCell>().IsSecretRoom()) ||
                       (_room.GetComponent<RoomCell>().IsTransitionRight() &&
                       _room.GetComponent<RoomCell>().IsSaveRoom() ||
                       _room.GetComponent<RoomCell>().IsTransitionRight() &&
                       _room.GetComponent<RoomCell>().IsTeleportRoom() ||
                       _room.GetComponent<RoomCell>().IsSecretRoom()))
                    {
                        _room.GetComponent<RoomCell>().AddNeighbour(2, arrayIntGrid[_x + 1, _y]);
                    }

                    if (arrayIntGrid[_x, _y] == 999)
                        _room.GetComponent<RoomCell>().SetZonesForTransitionRoom(2, arrayZoneNumber[_x + 1, _y]);
                }
            }
            else if (arrayIntGrid[_x + 1, _y] != 0 && arrayZoneNumber[_x + 1, _y] != arrayZoneNumber[_x, _y] &&
                arrayIntGrid[_x, _y] != 999)
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x + 1, _y]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x + 1, _y]);
                    }
                }
            }
        }
        #endregion

        #region Check left
        if (_x > 0)
        {
            if (arrayIntGrid[_x - 1, _y] == arrayIntGrid[_x, _y] && arrayZoneNumber[_x - 1, _y] == arrayZoneNumber[_x, _y])
            {
                _room.GetComponent<RoomCell>().UpdateWallDoor(3, true);
            }
            else if ((arrayIntGrid[_x - 1, _y] != arrayIntGrid[_x, _y] &&
                arrayZoneNumber[_x - 1, _y] == arrayZoneNumber[_x, _y]) && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x - 1, _y] == 999 && arrayZoneNumber[_x, _y] > 0 ||
                arrayIntGrid[_x, _y] == 999 && arrayZoneNumber[_x - 1, _y] > 0 &&
                _room.GetComponent<RoomCell>().IsTransitionLeft())
            {
                if (_room.GetComponent<RoomCell>().GetNeighboursList()[3] != arrayIntGrid[_x - 1, _y])
                {
                    if ((!_room.GetComponent<RoomCell>().IsSaveRoom() &&
                      !_room.GetComponent<RoomCell>().IsTeleportRoom() &&
                      !_room.GetComponent<RoomCell>().IsSecretRoom()) ||
                      (_room.GetComponent<RoomCell>().IsTransitionLeft() &&
                      _room.GetComponent<RoomCell>().IsSaveRoom() ||
                      _room.GetComponent<RoomCell>().IsTransitionLeft() &&
                      _room.GetComponent<RoomCell>().IsTeleportRoom() ||
                      _room.GetComponent<RoomCell>().IsSecretRoom()))
                    {
                        _room.GetComponent<RoomCell>().AddNeighbour(3, arrayIntGrid[_x - 1, _y]);
                    }

                    if (arrayIntGrid[_x, _y] == 999)
                        _room.GetComponent<RoomCell>().SetZonesForTransitionRoom(3, arrayZoneNumber[_x - 1, _y]);
                }
            }
            else if (arrayIntGrid[_x - 1, _y] != 0 && arrayZoneNumber[_x - 1, _y] != arrayZoneNumber[_x, _y] && arrayIntGrid[_x, _y] != 999)
            {
                Transform pointerList = transform.Find("Pointers");
                if (pointerList != null)
                {
                    Pointers _pointer = pointerList.Find("Pointer " + arrayZoneNumber[_x, _y]).GetComponent<Pointers>();
                    if (_pointer != null)
                    {
                        if (!_pointer.GetZoneNieghbours().Contains(arrayZoneNumber[_x - 1, _y]))
                            _pointer.AddZoneNeighbour(arrayZoneNumber[_x - 1, _y]);
                    }
                }
            }
        }
        #endregion
    }

    void UpdateNeighboursRooms(GameObject _room)
    {
        if (_room.GetComponent<RoomCell>().IsTransitionRoom())
        {
            for (int i = 0; i < _room.GetComponent<RoomCell>().GetZonesForTransitionRoom().Length; i++)
            {
                Transform goZone = gameObject.transform.Find("Zone " + _room.GetComponent<RoomCell>().GetZonesForTransitionRoom()[i]);
                if (goZone != null && goZone.name != "Zone 0")
                {
                    int neighbour = _room.GetComponent<RoomCell>().GetNeighboursList()[i];

                    Transform neighbourRoom = goZone.Find("Room " + neighbour);
                    if (neighbourRoom != null)
                    {
                        foreach (Transform child in neighbourRoom)
                        {
                            UpdateRoomsWallsDoorsAndNeighbours(child.gameObject);
                        }
                    }
                }
            }
        }
        else
        {
            Transform goZone = gameObject.transform.Find("Zone " + _room.GetComponent<RoomCell>().GetZoneNumber());
            if (goZone != null)
            {
                foreach (int neighbour in _room.GetComponent<RoomCell>().GetNeighboursList())
                {
                    Transform neighbourRoom = goZone.Find("Room " + neighbour);
                    if (neighbourRoom != null)
                    {
                        foreach (Transform child in neighbourRoom)
                        {
                            UpdateRoomsWallsDoorsAndNeighbours(child.gameObject);
                        }
                    }
                }
                Transform goRoom = goZone.Find("Room " + _room.GetComponent<RoomCell>().GetRoomNumber());
                if (goRoom != null)
                {
                    foreach (Transform child in goRoom)
                    {
                        UpdateRoomsWallsDoorsAndNeighbours(child.gameObject);
                    }
                }
            }
        }
    }

    void CloseAllDoors()
    {
        foreach (Transform zone in transform)
        {
            foreach (Transform room in zone.transform)
            {
                foreach (Transform roomCell in room.transform)
                {
                    if (roomCell.tag == "RoomCell")
                    {
                        roomCell.GetComponent<RoomCell>().UpdateWallDoor(false, false, false, false);
                        UpdateRoomsWallsDoorsAndNeighbours(roomCell.gameObject);
                    }
                }
            }
        }
    }

    public void RestructureRooms()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<RoomCell>())
            {
                UpdateRoomsWallsDoorsAndNeighbours(child.gameObject);
                UpdateNeighboursRooms(child.gameObject);
            }
        }
        OrganizeRoomsAndZones();
        SetRoomsNeighbours();
        CloseAllDoors();
        routeGen.GenerateRoute();
    }

    public GameObject GetRoomPrefab()
    {
        return roomPrefab;
    }

    public int[,] GetArrayIntGrid()
    {
        return arrayIntGrid;
    }

    public int[,] GetArrayZoneGrid()
    {
        return arrayZoneNumber;
    }

    private void ResetRoomNeighbours()
    {
        foreach (Transform zone in transform)
        {
            if (zone.tag == "Zones" && zone.name != "Zone 0")
            {
                foreach (Transform room in zone)
                {
                    room.GetComponent<Room>().ClearNeighboursOpenDoors();
                    room.GetComponent<Room>().CalculateWidthAndHeight();
                }
            }
        }
    }

    public void Reset()
    {
        arrayIntGrid = null;
        arrayZoneNumber = null;
        arrayBoolGrid = null;

        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).tag == "Zones")
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (arrayIntGrid != null && grid != null && showRoomNumbers)
        {
            for (int i = 0; i < arrayIntGrid.GetLength(0); i++)
            {
                for (int j = 0; j < arrayIntGrid.GetLength(1); j++)
                {
                    Gizmos.color = Color.black;
                    Handles.Label(grid.GetWorldPosition(i, j) + new Vector3(-2f, 3f), arrayIntGrid[i, j].ToString());
                }
            }
        }

    }
}
