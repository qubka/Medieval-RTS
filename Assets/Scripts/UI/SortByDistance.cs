using System;
using System.Collections;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[RequiredComponent(typeof(RectTransform))]
public class SortByDistance : MonoBehaviour
{
    private Transform worldTransform;
    private Transform camTransform;
    private SortList sortList;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Start()
    {
        camTransform = Manager.camTransform;
        sortList = Manager.sortList;
        StartCoroutine(SortList());
    }

    private IEnumerator SortList()
    {
        while (true) {
            var pos = camTransform.position;
            var i = sortList.Count - 1;
            foreach (var sortable in sortList.list.OrderBy(s => Vector.DistanceSq(s.GetPosition(), pos))) {
                sortable.GetTransform().SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
