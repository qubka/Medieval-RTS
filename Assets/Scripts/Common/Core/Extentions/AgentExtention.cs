using UnityEngine.AI;

public static class AgentExtention
{
    public static bool IsArrived(this NavMeshAgent agent)
    {
        if (!agent.pathPending) {
            if (agent.remainingDistance <= agent.stoppingDistance) {
                if (!agent.hasPath || agent.velocity.SqMagnitude() == 0f) {
                    return true;
                }
            }
        }
        return false;
    }
}
