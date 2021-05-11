using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Scrollbar))]
public class ScrollController : MonoBehaviour
{
    [SerializeField] private float step = 0.1f;
    private Scrollbar scrollbar;

    private void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    public void Increase()
    {
        if (math.abs(scrollbar.value - 1f) < 0.05f) return;
        scrollbar.value += step;
    }
    
    public void Decrease()
    {
        if (math.abs(scrollbar.value) < 0.05f) return;
        scrollbar.value -= step;
    }
}
