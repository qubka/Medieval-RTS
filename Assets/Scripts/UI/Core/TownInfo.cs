using System.Collections;
using UnityEngine;

public class TownInfo : Tooltip
{
    
    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
    
    public void OnUpdate()
    {
        /*var hover = manager.hover;
        if (hover != null) {
            
            
            
            var data = squad.data;
            icon.sprite = data.classIcon;
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
        }*/
    }
}