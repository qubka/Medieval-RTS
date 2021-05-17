using System.Collections;
using UnityEngine;

public abstract class BarBehavior : MonoBehaviour
{
    protected virtual void Start()
    {
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public abstract void OnUpdate();

    protected void SetInteractable(ControlButton control, bool value, bool select = false)
    {
        control.button.interactable = value;
        control.image.material.SetFloat(Manager.GrayscaleAmount, value ? 0f : 1f);
        control.target.SetActive(value && select);
    }
}