using System.Linq;
using UnityEngine;

public class SortByDistance : ListBehaviour<SquadBar>
{
    private Transform worldTransform;
    private Transform cameraTranform;

    private void Start()
    {
        worldTransform = transform;
        cameraTranform = Manager.camTransform;
        InvokeRepeating(nameof(SortList), 0f, 0.1f);
    }

    private void SortList()
    {
        var pos = cameraTranform.position;
        var i = list.Count - 1;
        foreach (var button in list.OrderBy(b => Vector.DistanceSq(b.squad.centroid, pos))) {
            button.transform.SetSiblingIndex(i);
            i--;
        }
        worldTransform.SetAsFirstSibling();
    }
}
