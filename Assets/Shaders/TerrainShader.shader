Shader "Custom/TerrainShader"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        const static int maxColorCount = 16;
        const static float epsilon = 1E-4;
        
        int colorCount;
        float3 baseColors[maxColorCount];
        float baseStartHeights[maxColorCount];
        float baseBlends[maxColorCount];

        float minHeight;
        float maxHeight;
        
        /*half   _Glossiness;
        half   _Metallic;
        fixed4 _Color;
		half   _Alpha;*/

        struct Input {
            float3 worldPos;
        };
        
        float inverseLerp(float a, float b, float value) {
            return saturate((value - a) / (b - a));
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            for (int i = 0; i < colorCount; i++) {
                float halfBlend = baseBlends[i] / 2;
                float drawStrength = inverseLerp(-halfBlend - epsilon, halfBlend, heightPercent - baseStartHeights[i]);
                o.Albedo = o.Albedo * (1 - drawStrength) + baseColors[i] * drawStrength;
            }
            //o.Metallic   = _Metallic;
            //o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}