using System;
using UnityEngine;

public class TimeBar : MonoBehaviour
{
    public bool isPaused { get; set; }
    [SerializeField] private GameObject status;
    [SerializeField] private Material play;
    [SerializeField] private Material stop;
    [SerializeField] private Material fast;
    [SerializeField] private Material rapid;

    private void Awake()
    {
        Normal();
    }

    public void Normal()
    {
        Time.timeScale = 1f;
        Switch(true);
    }
    
    public void Stop()
    {
        Time.timeScale = 0f;
        Switch(isStop: true);
    }

    public void Fast()
    {
        Time.timeScale = 2f;
        Switch(isFast: true);
    }

    public void Rapid()
    {
        Time.timeScale = 4f;
        Switch(isRapid: true);
    }

    public void Switch(bool isPlay = false, bool isStop = false, bool isFast = false, bool isRapid = false)
    {
        var id = Manager.GrayscaleAmount;
        play.SetFloat(id, isPlay ? 1f : 0f);
        stop.SetFloat(id, isStop ? 1f : 0f);
        fast.SetFloat(id, isFast ? 1f : 0f);
        rapid.SetFloat(id, isRapid ? 1f : 0f);
        isPaused = isStop;
        status.SetActive(isStop);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (isPaused || Time.timeScale > 1f) {
                Normal();
            } else {
                Stop();
            }
        }
    }
}
