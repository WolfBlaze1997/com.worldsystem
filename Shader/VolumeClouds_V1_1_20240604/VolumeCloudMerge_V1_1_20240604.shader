Shader "Hidden/WorldSystem/MergeCloudsOptimize_V1_1_20240604"
{
    SubShader
    {
        
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Merge Clouds"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/TextureUtils.hlsl"

            Texture2D _ScreenTexture;
            Texture2D _altos_CloudTexture;


            #define DEBUG_CLOUD_SHADOWS 0
            #if DEBUG_CLOUD_SHADOWS
            Texture2D _CLOUD_SHADOW_CURRENT_FRAME;
            #endif

            SamplerState point_clamp_sampler;
            SamplerState linear_clamp_sampler;
            
            int _UseDownscaledDepth;
            int _UseReprojection;
            int _UseDepth;

            float3 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(input.texcoord);
                #else
                    float depth = 1.0 - SampleSceneDepth(input.texcoord);
                #endif
    
                float3 screenColor = _ScreenTexture.SampleLevel(point_clamp_sampler, input.texcoord, 0).rgb;
                float4 cloudColor = _altos_CloudTexture.SampleLevel(point_clamp_sampler, input.texcoord, 0);
                
                if (_UseDepth == 1 || depth <= 0.0)
                {
                    screenColor = screenColor.rgb * (1.0 - cloudColor.a) + cloudColor.rgb;
                }
    
                #if DEBUG_CLOUD_SHADOWS
                if (input.texcoord.x > 0.8 && input.texcoord.y > 0.8)
                {
                    return saturate(_CLOUD_SHADOW_CURRENT_FRAME.SampleLevel(linear_clamp_sampler, (input.texcoord.xy - 0.8) * 5, 0).rgb);
                }
                #endif
    
                return screenColor;
            }
            ENDHLSL
        }
    }
}