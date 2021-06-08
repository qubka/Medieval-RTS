using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecruitLayout : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Material gray;
    [SerializeField] private GameObject bottom;
    [SerializeField] private Button button;
    [SerializeField] private Image portrait;
    [SerializeField] private Image type;
    [SerializeField] private TextMeshProUGUI amount;
    [SerializeField] private TextMeshProUGUI cost;

    [HideInInspector] public Troop troop;
    
    public void SetTroop(Troop troop)
    {
        this.troop = troop;
        var data = troop.data;
        portrait.sprite = data.bigIcon;
        type.sprite = data.classIcon;
        cost.text = data.recruitCost.ToString();
        cost.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bottom.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bottom.SetActive(false);
    }

    public void Pressed()
    {
        ArmyManager.Instance.AddTroop(troop.Clone());
        OnUpdate();
    }

    public void OnUpdate()
    {
        var player = Game.Player;
        if (player.leader.money < troop.data.recruitCost) {
            cost.color = Color.red;
            portrait.material = gray;
            button.interactable = false;
        } else if (player.troops.Count >= ArmyManager.Instance.maxTroops) {
            cost.color = Color.white;
            portrait.material = gray;
            button.interactable = false;
        } else {
            cost.color = Color.white;
            portrait.material = null;
            button.interactable = true;
        }
    }
}
