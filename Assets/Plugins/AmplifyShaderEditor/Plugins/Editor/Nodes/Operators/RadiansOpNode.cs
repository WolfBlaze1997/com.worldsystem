


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Radians"
#else
"辐射"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Converts values of scalars and vectors from degrees to radians"
#else
"将标量和向量的值从度数转换为弧度"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"degrees"
#else
"度"
#endif
)]
	public sealed class RadiansOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "radians";
			m_previewShaderGUID = "d27d189eaf6eeb04fae9913d9617ece5";
			m_inputPorts[ 0 ].CreatePortRestrictions( WirePortDataType.OBJECT,
														WirePortDataType.FLOAT,
														WirePortDataType.FLOAT2,
														WirePortDataType.FLOAT3,
														WirePortDataType.FLOAT4,
														WirePortDataType.COLOR,
														WirePortDataType.INT );
		}
	}
}
