using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.Tween;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleWindow : MonoBehaviour
{
    [SerializeField] private Image alliesPortrait;
    [SerializeField] private TMP_Text alliesLeader;
    [SerializeField] private TMP_Text alliesAmount;
    [SerializeField] private RectTransform alliesGrid;
    [Space]
    [SerializeField] private Image enemiesPortrait;
    [SerializeField] private TMP_Text enemiesLeader;
    [SerializeField] private TMP_Text enemiesAmount;
    [SerializeField] private RectTransform enemiesGrid;
    [Space]
    [SerializeField] private Rect left;
    [SerializeField] private Rect right;
    [SerializeField] private Rect bottom;
    [SerializeField] private Slider slider;
    [Space]
    [SerializeField] private GameObject unitLayout;
    [Space]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;

    private Battle battle;
    private bool enable;
    
    [Serializable]
    public class Rect
    {
        public RectTransform transform;
        [HideInInspector] public float initial;
    }

    private void Start()
    {
        left.initial = left.transform.localPosition.x;
        right.initial = right.transform.localPosition.x;
        bottom.initial = bottom.transform.localPosition.y;
    }

    public void SetBattle(Battle battle)
    {
        this.battle = battle;
        
        var allies = battle.attackers;
        var enemies = battle.defenders; 
        
        // Instantiate allied bar
        var alliesPower = allies[0];//.FindStrongest();
        //alliesLeader.text = alliesPower.leader.title.Length > 0 ? alliesPower.leader.title + ' ' + alliesPower.leader.name : alliesPower.leader.name;
        alliesAmount.text = allies.Sum(p => p.TroopSize).ToString();
        //alliesPortrait.sprite = allies.leader.
        
        for (var i = alliesGrid.childCount - 1; i >= 0; i--) {
            Destroy(alliesGrid.GetChild(i).gameObject);
        }

        foreach (var party in allies) {
            foreach (var troop in party.troops) {
                Instantiate(unitLayout, alliesGrid).GetComponent<UnitLayout>().SetTroop(troop);
            }
        }

        // Instantiate enemy bar
        var enemiesPower = enemies[0];//.FindStrongest();
        //enemiesLeader.text = enemiesPower.leader.title.Length > 0 ? enemiesPower.leader.title + ' ' + enemiesPower.leader.name : enemiesPower.leader.name;
        enemiesAmount.text = enemies.Sum(p => p.TroopSize).ToString();
        //enemiesPortrait.sprite = enemies.leader.

        for (var i = enemiesGrid.childCount - 1; i >= 0; i--) {
            Destroy(enemiesGrid.GetChild(i).gameObject);
        }

        foreach (var party in enemies) {
            foreach (var troop in party.troops) {
                Instantiate(unitLayout, enemiesGrid).GetComponent<UnitLayout>().SetTroop(troop);
            }
        }

        // Update slider like in CombatSliderController
        slider.value = math.clamp((float) allies.Sum(p => p.TroopStrength) / enemies.Sum(p => p.TroopStrength), 0.1f, 1.9f);
        
        // Enable it 
        Enable();
    }

    public void Attack()
    {
        if (battle) battle.Combat();
    }

    public void Retreat()
    {
        if (battle) battle.Retreat();
        Disable();
    }

    public void Enable()
    {
        if (enable)
            return;

        gameObject.Tween("LeftMove", left.transform.localPosition.x, left.initial + left.transform.sizeDelta.x - 7.5f, 0.5f, TweenScaleFunctions.CubicEaseInOut, LeftMove);
        gameObject.Tween("RightMove", right.transform.localPosition.x, right.initial - right.transform.sizeDelta.x + 7.5f, 0.5f, TweenScaleFunctions.CubicEaseInOut, RightMove);
        gameObject.Tween("BottomMove", bottom.transform.localPosition.y, bottom.initial + bottom.transform.sizeDelta.y, 0.5f, TweenScaleFunctions.CubicEaseInOut, BottomMove);
        
        TimeController.Instance.Lock();
        
        enable = true;
    }
    
    public void Disable()
    {
        if (!enable)
            return;

        gameObject.Tween("LeftMove", left.transform.localPosition.x, left.initial, 0.5f, TweenScaleFunctions.CubicEaseInOut, LeftMove);
        gameObject.Tween("RightMove", right.transform.localPosition.x, right.initial, 0.5f, TweenScaleFunctions.CubicEaseInOut, RightMove);
        gameObject.Tween("BottomMove", bottom.transform.localPosition.y, bottom.initial, 0.5f, TweenScaleFunctions.CubicEaseInOut, BottomMove);
        
        TimeController.Instance.Unlock();
        
        enable = false;
    }
    
    public void LeftMove(ITween<float> obj)
    {
        var position = left.transform.localPosition;
        position.x = obj.CurrentValue;
        left.transform.localPosition = position;
    }
    
    public void RightMove(ITween<float> obj)
    {
        var position = right.transform.localPosition;
        position.x = obj.CurrentValue;
        right.transform.localPosition = position;
    }
    
    public void BottomMove(ITween<float> obj)
    {
        var position = bottom.transform.localPosition;
        position.y = obj.CurrentValue;
        bottom.transform.localPosition = position;
    }
    
    /*private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width-400,0,200,50), "Enable"))
        {
            Enable();
        }
        
        if (GUI.Button(new Rect(Screen.width-800,0,200,50), "Disable"))
        {
            Disable();
        }
    }*/
}
