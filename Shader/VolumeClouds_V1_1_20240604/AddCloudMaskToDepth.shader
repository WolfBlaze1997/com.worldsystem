Shader "Unlit/AddCloudMaskToDepth"
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
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CloudNoFixupTex);SAMPLER(sampler_CloudNoFixupTex);

            float Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 UV = _FOVScale < 0.01 ? input.texcoord : input.texcoord * _FOVScale + (1 - _FOVScale) / 2;
                return (max(SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture,sampler_PointClamp,input.texcoord, 0).r, saturate(SAMPLE_TEXTURE2D_LOD(_CloudNoFixupTex,sampler_PointClamp,UV, 0).a - 0.2) * 0.005));
            }
            ENDHLSL
        }
    }
}
