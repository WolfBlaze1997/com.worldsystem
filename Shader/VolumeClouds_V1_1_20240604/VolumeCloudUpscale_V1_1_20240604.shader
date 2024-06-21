Shader "Hidden/WorldSystem/UpscaleCloudsOptimize_V1_1_20240604"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Upscale Clouds"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/TextureUtils.hlsl"

            Texture2D _ScreenTexture;
            #define SIGMA 0.001

            // Enclosing the SIGMA * SIGMA expression in parentheses ensures 
            // that the multiplication is evaluated as a floating-point operation.
            #define SIGMA2 (SIGMA * SIGMA) 
            #define HALF_RES 0.5
            int _UseDepth;

            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Note: We can add other metrics for similarity testing.
                // For example, we can test for color or normal similarity.
                float2 texCoord = GetTexCoordSize(HALF_RES);
                float3 offset = float3(texCoord.x, texCoord.y, 0.0);
    
    if (_UseDepth)
    {
        float depth00 = _DitheredDepthTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0).r;
        float depth10 = _DitheredDepthTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord + offset.xz, 0).r;
        float depth01 = _DitheredDepthTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord + offset.zy, 0).r;
        float depth11 = _DitheredDepthTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord + offset.xy, 0).r;
                
        float depth = SampleSceneDepth(input.texcoord);
                
        float dF00 = exp((-1.0 / SIGMA2) * dot(depth, depth00));
        float dF10 = exp((-1.0 / SIGMA2) * dot(depth, depth10));
        float dF01 = exp((-1.0 / SIGMA2) * dot(depth, depth01));
        float dF11 = exp((-1.0 / SIGMA2) * dot(depth, depth11));
    
    
        float4 upsampleResults00 = _ScreenTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0) * dF00;
        float4 upsampleResults10 = _ScreenTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord + offset.xz, 0) * dF10;
        float4 upsampleResults01 = _ScreenTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord + offset.zy, 0) * dF01;
        float4 upsampleResults11 = _ScreenTexture.SampleLevel(altos_point_clamp_sampler, input.texcoord + offset.xy, 0) * dF11;
    
        float sumWeights = dF00 + dF10 + dF01 + dF11;
        float4 upsampleResults = (upsampleResults00 + upsampleResults10 + upsampleResults01 + upsampleResults11) / (sumWeights + 1e-7);
                
        return upsampleResults;
    }
    else
    {
        return _ScreenTexture.SampleLevel(altos_linear_clamp_sampler, input.texcoord, 0);
    }
                
            }
            ENDHLSL
        }
    }
}