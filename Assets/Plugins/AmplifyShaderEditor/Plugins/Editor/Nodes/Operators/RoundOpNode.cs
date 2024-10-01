


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Round"
#else
"圆形"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Rounded value of scalars or vectors"
#else
"标量或向量的四舍五入值"
#endif
)]
	public sealed class RoundOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "round";
			m_previewShaderGUID = "554d561417b207c4bb3cd4a0c86b6907";
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
