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
    private List<IGameObject> sortingList;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Start()
    {
        camTransform = Manager.camTransform;
        sortingList = ObjectList.Instance.list;
        StartCoroutine(SortingList());
    }

    private IEnumerator SortingList()
    {
        while (true) {
            var pos = camTransform.position;
            var i = sortingList.Count - 1;
            foreach (var o in sortingList.OrderBy(s => Vector.DistanceSq(s.GetPosition(), pos))) {
                o.GetIcon().SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
