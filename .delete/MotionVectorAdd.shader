Shader "Hidden/WorldSystem/MotionVectorAdd"
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
//            Blend One Zero
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            // #include "../ShaderLibrary/GetCameraMotionVectors.hlsl"
            #include "../ShaderLibrary/TemporalAA.hlsl"

            float4x4 _ViewProjM_PerFrame;
            float4x4 _PrevViewProjM_PerFrame;
            float4x4 _InverseViewProjM_PerFrame;

            float2 GetMotionVector(float2 uv)
            {
                #if UNITY_REVERSED_Z
                    float depth = 0;
                #else
                    float depth = 1.0;
                #endif
                    
                    //float4 worldPos = float4(ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP), 1.0);
                    float4 worldPos = float4(ComputeWorldSpacePosition(uv, depth, _InverseViewProjM_PerFrame), 1.0);

                    float4 prevClipPos = mul(_PrevViewProjM_PerFrame, worldPos);
                    float4 curClipPos = mul(_ViewProjM_PerFrame, worldPos);

                    float2 prevPosCS = prevClipPos.xy / prevClipPos.w;
                    float2 curPosCS = curClipPos.xy / curClipPos.w;
                    
                    float2 velocity = (prevPosCS - curPosCS);

                    /*
                    Unnecessary? Double-checking the motion vector validation 
                    shows that motion vectors render incorrectly when this is enabled when using UNITY_MATRIX_I_VP...
                    */
                #if UNITY_UV_STARTS_AT_TOP
                        velocity.y = -velocity.y;
                #endif

                return velocity * 0.5;
            }

            
            // Texture2D _PREVIOUS_TAA_CLOUD_RESULTS;
            // Texture2D _CURRENT_TAA_FRAME;
            float _TAA_BLEND_FACTOR;
            TEXTURE2D(_PreviousMotionVector);SAMPLER(sampler_PreviousMotionVector);
            float2 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 previousMotionVector = SAMPLE_TEXTURE2D(_PreviousMotionVector,sampler_PreviousMotionVector,input.texcoord).rg * 2 - 1;
                // float2 motionVectors = GetMotionVector(input.texcoord);
                float2 CurrentMotionVectors = GetMotionVector(input.texcoord) + previousMotionVector;
                return CurrentMotionVectors * 0.5 + 0.5;

            }
            ENDHLSL
        }
    }
}