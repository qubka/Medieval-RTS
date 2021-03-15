using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class RandomExtention
{
    public static AnimationData GetRandom(this List<AnimationData> list, int index = 0)
    {
        return list.Count == 1 ? list[0] : list[Random.Range(index, list.Count)];
    }
    
    public static AnimationData GetRandom(this List<AnimationData> list, AnimationData prev)
    {
        return list.Count == 1 ? list[0] : list[RandomExcept(0, list.Count, list.IndexOf(prev))];
    }
    
    public static AnimationClip GetRandom(this List<AnimationClip> list, int index = 0)
    {
        return list.Count == 1 ? list[0] : list[Random.Range(index, list.Count)];
    }
    
    public static AnimationClip GetRandom(this List<AnimationClip> list, AnimationClip prev)
    {
        return list.Count == 1 ? list[0] : list[RandomExcept(0, list.Count, list.IndexOf(prev))];
    }
    
    public static AudioClip GetByProportion(this List<AudioClip> list, int current, int max)
    {
        return list[(int) Mathf.Floor((float) (list.Count - 1) * current / max)];
    }
    
    public static AudioClip GetRandom(this List<AudioClip> list)
    {
        return list.Count == 1 ? list[0] : list[Random.Range(0, list.Count)];
    }
    
    public static AudioClip GetRandom(this List<AudioClip> list, AudioClip prev)
    {
        return list.Count == 1 ? list[0] : list[RandomExcept(0, list.Count, list.IndexOf(prev))];
    }
    
    private static int RandomExcept(int min, int max, int except)
    {
        var random = Random.Range(min, max);
        if (random >= except) random = (random + 1) % max;
        return random;
    }
}
