Shader "Unlit/FixupLate"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/GetCameraMotionVectors.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TextureUtils.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TemporalAA.hlsl"
            #include "VolumeCloudMainPass_V1_1_20240604.hlsl"


            TEXTURE2D_X(_BlitTexture);
            TEXTURECUBE(_BlitCubeTexture);

            uniform float4 _BlitScaleBias;
            uniform float4 _BlitScaleBiasRt;
            uniform float _BlitMipLevel;
            uniform float2 _BlitTextureSize;
            uniform uint _BlitPaddingSize;
            uniform int _BlitTexArraySlice;
            uniform float4 _BlitDecodeInstructions;

            #if SHADER_API_GLES
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #else
            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #endif

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
            #endif

                output.positionCS = pos;
                output.texcoord   = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
                return output;
            }

            
            float2 GetMotionVector_Modif(float2 uv)
            {
               // #if UNITY_REVERSED_Z => Depth Buffer Range = [1,0]
                // #else => Depth Buffer Range = [0,1]
                
            #if UNITY_REVERSED_Z
                float depth = 0;
            #else
                float depth = 1.0;
            #endif
                
                //float4 worldPos = float4(ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP), 1.0);
                // float4 worldPos = float4(ComputeWorldSpacePosition(uv, depth, _InverseViewProjM), 1.0);
                float4 worldPos = float4(ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP), 1.0);

                float4 prevClipPos = mul(_PrevViewProjM, worldPos);
                float4 curClipPos = mul(_ViewProjM, worldPos);

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

            TEXTURE2D(_ActiveTarget);SAMPLER(sampler_ActiveTarget);

            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 UV = input.texcoord;
                
                float2 motionVectors = GetMotionVector_Modif(input.texcoord);
//                 float2 motionVectors1 = GetMotionVector_Modif(float2(0,0));
//                 float2 motionVectors2 = GetMotionVector_Modif(float2(1,0));
//                 float2 motionVectors3 = GetMotionVector_Modif(float2(0,1));
//                 float2 motionVectors4 = GetMotionVector_Modif(float2(1,1));
// float2 motionVectors = (motionVectors1 + motionVectors2 + motionVectors3 + motionVectors4) / 4;
                
                
                float2 HistUV = UV + motionVectors*1.0;
                bool isValidHistUV = IsUVInRange01(HistUV);
                // return half4(motionVectors,0,0);
                // return UV.xxxx;
                // if(isValidHistUV)
                // {
                    return SAMPLE_TEXTURE2D(_ActiveTarget,sampler_ActiveTarget,HistUV);
                // }
                // else
                // {
                //     return half4(1,0,0,1);
                // }
                
            }
            ENDHLSL
        }
    }
}
