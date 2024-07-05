Shader "Hidden/WorldSystem/CloudShadowTemporalAAOptimize_V1_1_20240604"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "CloudShadowTemporalAA"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/TextureUtils.hlsl"

            Texture2D _PREVIOUS_TAA_CLOUD_SHADOW;
            Texture2D _CURRENT_TAA_CLOUD_SHADOW;
            float4 _ShadowmapResolution;
            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


                // Quick box blur
                const float2 offsets[4] = { float2(-1.0,0), float2(1.0,0), float2(0,-1.0), float2(0,1.0)};
                float4 current = float4(0,0,0,0);
                current += _CURRENT_TAA_CLOUD_SHADOW.SampleLevel(altos_linear_clamp_sampler, input.texcoord + offsets[0] * _ShadowmapResolution.z * 2.0, 0);
                current += _CURRENT_TAA_CLOUD_SHADOW.SampleLevel(altos_linear_clamp_sampler, input.texcoord + offsets[1] * _ShadowmapResolution.z * 2.0, 0);
                current += _CURRENT_TAA_CLOUD_SHADOW.SampleLevel(altos_linear_clamp_sampler, input.texcoord + offsets[2] * _ShadowmapResolution.z * 2.0, 0);
                current += _CURRENT_TAA_CLOUD_SHADOW.SampleLevel(altos_linear_clamp_sampler, input.texcoord + offsets[3] * _ShadowmapResolution.z * 2.0, 0);
                current /= 4;

                // Get Previous Shadowmap Data
                float4 previous = _PREVIOUS_TAA_CLOUD_SHADOW.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0);
                
                // Blend
                return lerp(current, previous, 0.97);
            }
            ENDHLSL
        }
    }
}