


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Determinant"
#else
"行列式"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Operators"
#else
"矩阵运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Scalar determinant of a square matrix"
#else
"方阵的标量行列式"
#endif
)]
	public sealed class DeterminantOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "determinant";
			m_drawPreview = false;
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.FLOAT3x3,
														WirePortDataType.FLOAT4x4 );

			m_autoUpdateOutputPort = false;
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4x4, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
		}
	}
}
