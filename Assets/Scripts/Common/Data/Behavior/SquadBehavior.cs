using UnityEngine;

public abstract class SquadBehavior : MonoBehaviour
{
    protected Squad squad;
    protected Agent agent;
    protected Transform worldTransform;
    
    protected virtual void Awake()
    {
        squad = GetComponent<Squad>();
        agent = squad.agentScript;
        worldTransform = squad.worldTransform;
    }
    
    protected virtual void Start()
    {
        InvokeRepeating(nameof(UpdateHandler), 0f, 0.1f);
    }
    
    private void UpdateHandler()
    {
        if (!worldTransform) {
            CancelInvoke(nameof(UpdateHandler));
            return;
        }

        RareUpdate();
    }

    protected abstract void RareUpdate();
}
