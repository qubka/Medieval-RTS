using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SquadInfo : MonoBehaviour
{
    [SerializeField] private Text caption;
    [SerializeField] private Text status;
    [SerializeField] private Text count;
    [SerializeField] private Image icon;
    [SerializeField] private Slider morale;
    [SerializeField] private float offset;
    [Space]
    [SerializeField] private RectTransform attributeCanvas;
    [SerializeField] private GameObject attributeLayout;
    [SerializeField] private Text attributeField;

    private UnitManager manager;
    private RectTransform rectTransform;
    private Dictionary<MoraleAttribute, GameObject> attributes;
    private Vector3 shift;
    
    private void Start()
    {
        rectTransform = transform as RectTransform;
        Resize();
        rectTransform.position = Input.mousePosition + shift;
        // You can directly tell LoadAll to only load assets of the correct type
        // even if there would be other assets in the same folder
        var attributeObjects = Resources.LoadAll<MoraleAttribute>("Morale/").ToList();
        attributes = new Dictionary<MoraleAttribute, GameObject>(attributeObjects.Count);
        foreach (var attribute in attributeObjects.OrderByDescending(a => a.bonus)) {
            var obj = Instantiate(attributeLayout, attributeCanvas);
            obj.GetComponent<AttributeLayout>().SetAttribute(attribute);
            obj.SetActive(false);
            attributes.Add(attribute, obj);
        }
        manager = Manager.unitManager;
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
        var squad = manager.hover;
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
            
            var data = squad.data;
            icon.sprite = data.canvasIcon;
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

            attributeField.text = bonus.ToString("+#;-#;0");
            attributeField.color = bonus > 0 ? Color.green : bonus == 0 ? Color.yellow : Color.red;
            
            rectTransform.localScale = Vector3.one;
        } else {
            rectTransform.localScale = Vector3.zero;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform) {
            Resize();
        }
    }

    private void Resize()
    {
        var size = rectTransform.sizeDelta;
        shift = new Vector3((size.x + offset), -(size.y + offset), 0f);
    }

    private void Update()
    {
        rectTransform.position = Input.mousePosition + shift;
    }
}
