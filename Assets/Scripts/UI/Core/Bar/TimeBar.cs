using UnityEngine;

public class TimeBar : MonoBehaviour
{
    [SerializeField] private GameObject status;
    [SerializeField] private Material play;
    [SerializeField] private Material stop;
    [SerializeField] private Material fast;
    [SerializeField] private Material rapid;
    [SerializeField] private bool stopOnAwake;
    
    public bool isPaused { get; set; }
    public bool isLocked { get; set; }
    
    private void Awake()
    {
        if (stopOnAwake) {
            Stop();
        } else {
            Normal();
        }
    }

    public void Normal()
    {
        if (isLocked) 
            return;
        
        Time.timeScale = 1f;
        Switch(true);
    }
    
    public void Stop()
    {
        if (isLocked) 
            return;
        
        Time.timeScale = 0f;
        Switch(isStop: true);
    }

    public void Fast()
    {
        if (isLocked) 
            return;
        
        Time.timeScale = 2f;
        Switch(isFast: true);
    }

    public void Rapid()
    {
        if (isLocked) 
            return;
        
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
        if (isLocked) 
            return;
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (Time.timeScale == 0f || Time.timeScale > 1f) {
                Normal();
            } else {
                Stop();
            }
        }
    }
}
