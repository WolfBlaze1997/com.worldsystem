Shader "Unlit/FixupLateBlit"
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
            
            float _FOVScale;
            TEXTURE2D(_FixupLateTarget);SAMPLER(sampler_FixupLateTarget);

            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 UV = input.texcoord * _FOVScale + (1 - _FOVScale) / 2;
                return SAMPLE_TEXTURE2D(_FixupLateTarget,sampler_FixupLateTarget,UV);
            }
            ENDHLSL
        }
    }
}
