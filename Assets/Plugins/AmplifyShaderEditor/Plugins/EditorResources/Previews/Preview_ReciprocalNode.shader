Shader "Hidden/ReciprocalNode"
{
	Properties
	{
		_A ("_A", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag

			sampler2D _A;

			float4 frag(v2f_img i) : SV_Target
			{
			#if ( SHADER_TARGET >= 50 )
				return rcp( tex2D( _A, i.uv ) );
			#else
				return 1.0 / tex2D( _A, i.uv );
			#endif
			}
			ENDCG
		}
	}
}
