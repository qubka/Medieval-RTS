using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class ImageExtention
{
    public static IEnumerator FadeOut(this Image image, float targetFade, float fadeSpeed)
    {
        var currentTime = 0f;
        var currentFade = 1f - targetFade;
        
        while (currentTime < fadeSpeed) {
            var alpha = Mathf.Lerp(targetFade, currentFade, currentTime / fadeSpeed);
            
            var color = image.color;
            color.a = alpha;
            image.color = color;
            
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}
