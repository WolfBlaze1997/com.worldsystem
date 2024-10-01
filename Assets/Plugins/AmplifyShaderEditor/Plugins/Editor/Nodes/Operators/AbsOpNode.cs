


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Abs"
#else
"防抱死制动系统"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Absolute value of scalars and vectors"
#else
"标量和向量的绝对值"
#endif
)]
	public sealed class AbsOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "abs";
			m_previewShaderGUID = "cd6d6dfa3df214a479f68a490e177db6";
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.OBJECT,
														WirePortDataType.FLOAT, 
														WirePortDataType.FLOAT2,
														WirePortDataType.FLOAT3,
														WirePortDataType.FLOAT4,
														WirePortDataType.COLOR,
														WirePortDataType.INT);
		}
	}
}
