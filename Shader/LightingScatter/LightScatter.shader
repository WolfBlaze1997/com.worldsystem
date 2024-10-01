Shader "OccaSoftware/LSPP/LightScatter"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "LightScatterPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "LightScattering.hlsl"
            
            TEXTURE2D_X(_LightingScatter_OcclusionTex);

            float _LightingScatter_Density;
            bool _LightingScatter_UseSoftEdge;
            bool _LightingScatter_UseDynamicNoise;
            float _LightingScatter_MaxRayDistance;
            int _LightingScatter_NumSamples;
            float _LightingScatter_Saturation;
            float _LightingScatter_FalloffIntensity;
            
            float3 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 LightColorModif = Desaturate(_MainLightColor, _LightingScatter_Saturation);
                float3 Scatter = EstimateLightScattering(_LightingScatter_OcclusionTex, input.texcoord, _LightingScatter_Density,
                    _LightingScatter_UseSoftEdge, _LightingScatter_UseDynamicNoise, _LightingScatter_MaxRayDistance,
                    _LightingScatter_NumSamples, LightColorModif, _LightingScatter_FalloffIntensity);

                return Scatter;

            }
            ENDHLSL
        }
    }
}