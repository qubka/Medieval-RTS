using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeEffect : MonoBehaviour
{
    public float fadeSpeed = 0.5f;
    private Image image;
    private bool enable;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        StartCoroutine(Toggle());
    }

    private void OnDisable()
    {
        var color = image.color;
        color.a = 0f;
        image.color = color;
        enable = false;
    }

    private IEnumerator Toggle()
    {
        while (true) {
            enable = !enable;
            StartCoroutine(image.Fade(enable ? 0f : 1f, fadeSpeed - 0.1f));
            yield return new WaitForSecondsRealtime(fadeSpeed);
        }
    }
}
