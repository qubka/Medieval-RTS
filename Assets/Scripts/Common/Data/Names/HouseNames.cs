using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Names/House", order = 0)]
[Serializable]
public class HouseNames : ScriptableObject
{
    public string[] names1;
    public string[] names2;
    public string[] names3;
    public string[] names4;
    public string[] names5;
    public string[] names6;
    public string[] names7;
    public string[] names8;
    public string[] names9;
    public string[] names0;
    
    public string RandomName()
    {
        string names;
        var i = Random.Range(0, 11);
        if (i == 10) {
            names = names9[Random.Range(0, names9.Length)] + names0[Random.Range(0, names0.Length)];
        } else if (i < 3) {
            names = names1[Random.Range(0, names1.Length)] + names2[Random.Range(0, names2.Length)];
        } else if (i < 6) {
            names = names7[Random.Range(0, names7.Length)] + names8[Random.Range(0, names8.Length)];
        } else {
            var rnd = Random.Range(0, names3.Length);
            var rnd2 = Random.Range(0, names4.Length);
            var rnd3 = Random.Range(0, names5.Length);
            var rnd4 = Random.Range(0, names4.Length);
            if (rnd2 > 4) {
                while (rnd4 > 4) {
                    rnd4 = Random.Range(0, names4.Length);
                }
            }
            var rnd5 = Random.Range(0, names6.Length);
            names = names3[rnd] + names4[rnd2] + names5[rnd3] + names4[rnd4] + names6[rnd5];
        }
        return names.FirstLetterCapital();
    }
    
}