using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Battle : MonoBehaviour, IGameObject
{
    [ReadOnly] public List<Party> attackers = new List<Party>();
    [ReadOnly] public List<Party> defenders = new List<Party>();

    [Header("Children References")]
    //[Space(10f)]
    //[Space(10f)]
    [HideInInspector] public BattleIcon battleIcon;
    [HideInInspector] public Transform iconTransform;
    [HideInInspector] public Transform worldTransform;
    
    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);
    public float strongerCoefficient = 1.4f;
    
    #region Local
    
    private TableObject<IGameObject> objectTable;
    private TableObject<Battle> battleTable;
    
#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private CameraController cameraController;
    private float nextHoverTime;
    private bool isVisible = true;
    
    #endregion
    
    private void Awake()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        cameraController = Manager.cameraController;
        battleTable = BattleTable.Instance;
        objectTable = ObjectTable.Instance;
        
        // Add a battle to the tables
        battleTable.Add(gameObject, this);
        objectTable.Add(gameObject, this);
        
        // Create some staff
        battleIcon = Instantiate(Manager.global.battleIcon).GetComponent<BattleIcon>();
        battleIcon.SetActive(false);
        iconTransform = battleIcon.transform;
        worldTransform = transform;
        
        // Parent a bar to the screen
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;
    }

    private void Start()
    {
        StartCoroutine(Tick());
        StartCoroutine(Fight());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator Fight()
    {
        while (true) {
            OnFight();
            yield return new WaitForSeconds(3f);
        }
    }

    private void OnUpdate()
    {
        var distance = Vector.Distance(worldTransform.position, Game.Player.position);
        SetVisibility(distance <= (TimeController.Now.IsDay() ? 100f : 50f));
        battleIcon.OnUpdate(this);
    }

    private void OnFight()
    {
        // Do logic
        var attacker = attackers.FindStrongest();
        var defender = defenders.FindStrongest();

        if (!attacker || !defender) {
            Exit();
            return;
        }

        Fight(attacker, defender);

        // If there not troops, exit now
        if (!attackers.FindStrongest() || !defenders.FindStrongest()) {
            Exit();
        }
    }
    
    public void Update()
    {
        if (isVisible) {
            // Calculate position for the ui bar
            var pos = worldTransform.position;
            pos.y += canvasHeight;
            pos = camera.WorldToScreenPoint(pos);
        
            // If the army is behind the camera, or too far away from the player, make sure to hide the bar completely
            if (pos.z < 0f) {
                battleIcon.SetActive(false);
            } else {
                iconTransform.position = pos;
                battleIcon.SetActive(true);
            }
            
            // Icon UI mode
            battleIcon.SetEnabled(cameraController.zoomPos < 0.5f);
        }
    }

    public void Load(BattleSave save)
    {
        if (save != null) {
            attackers = Party.All.Where(p => save.attackers.Contains(p.leader.id)).ToList();
            defenders = Party.All.Where(p => save.defenders.Contains(p.leader.id)).ToList();
        } else {
            for (var i = 0; i < attackers.Count; i++) {
                attackers[i] = Party.All.First(p => p.leader.id == attackers[i].leader.id);
            }
            for (var i = 0; i < defenders.Count; i++) {
                defenders[i] = Party.All.First(p => p.leader.id == defenders[i].leader.id);
            }
        }
    }

    public void Destroy()
    {
        battleTable.Remove(gameObject);
        objectTable.Remove(gameObject);
        
        DestroyImmediate(battleIcon.gameObject);
        DestroyImmediate(gameObject);
    }

    public bool Contains(Party party) => ContainsAttacker(party) || ContainsDefenders(party);
    public bool ContainsAttacker(Party party) => attackers.Contains(party);
    public bool ContainsDefenders(Party party) => defenders.Contains(party);

    public void AddAsAlly(Party main, Party additional)
    {
        if (ContainsAttacker(main)) {
            attackers.Add(additional);
        } else if (ContainsDefenders(main)) {
            defenders.Add(additional);
        } else {
            throw new Exception("Party not exist in the battle!");
        }
        additional.army.SetBattle(this);
    }
    
    public void AddAsEnemy(Party main, Party additional)
    {
        if (ContainsAttacker(main)) {
            defenders.Add(additional);
        } else if (ContainsDefenders(main)) {
            attackers.Add(additional);
        } else {
            throw new Exception("Party not exist in the battle!");
        }
        additional.army.SetBattle(this);
    }
    
    public static Battle Create(Party attacker, Party defender)
    {
        var position = (attacker.position + defender.position) / 2f;
        position.y += 2.5f;
        var battle = Instantiate(Manager.global.battlePrefab, position, Quaternion.identity).GetComponent<Battle>();
        battle.attackers.Add(attacker);
        battle.defenders.Add(defender);
        attacker.army.SetBattle(battle);
        defender.army.SetBattle(battle);
        return battle;
    }
    
    public void SetVisibility(bool value)
    {
        if (isVisible == value)
            return;
        
        battleIcon.SetActive(value);

        isVisible = value;
    }
    
    #region Base

    public int GetID()
    {
        return GetInstanceID();
    }
    
    public Vector3 GetPosition()
    {
        return worldTransform.position;
    }

    public Transform GetIcon()
    {
        return iconTransform;
    }

    public UI GetUI()
    {
        return UI.Battle;
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    #endregion

    #region Tooltip

    public void OnMouseOver()
    {
        if (Manager.IsPointerOnUI || !isVisible) {
            Manager.fixedPopup.HideInfo();
            return;
        }
        
        var currentTime = Time.unscaledTime;
        if (currentTime > nextHoverTime) {

            var builder = new StringBuilder();
            
            var defendCount = 0;
            
            foreach (var party in defenders) {
                builder
                    .Append("<color=#")
                    .Append(party.leader.faction.color.ToHexString())
                    .Append('>');
                
                var troopSize = party.TroopSize;
                defendCount += troopSize;

                switch (party.leader.type) {
                    case CharacterType.Player:
                    case CharacterType.Noble:
                        builder
                            .Append(party.leader.title)
                            .Append(' ')
                            .Append(party.leader.surname)
                            .Append("'s Party");
                        break;
                    case CharacterType.Bandit:
                        builder.Append("Marauders");
                        break;
                    case CharacterType.Peasant:
                        builder.Append("Villagers");
                        break;
                }

                if (troopSize > 0) {
                    builder
                        .Append(' ')
                        .Append('(')
                        .Append(troopSize)
                        .Append(')')
                        .Append("</color>") // 
                        .AppendLine();
                } else {
                    builder
                        .Append("</color>") // 
                        .AppendLine();
                }
            }

            builder
                .Append("<color=#ffffffff>VS</color>")
                .AppendLine();

            var attackCount = 0;
            
            foreach (var party in attackers) {
                builder
                    .Append("<color=#")
                    .Append(party.leader.faction.color.ToHexString())
                    .Append('>');
                
                var troopSize = party.TroopSize;
                attackCount += troopSize;

                switch (party.leader.type) {
                    case CharacterType.Player:
                    case CharacterType.Noble:
                        builder
                            .Append(party.leader.title)
                            .Append(' ')
                            .Append(party.leader.surname)
                            .Append("'s Party");
                        break;
                    case CharacterType.Bandit:
                        builder.Append("Marauders");
                        break;
                    case CharacterType.Peasant:
                        builder.Append("Villagers");
                        break;
                }

                if (troopSize > 0) {
                    builder
                        .Append(' ')
                        .Append('(')
                        .Append(troopSize)
                        .Append(')')
                        .Append("</color>") // 
                        .AppendLine();
                } else {
                    builder
                        .Append("</color>") // 
                        .AppendLine();
                }
            }    
            
            var description = builder.ToString();
            
            builder.Clear();

            builder
                .Append("<color=#ffffffff>")
                .Append("BATTLE")
                .AppendLine()
                .Append('(')
                .Append(defendCount)
                .Append(" vs ")
                .Append(attackCount)
                .Append(')')
                .Append("</color>");
            
            var caption = builder.ToString();
            
            Manager.dynamicPopup.DisplayInfo(caption, description);
            
            nextHoverTime = currentTime + 1f;
        }
    }
    
    private void OnMouseExit()
    {
        nextHoverTime = 0f;
        Manager.dynamicPopup.HideInfo();
    }

    #endregion

    #region Simulation

    public void Fight(Party party1, Party party2) 
    {
        //Debug.Log("Terrain: " + terrain);
		
        // ranged fight
        RangedUnitsShoot(party1, party2);
        RangedUnitsShoot(party2, party1);
		
        // melee fight
        MeleeUnitsFight(party1, party2);
        MeleeUnitsFight(party2, party1);
    }
    
    /**
	 * First stage of battle where ranged units shoot.
	 */
    private void RangedUnitsShoot(Party attacker, Party defender) 
    {
        if (attacker.TroopCount == 0 || defender.TroopCount == 0) {
            return;
        }
        
        var rangeTroops = attacker.troops.Where(t => t.data.rangeWeapon);
        var targetTroops = defender.troops;
        
        foreach (var rangedTroop in rangeTroops) {
            //var terrainModificator = terrain.getModificatorForUnit(rangedUnit);
            var targetTroop = targetTroops[Random.Range(0, targetTroops.Count)];

            for (var i = 0; i < rangedTroop.size; i++) {
                if (Random.Range(0, 100) < rangedTroop.data.accuracy/* getGeneralCoeficient() * terrainModificator*/) {
                    targetTroop.size--;
                }
            }
        }

        defender.Validate();
    }

    /**
	 * Second part of the battle where melee units fight to death.
	 */
    private void MeleeUnitsFight(Party attacker, Party defender)
    {
        if (attacker.TroopCount == 0 || defender.TroopCount == 0) {
            return;
        }
        
        var meleeTroops = attacker.troops.Where(t => !t.data.rangeWeapon);
        var targetTroops = defender.troops;
        
        foreach (var meleeTroop in meleeTroops) {
            //var terrainModificator = terrain.getModificatorForUnit(rangedUnit);
            var targetTroop = targetTroops[Random.Range(0, targetTroops.Count)];

            for (var i = 0; i < meleeTroop.size; i++) {
                float strCoef;
                if (meleeTroop.IsStrongAgainst(targetTroop)) {
                    strCoef = strongerCoefficient;
                } else if (targetTroop.IsStrongAgainst(meleeTroop)) {
                    strCoef = 1f / strongerCoefficient;
                } else {
                    strCoef = 1f;
                }
				
                //var terrainModificator1 = terrain.getModificatorForUnit(unit1);
                //var terrainModificator2 = terrain.getModificatorForUnit(unit2);
                
                var damage1 = Random.Range(0, (int) (meleeTroop.data.meleeAttack * strCoef /*getGeneralCoeficient() * terrainModificator1*/));
                var damage2 = Random.Range(0, (int) (targetTroop.data.meleeAttack/* terrainModificator2*/));
                
                if (damage1 > damage2) {
                    targetTroop.size--;
                }
            }
        }

        defender.Validate();
    }

    private void Exit()
    {
        // stop
        Destroy();
        foreach (var party in attackers) {
            party.army.SetBattle(null);
        }
        foreach (var party in defenders) {
            party.army.SetBattle(null);
        }
    }

    #endregion

    public void Begin()
    {
        
    }
}

[Serializable]
public class BattleSave
{
    public int[] attackers;
    public int[] defenders;
    public Vector3 position;

    public BattleSave(Battle battle)
    {
        attackers = battle.attackers.Select(p => p.leader.id).ToArray();
        defenders = battle.defenders.Select(p => p.leader.id).ToArray();
        position = battle.worldTransform.position;
    }
}