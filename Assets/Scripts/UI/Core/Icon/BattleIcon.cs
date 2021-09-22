using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class BattleIcon : IconActivator
{
    [SerializeField] private TMP_Text left;
    [SerializeField] private TMP_Text right;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject status;
    [SerializeField] private Slider slider;

    public void OnUpdate(Battle battle)
    {
        var attackers = battle.attackers;
        if (attackers.Count > 0) {
            left.color = attackers[0].leader.faction.color;
        }

        var attackerCount = 0;
        var attackerStrength = 0;
        
        foreach (var attacker in attackers) {
            attackerCount += attacker.TroopSize;
            attackerStrength += attacker.TroopStrength;
        }
        
        left.text = attackerCount.ToString();
        
        var defenders = battle.defenders;
        if (defenders.Count > 0) {
            right.color = defenders[0].leader.faction.color;
        }
        
        var defenderCount = 0;
        var defenderStrength = 0;
        
        foreach (var defender in defenders) {
            defenderCount += defender.TroopSize;
            defenderStrength += defender.TroopStrength;
        }
        
        right.text = defenderCount.ToString();
        
        if (attackerStrength == 0) {
            slider.value = 2f;
        } else if (defenderStrength == 0) {
            slider.value = 0;
        } else {
            slider.value = math.clamp((float) attackerStrength / defenderStrength, 0.1f, 1.9f);
        }
    }

    public override void OnEnabled(bool value)
    {
        left.enabled = value;
        right.enabled = value;
        status.SetActive(value);
    }
}
