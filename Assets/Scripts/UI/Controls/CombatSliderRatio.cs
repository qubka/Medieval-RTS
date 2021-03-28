using UnityEngine;
using UnityEngine.UI;

public class CombatSliderRatio : ListBehaviour<Squad>
{
    private Slider slider;
    
    private void Start()
    {
        slider = GetComponent<Slider>();
        InvokeRepeating(nameof(ChangeSlider), 0f, 1.0f);
    }
    
    private void ChangeSlider()
    {
        var allies = 0;
        var enemies = 0;
        
        foreach (var squad in list) {
            if (squad.state == SquadFSM.Retreat)
                continue;
            
            var stats = squad.unitCount * squad.data.TotalStats();
            if (squad.team == Team.Enemy) {
                enemies += stats;
            } else {
                allies += stats;
            }
        }
        
        if (enemies == 0) {
            slider.value = 2f;
        } else if (allies == 0) {
            slider.value = 0;
        } else {
            slider.value = Mathf.Clamp((float) allies / enemies, 0.1f, 1.9f);
        }
    }
}
