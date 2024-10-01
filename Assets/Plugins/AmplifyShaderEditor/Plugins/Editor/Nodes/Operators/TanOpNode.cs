


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Tan",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Tangent of scalars and vectors"
#else
"标量和向量的切线"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"tangent"
#else
"切线"
#endif
)]
	public sealed class TanOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "tan";
			m_previewShaderGUID = "312e291832cac5749a3626547dfc8607";
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
