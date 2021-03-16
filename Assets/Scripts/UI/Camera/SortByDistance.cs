using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SortByDistance : MonoBehaviour
{
    private Transform worldTransform;
    private Transform cameraTranform;
    private List<SquadButton> buttons;
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
            if (buttons == null) {
                buttons = GetComponentsInChildren<SquadButton>().ToList();
            }
            
            var pos = cameraTranform.position;
            var i = 0;
            foreach (var button in buttons.OrderBy(b => Vector.DistanceSq(b.squad.centroid, pos)).Reverse()) {
                button.transform.SetSiblingIndex(i);
                i++;
            }
            worldTransform.SetAsFirstSibling();
            nextUpdateTime = currentTime + 0.1f;
        }
    }
    
    public void RemoveSquad(SquadButton button)
    {
        buttons.Remove(button);
    }
}
