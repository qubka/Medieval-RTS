using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : SingletonObject<ScreenFader>
{
    [SerializeField] private Texture2D fadeOutTexture;
    [SerializeField] private float fadeSpeed = 0.8f;
    [SerializeField] private float minAlpha = 0.0f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private int drawDepth = -1000;
 
    private float alpha = 1.0f;
    private int fadeDir = -1;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BeginFade(-1);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnGUI()
    {
        alpha += fadeDir * fadeSpeed * Time.unscaledDeltaTime;
        alpha = math.clamp(alpha, minAlpha, maxAlpha);

        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        GUI.depth = drawDepth;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fadeOutTexture);
        
        if (alpha == minAlpha && fadeDir == -1 || alpha == maxAlpha && fadeDir == 1) {
            enabled = false;
        }
    }
 
    public void BeginFade(int direction)
    {
        fadeDir = direction;
        enabled = true;
    }
}