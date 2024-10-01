


using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Divide"
#else
"分割"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Division of two values ( A / B )"
#else
"两个值的划分（A/B）"
#endif
, null, KeyCode.D )]
	public sealed class SimpleDivideOpNode : DynamicTypeNode
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
			m_previewShaderGUID = "409f06d00d1094849b0834c52791fa72";
		}

		public override string BuildResults( int outputId,  ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			SetExtensibleInputData( outputId, ref dataCollector, ignoreLocalvar );	
			string result = "( " + m_extensibleInputResults[ 0 ];
			for ( int i = 1; i < m_extensibleInputResults.Count; i++ )
			{
				result += " / " + m_extensibleInputResults[ i ];
			}
			result += " )";
			return result;
		}
	}
}
