Shader "Hidden/WorldSystem/TemporalAA"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Temporal AA"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/GetCameraMotionVectors.hlsl"
            #include "../ShaderLibrary/TemporalAA.hlsl"
            // #include "RenderCloudsPass.hlsl"

            Texture2D _PREVIOUS_TAA_CLOUD_RESULTS;
            Texture2D _CURRENT_TAA_FRAME;
            float _TAA_BLEND_FACTOR;
            // Texture2D altos_EdgeData;

            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // float edge = altos_EdgeData.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0).r;
                // if(edge > 0.01)
                // {
                //     //return float4(1, 0, 0, 1);
                //     //return SampleClouds(input.texcoord, 1, 0).color;
                // }
                
                float2 motionVectors = GetMotionVector(input.texcoord);
                return TemporalAA(_PREVIOUS_TAA_CLOUD_RESULTS, _CURRENT_TAA_FRAME, input.texcoord, _TAA_BLEND_FACTOR, motionVectors);
            }
            ENDHLSL
        }
    }
}