


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Exp"
#else
"费用"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Base-e exponential of scalars and vectors"
#else
"标量和向量的Base-e指数"
#endif
)]
	public sealed class ExpOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "exp";
			m_previewShaderGUID = "6416ff506137d97479a7ebde790b45e5";
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.OBJECT,
														WirePortDataType.FLOAT ,
														WirePortDataType.FLOAT2,
														WirePortDataType.FLOAT3,
														WirePortDataType.FLOAT4,
														WirePortDataType.COLOR ,
														WirePortDataType.INT);
		}
	}
}
