using System.Collections;
using System.Linq;
using UnityEngine;

public class SortByDistance : MonoBehaviour
{
    private Transform worldTransform;
    private Transform cameraTranform;
    private SquadTable squadList;

    private void Start()
    {
        worldTransform = transform;
        cameraTranform = Manager.camTransform;
        squadList = Manager.squadTable;
        StartCoroutine(SortList());
    }

    private IEnumerator SortList()
    {
        while (true) {
            var pos = cameraTranform.position;
            var i = squadList.Count - 1;
            foreach (var squad in squadList.Values.OrderBy(s => Vector.DistanceSq(s.centroid, pos))) {
                squad.barTransform.SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
