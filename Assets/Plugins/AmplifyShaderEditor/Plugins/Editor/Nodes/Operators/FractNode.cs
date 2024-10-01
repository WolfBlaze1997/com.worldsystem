


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Fract"
#else
"骨折"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Fractional portion of a scalar or each vector component"
#else
"标量或每个向量分量的分数部分"
#endif
)]
	public sealed class FractNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "frac";
			m_previewShaderGUID = "53a335f8f18d4694b8d94e8aee21fdca";
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
