using System.Collections;
using BehaviorDesigner.Runtime.Tasks;
using GPUInstancer.CrowdAnimations;
using UnityEngine;

[RequiredComponent(typeof(GPUICrowdPrefab))]
public class AnimatorRandomizer : MonoBehaviour
{
    private GPUICrowdPrefab crowd;
    private AnimationClip[] clips;
    private AnimationClip prev;
    
    private void Awake()
    {
        clips = GetComponent<Animator>().runtimeAnimatorController.animationClips;
        crowd = GetComponent<GPUICrowdPrefab>();
    }

    private IEnumerator Start()
    {
        while (true) {
            var random = clips.GetRandom(prev);
            crowd.StartAnimation(random, -1f, 1f, 0.5f);
            prev = random;
            yield return new WaitForSeconds(random.length);
        }
    }
}
