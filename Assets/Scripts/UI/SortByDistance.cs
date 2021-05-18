using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[RequiredComponent(typeof(RectTransform))]
public class SortByDistance : MonoBehaviour
{
    private Transform worldTransform;
    private Transform camTransform;
    private TableObject<IGameObject> objectTable;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Start()
    {
        camTransform = Manager.camTransform;
        objectTable = ObjectTable.Instance;
        StartCoroutine(SortingList());
    }

    private IEnumerator SortingList()
    {
        while (true) {
            var pos = camTransform.position;
            var i = objectTable.Count - 1;
            foreach (var o in objectTable.Values.OrderBy(s => Vector.DistanceSq(s.GetPosition(), pos))) {
                o.GetIcon().SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
