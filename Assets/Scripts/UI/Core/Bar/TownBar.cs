using System;
using UnityEngine;
using UnityEngine.UI;

public class TownBar : MonoBehaviour
{
    [SerializeField] private Image barIcon;
    [Space]
    [SerializeField] private ControlButton building;
    [SerializeField] private ControlButton unit;
    [SerializeField] private ControlButton recruit;
    [SerializeField] private ControlButton info;
    [Space]
    [SerializeField] private Sprite activateImage;
    [SerializeField] private Sprite disabledImage;
    [SerializeField] private Color activateColor;
    [SerializeField] private Color disabledColor;
    
    [Serializable]
    public class ControlButton
    {
        public GameObject target;
        public Button button;
        public Image image;
        public Sprite icon;
    }

    public void Building()
    {
        Switch(true);
    }
    
    public void Unit()
    {
        Switch(isUnit: true);
    }
    
    public void Recruit()
    {
        Switch(isRecruit: true);
    }
    
    public void Info()
    {
        Switch(isInfo: true);
    }

    private void Switch(bool isBuilding = false, bool isUnit = false, bool isRecruit = false, bool isInfo = false)
    {
        SetInteractable(building, isBuilding);
        SetInteractable(unit, isUnit);
        SetInteractable(recruit, isRecruit);
        SetInteractable(info, isInfo);
    }
    
    private void SetInteractable(ControlButton control, bool value)
    {
        control.button.interactable = !value;
        control.target.SetActive(value);
        if (value) {
            control.image.sprite = activateImage;
            control.image.color = activateColor;
            barIcon.sprite = control.icon;
        } else {
            control.image.sprite = disabledImage;
            control.image.color = disabledColor;
        }
    }
}
