using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TownTab : MonoBehaviour
{
    [SerializeField] private ControlButton building;
    [SerializeField] private ControlButton unit;
    [SerializeField] private ControlButton market;
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
    }

    public void ToggleBuilding()
    {
        Switch(true);
    }
    
    public void ToggleUnit()
    {
        Switch(isUnit: true);
    }
    
    public void ToggleMarket()
    {
        Switch(isMarket: true);
    }
    
    public void ToggleInfo()
    {
        Switch(isInfo: true);
    }

    private void Switch(bool isBuilding = false, bool isUnit = false, bool isMarket = false, bool isInfo = false)
    {
        SetInteractable(building, isBuilding);
        SetInteractable(unit, isUnit);
        SetInteractable(market, isMarket);
        SetInteractable(info, isInfo);
    }
    
    private void SetInteractable(ControlButton control, bool value)
    {
        control.button.interactable = !value;
        control.target.SetActive(value);
        if (value) {
            control.image.sprite = activateImage;
            control.image.color = activateColor;
        } else {
            control.image.sprite = disabledImage;
            control.image.color = disabledColor;
        }
    }
}
