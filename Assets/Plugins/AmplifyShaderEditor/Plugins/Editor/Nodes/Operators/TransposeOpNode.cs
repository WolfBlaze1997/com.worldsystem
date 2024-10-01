


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Transpose"
#else
"换位思考"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Operators"
#else
"矩阵运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Transpose matrix of a matrix"
#else
"矩阵的转置矩阵"
#endif
)]
	public sealed class TransposeOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "transpose";
			m_drawPreview = false;
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.FLOAT3x3,
														WirePortDataType.FLOAT4x4 );
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4x4, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4x4, false );
		}
	}
}
