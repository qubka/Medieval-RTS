using UnityEngine;

public class TimeBar : MonoBehaviour
{
    public void Stop()
    {
        Time.timeScale = 0f;
    }
    
    public void Normal()
    {
        Time.timeScale = 1f;
    }

    public void Fast()
    {
        Time.timeScale = 2f;
    }

    public void Rapid()
    {
        Time.timeScale = 4f;
    }
}
