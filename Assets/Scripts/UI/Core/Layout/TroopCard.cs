using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopCard : MonoBehaviour
{
    [SerializeField] private GameObject bottom;
    [SerializeField] private Image icon;
    [SerializeField] private Image type;
    [SerializeField] private TextMeshProUGUI amount;
    [SerializeField] private Image shadow;
    [HideInInspector] public Troop troop;
    
    public void SetTroop(Troop troop)
    {
        this.troop = troop;
        var data = troop.data;
        icon.sprite = data.bigIcon;
        type.sprite = data.classIcon;
        amount.text = troop.size + "/" + data.maxCount;
    }

    public void Select(bool value)
    {
        StopAllCoroutines();
        StartCoroutine(shadow.Fade(value ? 1f : 0f, 0.15f));
        bottom.SetActive(value);
    }
}