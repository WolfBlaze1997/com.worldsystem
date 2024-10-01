Shader "OccaSoftware/LSPP/Merge"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "MergePass"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
            SamplerState linear_clamp_sampler;

            // #include "CrossUpsampling.hlsl"
            
            TEXTURE2D_X(_LightingScatter_CameraTex);
            TEXTURE2D_X(_LightingScatter_ScatterTex);

            
            float3 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 screenColor = SAMPLE_TEXTURE2D_X_LOD(_LightingScatter_CameraTex, linear_clamp_sampler, input.texcoord, 0).rgb;
                // 效果不明显简化
                // float3 upscaleResults = CrossSample(_LightingScatter_ScatterTex, input.texcoord, _ScreenParams.xy * 0.5, 2.0);
                float3 upscaleResults = SAMPLE_TEXTURE2D_X_LOD(_LightingScatter_ScatterTex, linear_clamp_sampler, input.texcoord, 0).rgb;
                
                return screenColor + upscaleResults;
            }
            ENDHLSL
        }
    }
}