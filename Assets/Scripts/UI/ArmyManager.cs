using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmyManager : SingletonObject<ArmyManager>, IManager<Troop>
{
    [Header("Main")]
    public TownController townController;
    public TextMeshProUGUI amount;
    public int maxTroops;

    //variables not visible in the inspector
    [HideInInspector] public List<Troop> selectedTroops = new List<Troop>();
#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private CamController camController;
    private ILayout troopLayout;
    private TerrainBorder border;
    private Army army;
    private Ray groundRay;
    private RaycastHit groundHit;
    private bool groundCast;

    private Collider[] colliders = new Collider[1];
    
    //public Party player => Game.Player;

    public void SetArmy(Army target)
    {
        army = target;
        camController.SetTarget(army.worldTransform);
        var player = Game.Player;
        foreach (var troop in player.troops) {
            AddLayout(troop);
        }
        amount.text = player.troops.Count + "/" + maxTroops; 
        enabled = true;
    }

    private void Start()
    {
        CursorManager.SetCursor(Manager.global.basicCursor);
        camera = Manager.mainCamera;
        camController = Manager.camController;
        border = Manager.border;
        enabled = false;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(1) || Manager.IsPointerOnUI && !army.IsVisible())
            return;
        
        groundRay = camera.ScreenPointToRay(Input.mousePosition);
        groundCast = Physics.Raycast(groundRay, out groundHit, Manager.TerrainDistance, Manager.Ground);
        
        if (groundCast && !border.IsOutsideBorder(groundHit.point)) {
            if (Physics.Raycast(groundRay, out var hit, Manager.TerrainDistance, Manager.Building | Manager.Army)) {
                var obj = ObjectTable.Instance[hit.transform.gameObject];
                if (obj is Town town) {
                    army.SetDestination(town.doorPosition, obj);
                } else {
                    army.SetDestination(obj.GetPosition(), obj);
                }
            } else {
                var dest = groundHit.point;
                //dest.y += 0.5f;
                army.SetDestination(dest);
                //Instantiate(Manager.global.deployParticle, dest, Quaternion.identity);
            }
            
            camController.SetTarget(army.worldTransform);
        }
    }

    public int SelectedCount()
    {
        return selectedTroops.Count;
    }

    public void SetLayout(ILayout layout)
    {
        troopLayout = layout;
    }

    public ILayout GetLayout()
    {
        return troopLayout;
    }

    public void AddSelected(Troop troop, bool toggle = false)
    {
        if (!selectedTroops.Contains(troop)) {
            selectedTroops.Add(troop);
            troop.Select(true);
        } else if (toggle) {
            selectedTroops.Remove(troop);
            troop.Select(false);
        }
    }
    
    public void DeselectAllExcept(Troop filter)
    {
        foreach (var troop in selectedTroops) {
            if (troop != filter) {
                troop.Select(false);
            }
        }

        // if exist, remove all except that, otherwise clear and add as new instance
        if (selectedTroops.Contains(filter)) {
            selectedTroops.RemoveAll(t => t != filter);
        } else {
            selectedTroops.Clear();
            AddSelected(filter);
        }
    }
    
    public void DeselectAll()
    {
        foreach (var troop in selectedTroops) {
            troop.Select(false);
        }
        selectedTroops.Clear();
    }

    public void SwapSelected(int newPos, int oldPos)
    {
        Game.Player.troops.Swap(newPos, oldPos);
    }
    
    public void AddLayout(Troop troop)
    {
        var cardObject = Instantiate(Manager.global.troopCard, Manager.cardCanvas);
        var layoutObject = Instantiate(Manager.global.troopLayout, Manager.layoutCanvas);
       
        var pursue = cardObject.GetComponent<LayoutPursue>();
        pursue.layoutTransform = layoutObject.transform;
        var card = cardObject.GetComponent<TroopCard>();
        card.SetTroop(troop);
        var layout = layoutObject.GetComponent<TroopLayout>();
        layout.layout = troop;
        layout.cardPursue = pursue;

        troop.card = card;
        troop.layout = layout;
        
        cardObject.SetActive(false);
        StartCoroutine(RepositionCard(cardObject, pursue.layoutTransform)); // fix for re-parenting
    }

    private IEnumerator RepositionCard(GameObject cardObject, Transform layoutTransform)
    {
        yield return new WaitForEndOfFrame();
        cardObject.SetActive(true);
        cardObject.transform.position = layoutTransform.position;
    }

    // RemoveTroop
    public void Disband()
    {
        var player = Game.Player;
        for (var i = selectedTroops.Count - 1; i > -1; i--) {
            var troop = selectedTroops[i];
            player.troops.Remove(troop);
            troop.Destroy();
            selectedTroops.RemoveAt(i);
        }
        amount.text = player.troops.Count + "/" + maxTroops; 
    }
    
    public void Merge()
    {
        
    }
    
    public void Garrison()
    {
    }

    public void AddTroop(Troop troop)
    {
        var player = Game.Player;
        player.troops.Add(troop);
        AddLayout(troop);
        amount.text = player.troops.Count + "/" + maxTroops;
        player.leader.money -= troop.data.recruitCost;
    }
}