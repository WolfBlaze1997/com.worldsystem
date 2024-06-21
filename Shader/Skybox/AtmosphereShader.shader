Shader "Hidden/WorldSystem/Sky/AtmosphereShader"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "ATMOSPHERE"
            Tags { "LightMode" = "Altos" }
            Cull Off
            Blend One One
            ZWrite Off
            ZTest Always
            ZClip True

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TextureUtils.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/Math.hlsl"
            


            struct Attributes
            {
                float4 positionOS    : POSITION;
                
            };

            struct Varyings
            {
                float4 positionHCS   : SV_POSITION;
                float4 positionWS    : TEXCOORD0;
                float4 positionSS    : TEXCOORD1;
                float3 viewDirection : TEXCOORD2;
            };


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS.xyz);
                OUT.positionSS = ComputeScreenPos(OUT.positionHCS);
                OUT.viewDirection = GetWorldSpaceNormalizeViewDir(OUT.positionWS.xyz);
                return OUT;
            }
            
            #define MAX_SKY_OBJECT_COUNT 4
            float3 _Direction[MAX_SKY_OBJECT_COUNT];
            float3 _Color[MAX_SKY_OBJECT_COUNT];
            float _Falloff[MAX_SKY_OBJECT_COUNT];
            
            int _SkyObjectCount;
            
            float3 _HorizonColor;
            float3 _ZenithColor;
            
            float ReduceBanding(float2 screenPosition)
            {
                const float margin = 0.5/255.0;

                float v = rand2dTo1d(screenPosition);
                v = lerp(-margin, margin, v);
                return v;
            }
            
            
            float3 GetSkyColor(float3 viewDirection)
            {
                return lerp(_HorizonColor, _ZenithColor, abs(viewDirection.y));
            }

            float3 GetLighting(float3 viewDirection, float3 direction, float3 color, float falloff)
            {
                float d = distance(viewDirection, -direction);
                d = saturate(d);
                d = pow(d, falloff);
                
                d = 1.0 - d;
                d *= d;
                return color * d;
            }
       
            float4 frag(Varyings IN) : SV_Target
            {
                float3 col = GetSkyColor(IN.viewDirection);
                for(int i = 0; i < _SkyObjectCount; i++)
                {
                    col += GetLighting(IN.viewDirection, _Direction[i], _Color[i], _Falloff[i]);
                }
                
                col += ReduceBanding(IN.positionSS.xy / IN.positionSS.w);
                col = max(0.0, col);
                return float4(col,1);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ATMOSPHEREMAP"
            Tags { "LightMode" = "Altos" }
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            ZClip True

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TextureUtils.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/Math.hlsl"
            


            struct Attributes
            {
                float4 positionOS    : POSITION;
                
            };

            struct Varyings
            {
                float4 positionHCS   : SV_POSITION;
                float4 positionWS    : TEXCOORD0;
                float4 positionSS    : TEXCOORD1;
                float3 viewDirection : TEXCOORD2;
            };


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS.xyz);
                OUT.positionSS = ComputeScreenPos(OUT.positionHCS);
                OUT.viewDirection = GetWorldSpaceNormalizeViewDir(OUT.positionWS.xyz);
                return OUT;
            }
            
            #define MAX_SKY_OBJECT_COUNT 8
            float3 _Direction[MAX_SKY_OBJECT_COUNT];
            float3 _Color[MAX_SKY_OBJECT_COUNT];
            float _Falloff[MAX_SKY_OBJECT_COUNT];
            
            int _SkyObjectCount;
            
            float3 _HorizonColor;
            float3 _ZenithColor;
            
            float ReduceBanding(float2 screenPosition)
            {
                const float margin = 0.5/255.0;

                float v = rand2dTo1d(screenPosition);
                v = lerp(-margin, margin, v);
                return v;
            }
            
            
            float3 GetSkyColor(float3 viewDirection)
            {
                return lerp(_HorizonColor, _ZenithColor, abs(viewDirection.y));
            }

            float3 GetLighting(float3 viewDirection, float3 direction, float3 color, float falloff)
            {
                float d = distance(viewDirection, -direction);
                d = saturate(d);
                d = pow(d, falloff);
                
                d = 1.0 - d;
                d *= d;
                return color * d;
            }
       
            float4 frag(Varyings IN) : SV_Target
            {
                float3 col = GetSkyColor(IN.viewDirection);
                for(int i = 0; i < _SkyObjectCount; i++)
                {
                    col += GetLighting(IN.viewDirection, _Direction[i], _Color[i], _Falloff[i]);
                }
                
                col += ReduceBanding(IN.positionSS.xy / IN.positionSS.w);
                col = max(0.0, col);
                return float4(col,1);
            }
            ENDHLSL
        }
        

    }
}