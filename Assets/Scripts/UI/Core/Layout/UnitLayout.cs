using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitLayout : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private Image type;
    [SerializeField] private TMP_Text amount;

    public Troop Troop { get; private set; }
    
    public void SetTroop(Troop troop)
    {
        Troop = troop;
        var data = troop.data;
        portrait.sprite = data.bigIcon;
        type.sprite = data.classIcon;
        amount.text = troop.size.ToString();
    }
}