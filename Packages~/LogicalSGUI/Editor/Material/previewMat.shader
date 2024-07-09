Shader "Unlit/previewMat"
{
    Properties
    {
        [MainTexture]_MainTex ("Texture", 2D) = "white" { }
        // _ColorMask ("_ColorMask", Vector) = (1.0, 1.0, 1.0, 1.0)
        // _Mip ("_Mip", Integer) = 0

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorMask;
            int _Mip;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2Dlod(_MainTex, half4(i.uv, 0.0, _Mip));

                half final = dot(col, _ColorMask);
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                if (_ColorMask.x + _ColorMask.y + _ColorMask.z + _ColorMask.w == 0)
                {
                    return half4((half3(col.xy, 1.0)), 1.0);
                }
                if (_ColorMask.x + _ColorMask.y + _ColorMask.z + _ColorMask.w == 4)
                {
                    return half4(LinearToGammaSpace(half3(col.xyz)), 1.0);
                }

                return final.xxxx;
            }
            ENDCG
        }
    }
}
