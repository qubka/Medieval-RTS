using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
    
    #region Local
    
    private TableObject<IGameObject> objectTable;
    private TableObject<Battle> battleTable;
    
#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private float nextHoverTime;
    private bool isVisible = true;
    
    #endregion
    
    private void Awake()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        battleTable = BattleTable.Instance;
        objectTable = ObjectTable.Instance;
        
        // Add a battle to the tables
        battleTable.Add(gameObject, this);
        objectTable.Add(gameObject, this);
        
        // Create some staff
        battleIcon = Instantiate(Manager.global.battleIcon).GetComponent<BattleIcon>();
        iconTransform = battleIcon.transform;
        worldTransform = transform;
        
        // Parent a bar to the screen
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;
    }

    private IEnumerator Start()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnUpdate()
    {
        var distance = Vector.Distance(worldTransform.position, Game.Player.position);
        SetVisibility(distance <= (TimeController.Now.IsDay() ? 100f : 50f));
        battleIcon.OnUpdate(this);
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
                
                var troopCount = party.TroopCount;
                defendCount += troopCount;

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

                if (troopCount > 0) {
                    builder
                        .Append(' ')
                        .Append('(')
                        .Append(troopCount)
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
                
                var troopCount = party.TroopCount;
                attackCount += troopCount;

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

                if (troopCount > 0) {
                    builder
                        .Append(' ')
                        .Append('(')
                        .Append(troopCount)
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