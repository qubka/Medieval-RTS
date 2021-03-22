using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{
    private Image image;
    public Sprite activate;
    public Sprite disabled;
    public List<GameObject> trigger;
    public bool enable;
    
    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnButtonPressed()
    {
        enable = !enable;
        image.sprite = enable ? activate : disabled;
        foreach (var obj in trigger) {
            obj.SetActive(enable);
        }
    }
}
