using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SortByDistance : MonoBehaviour
{
    [ReadOnly] public List<SquadBar> buttons;
    private Transform worldTransform;
    private Transform cameraTranform;
    private float nextUpdateTime;

    private void Awake()
    {
        buttons = new List<SquadBar>();
    }

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
            var i = buttons.Count - 1;
            foreach (var button in buttons.OrderBy(b => Vector.DistanceSq(b.squad.centroid, pos))) {
                button.transform.SetSiblingIndex(i);
                i--;
            }
            worldTransform.SetAsFirstSibling();
            nextUpdateTime = currentTime + 0.1f;
        }
    }

    public void AddButton(SquadBar bar)
    {
        buttons.Add(bar);
    }
    
    public void RemoveButton(SquadBar bar)
    {
        buttons.Remove(bar);
    }
}
