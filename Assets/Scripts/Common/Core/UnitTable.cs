using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTable : MonoBehaviour, IEnumerable<Unit>
{
    private Dictionary<int, Unit> unitTable;
    public Unit this[int index] => unitTable[index];

    private void Awake()
    {
        unitTable = new Dictionary<int, Unit>();
    }

    public void AddUnit(GameObject obj, Unit unit)
    {
        var id = obj.GetInstanceID();
        if (!unitTable.ContainsKey(id)) {
            unitTable.Add(id, unit);
        }
    }

    public Unit GetUnit(GameObject obj)
    {
        return unitTable[obj.GetInstanceID()];
    }

    public bool ContainsId(int id)
    {
        return unitTable.ContainsKey(id);
    }

    public void RemoveEntry(int id)
    {
        unitTable.Remove(id);
    }

    public IEnumerator<Unit> GetEnumerator()
    {
        return unitTable.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
