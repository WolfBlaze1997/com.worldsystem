


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Rsqrt"
#else
"Rsqrt"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Reciprocal square root of scalars and vectors"
#else
"标量和向量的互易平方根"
#endif
, tags: 
#if !WB_LANGUAGE_CHINESE
"reciprocal square root"
#else
"倒数平方根"
#endif
)]
	public sealed class RSqrtOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "rsqrt";
			m_previewShaderGUID = "c58c17cb1f7f6e6429a2c7a6cdaef87d";
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
