Shader "Hidden/WorldSystem/Sky/StarShader"
{
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "DisableBatching" = "True" }
        Pass
        {
            Name "Star Shader"
            Tags { "LightMode" = "Altos" }
            Cull Off
            Blend One One
            ZWrite Off
            ZTest Always
            ZClip False

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/Utils.hlsl"



            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float flicker       : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };            
            
            struct MeshProperties {
                float4x4 mat;
                float3 color;
                float brightness;
                float id;
            };


            StructuredBuffer<MeshProperties> altos_StarBuffer;
            float _EarthTime;
            float _Brightness;
            float _FlickerFrequency;
            float _FlickerStrength;
            TEXTURE2D(_Star_MainTex);
            SAMPLER(sampler_Star_MainTex);
            float _Inclination;
            float3 _StarColor;
            
            #define STAR_DISTANCE 4e16
            
            Varyings vert(Attributes IN, uint instanceID: SV_InstanceID)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                
                float3 positionOS = mul(altos_StarBuffer[instanceID].mat, float4(IN.positionOS.xyz, 1.0)).xyz;
                float3 forward = float3(0,0,1);
                float3 right = float3(1,0,0);
                float3 axis = RotateAroundAxis(forward, right, _Inclination); 
                positionOS = RotateAroundAxis(positionOS, axis, _EarthTime * 15.0);
                
                
                float starFlickerFrequency = _FlickerFrequency * (altos_StarBuffer[instanceID].id + 1.0);
                float starFlickerStrength = _FlickerStrength;

                float3 positionWS = TransformObjectToWorld(positionOS);
                float horizon = abs(normalize(positionWS).y);
                horizon = lerp(2.0, 1.0, horizon);


                OUT.flicker = ((sin(altos_StarBuffer[instanceID].id * 6.28 + _Time.y * starFlickerFrequency) * starFlickerStrength * horizon) + 1.0);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.color = altos_StarBuffer[instanceID].color * altos_StarBuffer[instanceID].brightness * _StarColor;
                OUT.uv = IN.uv;
                return OUT;
            }

            float3 frag(Varyings IN) : SV_Target
            { 
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                float4 tex = SAMPLE_TEXTURE2D(_Star_MainTex, sampler_Star_MainTex, IN.uv);
                float3 color = IN.color * _Brightness * IN.flicker * tex.rgb * tex.a;
                return color;
            }
            ENDHLSL
        }
    }
}