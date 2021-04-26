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

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Start()
    {
        camTransform = Manager.camTransform;
        StartCoroutine(SortingList());
    }

    private IEnumerator SortingList()
    {
        while (true) {
            var pos = camTransform.position;
            var list = SortList.Instance.list;
            var i = list.Count - 1;
            foreach (var sortable in list.OrderBy(s => Vector.DistanceSq(s.GetPosition(), pos))) {
                sortable.GetTransform().SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
