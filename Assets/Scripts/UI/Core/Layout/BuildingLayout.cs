using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class BuildingLayout : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image level;
    [SerializeField] private Button button;
    [Space]
    [SerializeField] private Slider slider;
    [SerializeField] private Slider background;
    [Space]
    [SerializeField] private GameObject bar;
    [SerializeField] private GameObject disabled;
    [SerializeField] private GameObject build;
    [SerializeField] private GameObject notBuild;
    [Space]
    [SerializeField] private Sprite[] levelIcons;
    
    public Settlement Settlement { get; private set; }
    public Building Building { get; private set; }

    public void SetBuilding(Settlement settlement, Building building)
    {
        Settlement = settlement;
        Building = building;
    }
    
    public void Enable(Pack<int, float> data)
    {
        button.interactable = true;
        disabled.SetActive(false);
        icon.sprite = Building.icon;
        label.text = Building.name;
        level.sprite = levelIcons[data.item1];
        if (data.item2 < 1f) {
            slider.value = data.item2;
            background.value = 1f - data.item2;
            bar.SetActive(true);
        } else {
            bar.SetActive(false);
        }
        build.SetActive(true);
        notBuild.SetActive(false);
    }

    public void Disable()
    {
        button.interactable = true;
        
        icon.sprite = Building.icon;
        label.text = Building.name;
        
        if (Building.dependencies.Length > 0) {
            foreach (var resource in Building.dependencies) {
                if (!Settlement.resources.Contains(resource)) {
                    button.interactable = false;
                    disabled.SetActive(true);
                    break;
                }
            }
        }
        
        bar.SetActive(false);
        build.SetActive(false);
        notBuild.SetActive(true);
    }

    public void Pressed()
    {
        Settlement.Build(Building);
        ArmyManager.Instance.townWindow.OnUpdate();
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
