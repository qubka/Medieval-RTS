using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class BuildingLayout : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI label;
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

    [HideInInspector] public Settlement settlement;
    [HideInInspector] public Building building;

    public void SetBuilding(Settlement settlement, Building building)
    {
        this.settlement = settlement;
        this.building = building;
    }
    
    public void Enable(Pack<int, float> data)
    {
        button.interactable = true;
        disabled.SetActive(false);
        icon.sprite = building.icon;
        label.text = building.name;
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
        
        icon.sprite = building.icon;
        label.text = building.name;
        
        if (building.dependencies.Length > 0) {
            foreach (var resource in building.dependencies) {
                if (!settlement.resources.Contains(resource)) {
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

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void Pressed()
    {
        settlement.Build(building);
        ArmyManager.Instance.townController.OnUpdate();
    }
}
