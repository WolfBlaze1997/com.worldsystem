Shader "Unlit/FixupLateSplitFrame"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Reproject"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            // float _FOVScale;
            TEXTURE2D(_SplitFrameRT);SAMPLER(sampler_SplitFrameRT);
            TEXTURE2D(_MotionVectorAdd);SAMPLER(sampler_MotionVectorAdd);

            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 UV = input.texcoord;
                float2 motionVectors = SAMPLE_TEXTURE2D(_MotionVectorAdd,sampler_MotionVectorAdd,UV).rg * 2 - 1;
                float2 HistUV = UV + motionVectors;
                
                return SAMPLE_TEXTURE2D(_SplitFrameRT,sampler_SplitFrameRT,HistUV);
            }
            ENDHLSL
        }
    }
}
