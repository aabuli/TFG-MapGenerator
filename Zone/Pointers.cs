using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointers : MonoBehaviour
{
    private int pointerNumber = 0;
    [SerializeField] private List<int> zoneNeighbours = new List<int>();

    public int GetPointerNumber()
    { return pointerNumber; }

    public void SetPointerNumber(int _n)
    { pointerNumber = _n; }

    public void AddZoneNeighbour(int _n)
    { zoneNeighbours.Add(_n); }

    public void RemoveZoneNeighbour(int _n)
    {
        zoneNeighbours.Remove(_n);
    }

    public List<int> GetZoneNieghbours()
    { return zoneNeighbours; }


}
