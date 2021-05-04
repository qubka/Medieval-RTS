using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class RotateEffect : MonoBehaviour
{
    public float rotateSpeed = 0.5f;
    public Vector3 targetAngle;
    private RectTransform rectTransform;
    private bool enable;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    private void OnEnable()
    {
        StartCoroutine(Toggle());
    }

    private void OnDisable()
    {
        rectTransform.localRotation = Quaternion.identity;
        enable = false;
    }

    private IEnumerator Toggle()
    {
        while (true) {
            enable = !enable;
            if (enable) {
                rectTransform.localEulerAngles = targetAngle;
            } else {
                rectTransform.localRotation = Quaternion.identity;
            }
            yield return new WaitForSecondsRealtime(rotateSpeed);
        }
    }
}
