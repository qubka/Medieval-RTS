using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SortByDistance : MonoBehaviour
{
    [ReadOnly] public List<SquadButton> buttons;
    private Transform worldTransform;
    private Transform cameraTranform;
    private float nextUpdateTime;

    private void Awake()
    {
        buttons = new List<SquadButton>();
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
            var i = 0;
            foreach (var button in buttons.OrderBy(b => Vector.DistanceSq(b.squad.centroid, pos)).Reverse()) {
                button.transform.SetSiblingIndex(i);
                i++;
            }
            worldTransform.SetAsFirstSibling();
            nextUpdateTime = currentTime + 0.1f;
        }
    }

    public void AddObject(GameObject obj)
    {
        buttons.Add(obj.GetComponent<SquadButton>());
    }
    
    public void RemoveObject(GameObject obj)
    {
        buttons.Remove(obj.GetComponent<SquadButton>());
    }
}
