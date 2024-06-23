Shader "Hidden/WorldSystem/Sky/SkyObjectShader"
{
    Properties
    { 
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "Sky Object"
            Tags { "LightMode" = "Altos"}

            Cull Off
            Blend OneMinusDstAlpha One
            ZTest LEqual
            ZWrite Off
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Intensity;

            struct Attributes
            {
                float2 uv            : TEXCOORD0;
                float4 positionOS    : POSITION;
            };

            struct Varyings
            {
                float2 uv            : TEXCOORD0;
                float4 positionHCS   : SV_POSITION;
                float3 viewDirection : TEXCOORD1;
            };

            Varyings vert(Attributes IN, uint instanceID: SV_InstanceID)
            {
                Varyings OUT;
                
                float4 positionWS = mul(unity_ObjectToWorld, IN.positionOS);

                OUT.positionHCS = TransformWorldToHClip(positionWS.xyz);
                OUT.viewDirection = GetWorldSpaceNormalizeViewDir(positionWS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
            
            float3 GetAtmosphericTransmittance(float altitude01)
            {
                float3 c;
                c.r = (1.0 - pow(abs(1.0 - altitude01), 10)) * 0.88 + 0.05;
                c.g = (1.0 - pow(abs(1.0 - altitude01), 7)) * 0.85;
                c.b = (1.0 - pow(abs(1.0 - altitude01), 4)) * 0.75;

                return c;
            }


            float3 GetLimbDarkeningFactor(float distance01)
            {
                const float3 rgbWavelength = float3(0.397, 0.503, 0.652);

                distance01 *= distance01;
                distance01 = 1.0 - distance01;
                distance01 = pow(distance01, 0.5);
                float3 result = pow(distance01.xxx, rgbWavelength);
                return saturate(result);
            }

            float GetRadius(float2 uv)
            {
                float2 _CENTER = float2(0.5,0.5);
                return length(uv - _CENTER) * 2;
            }

            float GetCircle(float radius)
            {
                float antiAliasing = fwidth(radius);
                float circle = smoothstep(1.0 - antiAliasing, 1.0, radius);
                circle = 1.0 - circle;
                return circle;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float radius = GetRadius(IN.uv);
                float circle = GetCircle(radius);
                float3 transmittance = GetAtmosphericTransmittance(abs(IN.viewDirection.y));
                float3 limbDarkening = GetLimbDarkeningFactor(radius);
                float3 color = limbDarkening * transmittance * circle;
                
                float4 textureValue = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 result = float4(color.rgb, circle);
                result = result * textureValue * _Color;
                return result;
            }
            ENDHLSL
        }
    }
}