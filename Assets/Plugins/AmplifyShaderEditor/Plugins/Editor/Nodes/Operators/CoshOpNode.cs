


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Cosh"
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
"Hyperbolic cosine of scalars and vectors"
#else
"标量和向量的双曲余弦"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"hyperbolic cosine"
#else
"双曲余弦"
#endif
)]
	public sealed class CoshOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "cosh";
			m_previewShaderGUID = "154a4c85fe88657489a54a02416402c0";
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
