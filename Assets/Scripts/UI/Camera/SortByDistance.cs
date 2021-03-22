using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SortByDistance : ListBehavior<SquadBar>
{
    private Transform worldTransform;
    private Transform cameraTranform;
    private float nextUpdateTime;

    private void Start()
    {
        worldTransform = transform;
        cameraTranform = Manager.cameraTransform;
    }

    private void LateUpdate()
    {
        var currentTime = Time.time;
        if (currentTime > nextUpdateTime) {
            var pos = cameraTranform.position;
            var i = list.Count - 1;
            foreach (var button in list.OrderBy(b => Vector.DistanceSq(b.squad.centroid, pos))) {
                button.transform.SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            nextUpdateTime = currentTime + 0.1f;
        }
    }
}
