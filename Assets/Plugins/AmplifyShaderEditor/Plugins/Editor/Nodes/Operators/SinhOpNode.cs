


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Sinh"
#else
"Sinh"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Hyperbolic sine of scalars and vectors"
#else
"标量和向量的双曲正弦"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"hyperbolic sine"
#else
"双曲正弦曲线"
#endif
)]
	public sealed class SinhOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "sinh";
			m_previewShaderGUID = "4e9c00e6dceb4024f80d4e3d7786abad";
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
