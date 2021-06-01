using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using BehaviorDesigner.Runtime.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityJSON;

[RequiredComponent(typeof(TextMeshProUGUI))]
public class ClockController : MonoBehaviour {

    private TextMeshProUGUI timeMesh;

    private void Start()
    {
        timeMesh = GetComponent<TextMeshProUGUI>();
        StartCoroutine(Tick());
    } 
    
    private IEnumerator Tick()
    {
        while (true) {
            yield return new WaitForSecondsRealtime(0.1f);
            timeMesh.text = ToString() + Environment.NewLine + StringExtention.GetPrettyName(Game.Now.GetTimeOfDay());
        }
    }

    public override string ToString() => Game.Now.ToString("d MMMM, yyyy", CultureInfo.InvariantCulture);
}