using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Town : MonoBehaviour
{
    [SerializeField] private Text title;

    public void GenerateName()
    {
        var names = Resources.LoadAll<TownNames>("Names/");
        var db = names[Random.Range(0, names.Length)];
        var prefix = db.RandomPrefix;
        var anyfix = db.RandomAnyfix;
        while (string.Equals(prefix, anyfix, StringComparison.OrdinalIgnoreCase)){
            anyfix = db.RandomAnyfix;
        }
        var generated = prefix.FirstLetterCapital() + anyfix;
        title.text = generated;
        name = generated;
    }
}
