using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class SquadDescription : MonoBehaviour
{
    public Text caption;
    public Text count;
    public Text killed;
    public Image icon;
    public Slider meleeAttack;
    public Slider meleeDamage;
    public Slider chargeBonus;
    public Slider defenseSkill;
    public Slider armor;
    public Slider health;
    public Slider shield;
    public Slider morale;
    public Slider speed;
    public Slider ammunition;
    public Slider range;
    public Slider missleDamage;

    private RectTransform rectTransform;
    private UnitManager manager;
    private bool enable;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        manager = Manager.unitManager;
        InvokeRepeating(nameof(UpdateData), 0f, 1f);
    }

    public void UpdateData()
    {
        if (manager.selectedCount == 1) {
            Toggle(true);

            var squad = manager.selectedUnits[0];
            var data = squad.data;
            meleeAttack.value = data.meleeAttack;
            meleeDamage.value = data.meleeWeapon.baseDamage + data.meleeWeapon.armorPiercingDamage;
            chargeBonus.value = data.chargeBonus;
            defenseSkill.value = data.defenceSkill;
            armor.value = data.armor;
            health.value = data.manHealth + data.mountHealth;
            shield.value = data.shield;
            morale.value = data.morale;
            speed.value = data.squadRunSpeed * 10f;
            ammunition.value = data.ammunition;
            range.value = data.rangeDistance;
            missleDamage.value = (data.rangeWeapon) ? data.rangeWeapon.missileDamage + data.rangeWeapon.missileArmorPiercingDamage : 0f;
            icon.sprite = data.canvasIcon;
            caption.text = data.name; //TODO: Translation
            count.text = $"{squad.unitCount} ({squad.squadSize})";
            killed.text = squad.killed.ToString();
        } else {
            Toggle(false);
        }
    }

    private void Toggle(bool value)
    {
        if (enable == value)
            return;
        
        enable = value;
        
        var current = rectTransform.localPosition.x;
        var target = rectTransform.sizeDelta.x / (enable ? 2f : -2f);

        gameObject.Tween("DescMove", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, DescMove);
    }

    private void DescMove(ITween<float> obj)
    {
        var position = rectTransform.localPosition;
        position.x = obj.CurrentValue;
        rectTransform.localPosition = position;
    }
}
