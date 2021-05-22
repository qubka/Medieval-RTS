using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public static class ImageExtention
{
    public static IEnumerator Fade(this Image image, float targetFade, float fadeSpeed)
    {
        var currentTime = 0f;
        var currentFade = 1f - targetFade;

        while (currentTime < fadeSpeed) {
            var alpha = math.lerp(currentFade, targetFade, currentTime / fadeSpeed);
            
            var color = image.color;
            color.a = alpha;
            image.color = color;

            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
