using System;
using DigitalRuby.Tween;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
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

    public void SetParties(Party allies, Party enemies)
    {
        // Instantiate allied bar
        alliesLeader.text = allies.leader.title.Length > 0 ? allies.leader.title + ' ' + allies.leader.name : allies.leader.name;
        alliesAmount.text = allies.TroopCount.ToString();
        //alliesPortrait.sprite = allies.leader.

        while (alliesGrid.childCount > 0){
            Destroy(alliesGrid.GetChild(0));
        }
        
        foreach (var troop in allies.troops) {
            Instantiate(unitLayout, alliesGrid).GetComponent<UnitLayout>().SetTroop(troop);
        }
        
        // Instantiate enemy bar
        enemiesLeader.text = enemies.leader.title.Length > 0 ? enemies.leader.title + ' ' + enemies.leader.name : enemies.leader.name;
        enemiesAmount.text = enemies.TroopCount.ToString();
        //enemiesPortrait.sprite = enemies.leader.
        
        while (enemiesGrid.childCount > 0){
            Destroy(enemiesGrid.GetChild(0));
        }
        
        foreach (var troop in enemies.troops) {
            Instantiate(unitLayout, enemiesGrid).GetComponent<UnitLayout>().SetTroop(troop);
        }
        
        // Update slider like in CombatSliderController
        slider.value = math.clamp((float) allies.TroopStrength / enemies.TroopStrength, 0.1f, 1.9f);
        
        // Enable it 
        Enable();
    }

    public void Enable()
    {
        gameObject.Tween("LeftMove", left.transform.localPosition.x, left.initial + left.transform.sizeDelta.x - 7.5f, 0.5f, TweenScaleFunctions.CubicEaseInOut, LeftMove);
        gameObject.Tween("RightMove", right.transform.localPosition.x, right.initial - right.transform.sizeDelta.x + 7.5f, 0.5f, TweenScaleFunctions.CubicEaseInOut, RightMove);
        gameObject.Tween("BottomMove", bottom.transform.localPosition.y, bottom.initial + bottom.transform.sizeDelta.y, 0.5f, TweenScaleFunctions.CubicEaseInOut, BottomMove);
    }
    
    public void Disable()
    {
        gameObject.Tween("LeftMove", left.transform.localPosition.x, left.initial, 0.5f, TweenScaleFunctions.CubicEaseInOut, LeftMove);
        gameObject.Tween("RightMove", right.transform.localPosition.x, right.initial, 0.5f, TweenScaleFunctions.CubicEaseInOut, RightMove);
        gameObject.Tween("BottomMove", bottom.transform.localPosition.y, bottom.initial, 0.5f, TweenScaleFunctions.CubicEaseInOut, BottomMove);
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
