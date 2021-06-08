using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadInfo : Tooltip
{
    [SerializeField] private TextMeshProUGUI caption;
    [SerializeField] private TextMeshProUGUI status;
    [SerializeField] private TextMeshProUGUI count;
    [SerializeField] private Image type;
    [SerializeField] private Slider morale;
    [Space]
    [SerializeField] private RectTransform attributeCanvas;
    [SerializeField] private GameObject attributeLayout;
    [SerializeField] private TextMeshProUGUI attributeField;
    [Space] 
    [SerializeField] private Image countBackground;
    [SerializeField] private Image typeBackground;
    [SerializeField] private Image barBackground;
    [Space] 
    [SerializeField] private Sprite enemyIcon;
    [SerializeField] private Sprite friendlyIcon;
    [SerializeField] private Sprite enemyBar;
    [SerializeField] private Sprite friendlyBar;

    private readonly Dictionary<MoraleAttribute, GameObject> attributes = new Dictionary<MoraleAttribute, GameObject>();

    private void Start()
    {
        foreach (var attribute in Manager.MoraleAttributes.OrderByDescending(a => a.bonus)) {
            var obj = Instantiate(attributeLayout, attributeCanvas);
            obj.GetComponent<AttributeLayout>().SetAttribute(attribute);
            obj.SetActive(false);
            attributes.Add(attribute, obj);
        }
        
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
    
    public void OnUpdate()
    {
        var squad = SquadManager.Instance.hover;
        if (squad) {
            switch (squad.state) {
                case SquadFSM.Idle:
                    status.text = "Ready";
                    break;
                case SquadFSM.Seek:
                    status.text = "Moving";
                    break;
                case SquadFSM.Attack:
                    status.text = squad.touchEnemies ? "In Combat" : squad.isRange ? "Shooting" : "Charging";
                    break;
                case SquadFSM.Retreat:
                    status.text = "Retreating";
                    break;
            }

            if (squad.team == Team.Enemy) {
                countBackground.sprite = enemyIcon;
                typeBackground.sprite = enemyIcon;
                barBackground.sprite = enemyBar;
            } else {
                countBackground.sprite = friendlyIcon;
                typeBackground.sprite = friendlyIcon;
                barBackground.sprite = friendlyBar;
            }
            
            var data = squad.data;
            type.sprite = data.classIcon;
            caption.text = data.name; //TODO: Translation
            count.text = squad.unitCount.ToString();
            morale.value = squad.morale;

            var bonus = 0;
            foreach (var pair in attributes) {
                var att = pair.Key;
                var obj = pair.Value;
                
                var enable = squad.attributes.Contains(att);
                obj.SetActive(enable);
                if (enable) {
                    bonus += att.bonus;
                }
            }

            attributeField.SetInteger(bonus);

            SetActive(true);
        } else {
            SetActive(false);
        }
    }
}
