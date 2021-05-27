using UnityEngine;

//[ExecuteInEditMode]
public class LightManager : SingletonObject<LightManager>
{
   [SerializeField, Range(0, 24)] private float timeOfDay;
   [SerializeField] private LightPreset preset;
   [SerializeField] private Light directionalLight;

   private void Update()
   {
      if (!Application.isPlaying) {
         UpdateLighting(timeOfDay / 24f);
      }
   }

   public void UpdateLighting(float timePercent)
   {
      RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
      RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);
      if (directionalLight) {
         directionalLight.color = preset.directionColor.Evaluate(timePercent);
         directionalLight.transform.localEulerAngles = new Vector3(timePercent * 360f - 90f, 120f, 0f);
      }
   }
   
   public void OnValidate()
   {
      if (directionalLight)
         return;

      var sun = RenderSettings.sun;
      if (sun) {
         directionalLight = sun;
      } else {
         var lights = FindObjectsOfType<Light>();
         foreach (var light in lights) {
            if (light.type == LightType.Directional) {
               directionalLight = light;
               return;
            }
         }
      }
   }
}