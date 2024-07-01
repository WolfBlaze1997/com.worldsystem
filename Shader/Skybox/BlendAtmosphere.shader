Shader "Hidden/OccaSoftware/Altos/BlendAtmosphere"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Blend Atmosphere"
            Blend OneMinusSrcAlpha SrcAlpha
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TextureUtils.hlsl"
            
            float _BlendStart;
            float _Density;
            Texture2D _SkyTexture;


            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 skyColor = _SkyTexture.SampleLevel(altos_linear_clamp_sampler, input.texcoord, 0).rgb;
                float rawDepth = SampleSceneDepth(input.texcoord);
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depth01 = Linear01Depth(rawDepth, _ZBufferParams);
                float eyeDistance = eyeDepth * length(ScreenToViewVector(input.texcoord));
                eyeDistance = max(eyeDistance - _BlendStart, 0);
                float d = _Density * eyeDistance;
                float ex = saturate(1.0f / exp(d));
                if (depth01 >= 1.0)
                {
                    return half4(0,0,0,1);
                }
                return half4(skyColor,ex);
            }
            ENDHLSL
        }
    }
}