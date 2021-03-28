using UnityEngine;

public class ParticleCallback : MonoBehaviour
{
    public string poolTag;
    
    public void OnParticleSystemStopped()
    {
        Manager.objectPool.ReturnToPool(poolTag, gameObject); 
    }
}
