Shader "Hidden/WorldSystem/RenderShadowsToScreenOptimize_V1_1_20240604"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Render Shadows to Screen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/TextureUtils.hlsl"

            Texture2D _ScreenTexture;
            Texture2D _CloudScreenShadows;

            float3 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 screenColor = _ScreenTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0).rgb;
                float3 shadowSample = _CloudScreenShadows.SampleLevel(altos_linear_clamp_sampler, input.texcoord, 0).rgb;
                return screenColor * shadowSample;
            }
            ENDHLSL
        }
    }
}