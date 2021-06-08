using System.Collections;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[RequiredComponent(typeof(RectTransform))]
public class SortByDistance : MonoBehaviour
{
    private Transform worldTransform;
    private Transform cameraTransform;
    private TableObject<IGameObject> objectTable;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Start()
    {
        cameraTransform = Manager.cameraTransform;
        objectTable = ObjectTable.Instance;
        StartCoroutine(SortingList());
    }

    private IEnumerator SortingList()
    {
        while (true) {
            var pos = cameraTransform.position;
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
