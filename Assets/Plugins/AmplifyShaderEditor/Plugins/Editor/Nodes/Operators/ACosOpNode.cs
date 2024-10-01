


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"ACos"
#else
"ACos"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Arccosine of scalars and vectors"
#else
"标量和向量的反正弦"
#endif
, tags: 
#if !WB_LANGUAGE_CHINESE
"arccosine"
#else
"反余弦"
#endif
)]
	public sealed class ACosOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "acos";
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.OBJECT, 
														WirePortDataType.FLOAT , 
														WirePortDataType.FLOAT2, 
														WirePortDataType.FLOAT3, 
														WirePortDataType.FLOAT4, 
														WirePortDataType.COLOR, 
														WirePortDataType.INT );
			m_previewShaderGUID = "710f3c0bbd7ba0c4aada6d7dfadd49c2";
		}
	}
}
