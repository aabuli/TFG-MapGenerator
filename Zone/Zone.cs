using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    private int zoneNumber = 0;
    [SerializeField] private GameObject firstZoneRoom = null;
    [SerializeField] private List<GameObject> routeRoomsList = new List<GameObject>();

    public void SetZoneNumber(int _n)
    {
        zoneNumber = _n;
    }

    public void SetRouteRoomList(List<GameObject> _list)
    {
        routeRoomsList = _list;
    }

    public void AddRouteRoom(GameObject _node)
    {
        routeRoomsList.Add(_node);
    }

    public void RemoveRouteRoom(GameObject _node)
    {
        routeRoomsList.Remove(_node);
    }

    public int GetZoneNumber()
    {
        return zoneNumber;
    }

    public List<GameObject> GetRouteRoomList()
    {
        return routeRoomsList;
    }

    public void SetFirstZoneRoom(GameObject _go)
    {
        firstZoneRoom = _go;
    }

    public GameObject GetFirstZoneRoom()
    {
        return firstZoneRoom;
    }
}
