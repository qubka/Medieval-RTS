using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CombatSliderController : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        StartCoroutine(ChangeSlider());
    }
    
    private IEnumerator ChangeSlider()
    {
        while (true) {
            var allies = 0;
            var enemies = 0;
            
            foreach (var squad in SquadTable.Instance) {
                if (squad.state == SquadFSM.Retreat)
                    continue;
                
                var stats = squad.unitCount * squad.data.TotalStats;
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
                slider.value = math.clamp((float) allies / enemies, 0.1f, 1.9f);
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }
}
