using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class RandomExtention
{
    public static AnimationData GetRandom(this AnimationData[] array, int index = 0)
    {
        return array.Length == 1 ? array[0] : array[Random.Range(index, array.Length)];
    }
    
    public static AnimationData GetRandom(this AnimationData[] array, AnimationData prev)
    {
        return array.Length == 1 ? array[0] : array[RandomExcept(0, array.Length, Array.IndexOf(array, prev))];
    }
    
    public static AnimationClip GetRandom(this AnimationClip[] array, int index = 0)
    {
        return array.Length == 1 ? array[0] : array[Random.Range(index, array.Length)];
    }
    
    public static AnimationClip GetRandom(this AnimationClip[] array, AnimationClip prev)
    {
        return array.Length == 1 ? array[0] : array[RandomExcept(0, array.Length, Array.IndexOf(array, prev))];
    }
    
    public static AudioClip GetByProportion(this AudioClip[] array, int current, int max)
    {
        return array[(int) math.floor((float) (array.Length - 1) * current / max)];
    }
    
    public static AudioClip GetRandom(this AudioClip[] array)
    {
        return array.Length == 1 ? array[0] : array[Random.Range(0, array.Length)];
    }
    
    public static AudioClip GetRandom(this AudioClip[] array, AudioClip prev)
    {
        return array.Length == 1 ? array[0] : array[RandomExcept(0, array.Length, Array.IndexOf(array, prev))];
    }
    
    private static int RandomExcept(int min, int max, int except)
    {
        var random = Random.Range(min, max);
        if (random >= except) random = (random + 1) % max;
        return random;
    }

    public static bool NextBool => Random.Range(0, 2) == 0;
    public static float NextFloat => Random.Range(0f, 1f);
}
