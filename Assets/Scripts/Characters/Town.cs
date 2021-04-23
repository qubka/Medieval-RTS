using System;
using UnityEngine;
using UnityEngine.UI;

public class Town : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private Character owner;

    [SerializeField] private Image flag;
    [SerializeField] private Image shadow;
    [SerializeField] private Text title;

    private void Start()
    {
        shadow.color = faction.color;
        Manager.townTable.Add(gameObject, this);
    }

#if UNITY_EDITOR    
    public void GenerateName(TownNames names)
    {
        var prefix = names.RandomPrefix;
        var anyfix = names.RandomAnyfix;
        while (string.Equals(prefix, anyfix, StringComparison.OrdinalIgnoreCase)){
            anyfix = names.RandomAnyfix;
        }
        title.text = prefix.FirstLetterCapital() + anyfix;
    }
#endif    
}
