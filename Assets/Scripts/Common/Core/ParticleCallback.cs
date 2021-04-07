using UnityEngine;

public class ParticleCallback : MonoBehaviour
{
    public string poolTag;
    private int poolHash;
    
    public void Awake()
    {
        if (poolHash == 0) {
            poolHash = poolTag.GetHashCode();
        }
    }
    
    public void OnParticleSystemStopped()
    {
        Manager.objectPool.ReturnToPool(poolHash, gameObject); 
    }
}
