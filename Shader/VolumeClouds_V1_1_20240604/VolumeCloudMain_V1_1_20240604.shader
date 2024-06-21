Shader "Hidden/WorldSystem/RenderCloudsOptimize_V1_1_20240604"
{
//    Properties
//    {
//        _Render_BlueNoiseArray ("_Render_BlueNoiseArray", 2DArray) = "" {}
//    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Render Clouds"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            #pragma require 2darray
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            int _UseDownscaledDepth;
            int _UseReprojection;
            int _UseDepth;
            
            #include "VolumeCloudMainPass_V1_1_20240604.hlsl"
            
            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return SampleClouds(input.texcoord, _UseDownscaledDepth, _UseReprojection, _UseDepth).color;
                
            }
            ENDHLSL
        }
    }
}