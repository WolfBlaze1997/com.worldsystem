


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Exp2"
#else
"Exp2"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Base-2 exponential of scalars and vectors"
#else
"标量和向量的基2指数"
#endif
)]
	public sealed class Exp2OpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "exp2";
			m_previewShaderGUID = "ceb70ed5423a36647a504a41de7dbfe6";
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.OBJECT,
														WirePortDataType.FLOAT,
														WirePortDataType.FLOAT2,
														WirePortDataType.FLOAT3,
														WirePortDataType.FLOAT4,
														WirePortDataType.COLOR,
														WirePortDataType.INT );
		}
	}
}
