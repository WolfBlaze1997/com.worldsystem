


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "ATan",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Arctangent of scalars and vectors"
#else
"标量和向量的反正切"
#endif
, tags: 
#if !WB_LANGUAGE_CHINESE
"Arctangent"
#else
"反正切函数"
#endif
)]
	public sealed class ATanOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "atan";
			m_previewShaderGUID = "7d7f3331a98831241b017364e80625ea";
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
