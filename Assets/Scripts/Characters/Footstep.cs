using System;
using UnityEngine;

public class Footstep : MonoBehaviour
{
    [HideInInspector] public DateTime started;
    [HideInInspector] public Transform worldTransform;
    
    private Line line;
    private Line head;
    private bool isVisible = true;
        
    private void Awake()
    {
        started = TimeController.Now;
        worldTransform = transform;
        line = new Line(Manager.global.movementLine);
        head = new Line(Manager.global.arrowLine);
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        throw new NotImplementedException();
    }

    public void Load(FootstepSave save)
    {
        started = new DateTime(save.ticks);
        worldTransform.position = save.position;
        worldTransform.rotation = save.rotation;
    }

    public void SetVisibility(bool value)
    {
        if (isVisible == value)
            return;

        line.SetActive(value);
        head.SetActive(value);
        
        isVisible = value;
    }
    
    private void Destroy()
    {
        StartCoroutine(line.FadeLineRenderer(0.5f));
        StartCoroutine(head.FadeLineRenderer(0.5f));
        Destroy(gameObject, 0.5f);
    }

    #region Tooltip

    private void OnMouseOver()
    {
        /*if (Manager.IsPointerOnUI || !isVisible) {
            Manager.fixedPopup.HideInfo();
            return;
        }
        
        var currentTime = Time.unscaledTime;
        if (currentTime > nextHoverTime) {
            var color = data.leader.faction.color.ToHexString();
            var builder = new StringBuilder();
            var totalCount = 0;

            builder
                .Append("<size=18>")
                .Append("<color=#")
                .Append(color)
                .Append('>')
                .Append('(')
                .Append(data.leader.faction.label)
                .Append(')')
                .Append("</size>")
                .Append("</color>")
                .AppendLine();

            if (data.troops.Count > 0) {
                builder
                    .AppendLine()
                    .Append("<size=15>")
                    .Append("<color=#ffffffff>")
                    .Append("TROOPS:")
                    .Append("</size>")
                    .Append("</color>")
                    .AppendLine()
                    .Append("<color=#00ffffff>");

                var dict = new Dictionary<Squadron, int>(data.troops.Count);

                foreach (var troop in data.troops) {
                    var troopCount = troop.size;
                    totalCount += troopCount;
                    
                    var data = troop.data;
                    if (dict.ContainsKey(data)) {
                        dict[data] += troop.size;
                    } else {
                        dict.Add(data, troop.size);
                    }
                }
                
                foreach (var pair in dict) {
                    builder
                        .Append(pair.Key.name)
                        .Append(' ')
                        .Append('(')
                        .Append(pair.Value)
                        .Append(')')
                        .AppendLine();
                }

                builder.Append("</color>").AppendLine();
            }
            
            var description = builder.ToString();

            builder.Clear();

            builder
                .Append("<color=#")
                .Append(color)
                .Append('>');

            switch (data.leader.type) {
                case CharacterType.Player:
                case CharacterType.Noble:
                    builder
                        .Append(data.leader.title)
                        .Append(' ')
                        .Append(data.leader.surname)
                        .Append("'s Party");
                    break;
                case CharacterType.Bandit:
                    builder.Append("Marauders");
                    break;
                case CharacterType.Peasant:
                    builder.Append("Villagers");
                    break;
            }

            if (totalCount > 0) {
                builder
                    .Append(' ')
                    .Append('(')
                    .Append(totalCount)
                    .Append(')');
            }

            builder.Append("</color>");

            var caption = builder.ToString();

            Manager.dynamicPopup.DisplayInfo(caption, description);
            
            nextHoverTime = currentTime + 1f;
        }*/
    }
    
    private void OnMouseExit()
    {
        //nextHoverTime = 0f;
        //Manager.dynamicPopup.HideInfo();
    }

    #endregion
}

[Serializable]
public class FootstepSave
{
    public long ticks;
    public Vector3 position;
    public Quaternion rotation;

    public FootstepSave(Footstep footstep)
    {
        ticks = footstep.started.Ticks;
        position = footstep.worldTransform.position;
        rotation = footstep.worldTransform.rotation;
    }
}