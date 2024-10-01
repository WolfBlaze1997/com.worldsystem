


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Cos"
#else
"科斯"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Cosine of scalars and vectors"
#else
"标量和向量的余弦"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"cosine"
#else
"余弦"
#endif
)]
	public sealed class CosOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "cos";
			m_previewShaderGUID = "3dde9e80389196f459eb94137268de4a";
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
