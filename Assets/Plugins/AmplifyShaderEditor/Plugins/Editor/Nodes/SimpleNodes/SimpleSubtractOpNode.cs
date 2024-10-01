


using System;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Subtract"
#else
"减法"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Subtraction of two values ( A - B )"
#else
"减去两个值（A-B）"
#endif
, null, UnityEngine.KeyCode.S )]
	public sealed class SimpleSubtractOpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			m_dynamicRestrictions = new WirePortDataType[]
			{
				WirePortDataType.OBJECT,
				WirePortDataType.FLOAT,
				WirePortDataType.FLOAT2,
				WirePortDataType.FLOAT3,
				WirePortDataType.FLOAT4,
				WirePortDataType.COLOR,
				WirePortDataType.FLOAT3x3,
				WirePortDataType.FLOAT4x4,
				WirePortDataType.INT
			};

			base.CommonInit( uniqueId );
			m_allowMatrixCheck = true;
			m_previewShaderGUID = "5725e8300be208449973f771ab6682f2";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			return "( " + m_inputA + " - " + m_inputB + " )";
		}
	}
}
