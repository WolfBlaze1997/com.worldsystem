Shader "Hidden/WorldSystem/CloudMap_V1_1_20240604"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Cloud Map"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../ShaderLibrary/Math.hlsl"
            #include "../ShaderLibrary/TextureUtils.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float _Modeling_ShapeBase_Scale;
            float2 _Speed;
            float _Modeling_ShapeBase_Octaves;
            float _Modeling_ShapeBase_Freq;
            float _Modeling_ShapeBase_Gain;
            float _PrecipitationGlobal;
            float2 _MotionBase_Position;
            
            float _Modeling_Amount_CloudAmount;
            float _Render_MaxRenderDistance;
            bool _Modeling_Amount_UseFarOverlay;
            float _Modeling_Amount_OverlayCloudAmount;
            float _Modeling_Amount_OverlayStartDistance;
            
            float2 PositionWSToPositionUV(float2 positionWS, float maxDistance)
            {
                float2 UV = positionWS;
                UV *= rcp(maxDistance); // needs to be 1.0 / max distance
                UV *= 0.5;
                UV += 0.5;
                return UV;
            }

            float2 PositionUVToPositionWS(float2 UV, float maxDistance)
            {
                float2 positionWS = UV;
                positionWS -= 0.5;
                positionWS *= 2.0;
                positionWS *= maxDistance; // needs to be  max distance
                return positionWS;
            }
            

            float GetSDF(float2 positionWS, float2 cellPosition, float radiusWS)
            {
                return saturate((length(positionWS - cellPosition) - radiusWS) / radiusWS * -1.0);
            }

            float GetPrecipitationGlobal(float2 positionWS)
            {
                float map = GetLayeredPerlinNoise(_Modeling_ShapeBase_Octaves, positionWS * 0.00005 * _Modeling_ShapeBase_Scale - _MotionBase_Position, _Modeling_ShapeBase_Gain, _Modeling_ShapeBase_Freq, 314);
                map = os_Remap(1.0 - _PrecipitationGlobal, 1.0, 0.0, 1, map);
                map = saturate(map);
                map = pow(map, 0.5); // expose this value as a property, "Precipitation Strength"?.
                return map;
            }

            float3 GetPrecipitationData(float2 positionWS)
            {
                float sdf = 0.0;
                float state = GetPrecipitationGlobal(positionWS);
                float value = _PrecipitationGlobal;
                
                return float3(saturate(sdf), saturate(state), saturate(value));
            }

            float easeIn(float x)
            {
                return pow(x, 2.0);
            }

            float easeOut(float x)
            {
                return 1.0 - pow((1.0 - x), 2.0);
            }

            float easeInOut(float x)
            {
                return lerp(easeIn(x), easeOut(x), x);
            }

            float easeOutIn(float x)
            {
                return lerp(easeOut(x), easeIn(x), x);
            }
            
            
            float4 Fragment(Varyings input) : SV_Target
            {
    
                // Setup
                float maxCloudDistance = _Render_MaxRenderDistance;
                float2 positionWS = PositionUVToPositionWS(input.uv, maxCloudDistance) + _WorldSpaceCameraPos.xz;

                // Precipitation
                float w = _PrecipitationGlobal;
                float3 precipitation = GetPrecipitationData(positionWS);

                
                // Coverage
                float n = GetLayeredPerlinNoise(_Modeling_ShapeBase_Octaves, positionWS * 0.0001 * _Modeling_ShapeBase_Scale - _MotionBase_Position, _Modeling_ShapeBase_Gain, _Modeling_ShapeBase_Freq);

                
                float coverage = _Modeling_Amount_CloudAmount;
                
                if(_Modeling_Amount_UseFarOverlay)
                {
                    float distantCoverage = _Modeling_Amount_OverlayCloudAmount;
                    float distantCoverageStart = _Modeling_Amount_OverlayStartDistance;
                    distantCoverageStart = min(distantCoverageStart, maxCloudDistance);

                    float distantCoverageBlend = os_Map01(distantCoverageStart, maxCloudDistance, length(positionWS - _WorldSpaceCameraPos.xz));
                
                    distantCoverageBlend = 1.0 - distantCoverageBlend;
                    distantCoverageBlend *= distantCoverageBlend;
                    distantCoverageBlend = 1.0 - distantCoverageBlend;
                
                    coverage = lerp(coverage, distantCoverage, distantCoverageBlend);
                }
                
                coverage = lerp(coverage, 1.0, precipitation.z);

                n = os_Remap(1.0 - coverage, 1.0, 0.0, 1, n);
                n = saturate(n);
                n = pow(n, 0.5);// expose this value as a property, "Coverage Strength"?.
                
                
                float type = GetLayeredPerlinNoise(_Modeling_ShapeBase_Octaves, positionWS * 0.0001 * _Modeling_ShapeBase_Scale - _MotionBase_Position, _Modeling_ShapeBase_Gain, _Modeling_ShapeBase_Freq, 675);
                type = lerp(type, 1.0, precipitation.y);

                return float4(n, precipitation.y, type, 1.0);
            }
            ENDHLSL
        }

    }
}