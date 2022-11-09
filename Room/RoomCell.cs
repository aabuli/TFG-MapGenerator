using UnityEngine;

public class RoomCell : MonoBehaviour
{
    [SerializeField] private int zoneNumber, roomNumber;
    [Space]
    private int[] neighbours = new int[4];
    [SerializeField] private bool saveRoom, teleportRoom, transitionRoom, bossRoom, gateRoom, secretRoom;
    [SerializeField] private bool transitionUp, transitionDown, transitionRight, transitionLeft;
    private int[] transitionZones = new int[4];
    private SpriteRenderer sprite;
    private Grid grid;

    [Header("WallBehaviour")]
    [SerializeField] private GameObject[] middleDoor;
    [SerializeField] private GameObject[] middleWall;
    [SerializeField] private GameObject[] walls;
    private bool[] doors = new bool[4];    //Direction of the doors -> { up, down, right, left };

    private void Awake()
    {
        grid = FindObjectOfType<GridGenerator>().GetGrid();
        sprite = gameObject.transform.Find("ColorBackground").GetComponentInChildren<SpriteRenderer>();
    }

    #region Getters
    public Vector3 GetRoomCellGridPos()
    {
        if (grid == null)
            grid = FindObjectOfType<GridGenerator>().GetGrid();
        return new Vector3(
            grid.GetGridPosition(transform.position.x, transform.position.y).x,
            grid.GetGridPosition(transform.position.x, transform.position.y).y
            );
    }
    public Color GetColor()
    {
        sprite = gameObject.transform.Find("ColorBackground").GetComponentInChildren<SpriteRenderer>();
        return sprite.color;
    }
    public int GetZoneNumber()
    {
        return zoneNumber;
    }
    public int GetRoomNumber()
    {
        return roomNumber;
    }
    public int[] GetNeighboursList()
    {
        if (neighbours.Length == 0)
            neighbours = new int[4];
        return neighbours;
    }
    public GameObject GetWalker()
    {
        Collider2D[] colliders;
        GameObject go = null;
        GameObject walkersList = FindObjectOfType<MapGenerator>().transform.Find("Walkers").gameObject;
        walkersList.SetActive(true);

        if ((colliders = Physics2D.OverlapCircleAll(transform.position + new Vector3(4.5f, 4.5f), 2f)).Length > 1)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject)
                    go = collider.gameObject;
            }
        }
        walkersList.SetActive(false);

        return go;
    }
    public int GetNumberOfDoorsOpen()
    {
        int num = 0;

        foreach (Transform wall in transform.Find("Walls"))
        {
            if (wall.gameObject.activeSelf)
            {
                if (wall.Find("Doors").GetChild(0).gameObject.activeSelf && !wall.Find("Doors").GetChild(1).gameObject.activeSelf)
                    num++;
            }
        }

        return num;
    }
    public int[] GetZonesForTransitionRoom()
    {
        return transitionZones;
    }
    public bool IsTransitionRoom()
    {
        return transitionRoom;
    }
    public bool IsTransitionUp()
    {
        return transitionUp;
    }
    public bool IsTransitionDown()
    {
        return transitionDown;
    }
    public bool IsTransitionRight()
    {
        return transitionRight;
    }
    public bool IsTransitionLeft()
    {
        return transitionLeft;
    }
    public bool IsSaveRoom()
    {
        return saveRoom;
    }
    public bool IsTeleportRoom()
    {
        return teleportRoom;
    }
    public bool IsBossRoom()
    {
        return bossRoom;
    }
    public bool IsGateRoom()
    {
        return gateRoom;
    }
    public bool IsSecretRoom()
    {
        return secretRoom;
    }
    #endregion

    #region Setters
    public void SetColor(Color _color)
    {
        sprite = gameObject.transform.Find("ColorBackground").GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
            sprite.color = _color;
    }
    public void SetZoneNumber(int _num)
    {
        zoneNumber = _num;
    }
    public void SetRoomNumber(int _num)
    {
        roomNumber = _num;
    }
    public void SetZonesForTransitionRoom(int _pos, int _num)
    {
        transitionZones[_pos] = _num;
    }
    public void SetSecretRoom(bool _b)
    {
        secretRoom = _b;
    }
    public void SetTransitionRoom(bool _b)
    {
        transitionRoom = _b;
    }
    public void SetTransitionUp(bool _b)
    {
        transitionUp = _b;
    }
    public void SetTransitionDown(bool _b)
    {
        transitionDown = _b;
    }
    public void SetTransitionRight(bool _b)
    {
        transitionRight = _b;
    }
    public void SetTransitionLeft(bool _b)
    {
        transitionLeft = _b;
    }
    public void SetSaveRoom(bool _b)
    {
        saveRoom = _b;
    }
    public void SetTeleportRoom(bool _b)
    {
        teleportRoom = _b;
    }
    public void SetBossRoom(bool _b)
    {
        bossRoom = _b;
    }
    public void SetGateRoom(bool _b)
    {
        gateRoom = _b;
    }

    #endregion

    public void AddNeighbour(int _pos, int _neighbour)
    {
        neighbours[_pos] = _neighbour;
    }

    public void RemoveNeighbour(int _pos)
    {
        neighbours[_pos] = 0;
    }

    public void UpdateWallDoor(bool up = false, bool down = false, bool right = false, bool left = false)
    {
        doors[0] = up;
        doors[1] = down;
        doors[2] = right;
        doors[3] = left;

        for (int i = 0; i < doors.Length; i++)
        {
            middleDoor[i].SetActive(doors[i]);
            middleWall[i].SetActive(!doors[i]);
            walls[i].SetActive(!doors[i]);
        }
    }

    public void UpdateWallDoor(int doorNum, bool isOpen)
    {
        doors[doorNum] = isOpen;

        for (int i = 0; i < doors.Length; i++)
        {
            middleDoor[i].SetActive(doors[i]);
            middleWall[i].SetActive(!doors[i]);
            walls[i].SetActive(!doors[i]);
        }
    }

    public void OpenDoor(int doorNum, bool isOpen)
    {
        doors[doorNum] = isOpen;

        middleDoor[doorNum].SetActive(doors[doorNum]);
        middleWall[doorNum].SetActive(!doors[doorNum]);
    }

    public void UncheckSpecialRoomFlags()
    {
        saveRoom = false;
        teleportRoom = false;
        transitionRoom = false;
        bossRoom = false;
        gateRoom = false;
        secretRoom = false;
        transitionUp = false;
        transitionDown = false;
        transitionRight = false;
        transitionLeft = false;
    }

}
