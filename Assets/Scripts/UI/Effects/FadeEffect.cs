using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeEffect : MonoBehaviour
{
    public float fadeSpeed = 0.5f;
    private Image image;
    private bool enable;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(ToggleState), 0f, fadeSpeed);
    }

    private void OnDisable()
    {
        if (IsInvoking(nameof(ToggleState))) {
            CancelInvoke(nameof(ToggleState));
            var color = image.color;
            color.a = 0f;
            image.color = color;
            enable = false;
        }
    }

    private void ToggleState()
    {
        enable = !enable;
        StartCoroutine(image.Fade(enable ? 0f : 1f, fadeSpeed - 0.1f));
    }
}
