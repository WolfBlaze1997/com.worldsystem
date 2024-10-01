


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Add"
#else
"添加"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Addition of two or more values ( A + B + .. )"
#else
"添加两个或多个值（A+B+..）"
#endif
, null, KeyCode.A )]
	public sealed class SimpleAddOpNode : DynamicTypeNode
	{
		private int m_cachedPropertyId = -1;

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
			m_extensibleInputPorts = true;
			m_previewShaderGUID = "9eb150cbc752cbc458a0a37984b9934a";
		}

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();

			if ( m_cachedPropertyId == -1 )
				m_cachedPropertyId = Shader.PropertyToID( "_Count" );

			PreviewMaterial.SetInt( m_cachedPropertyId, m_inputPorts.Count);
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			string result = "( " + m_extensibleInputResults[ 0 ];
			for ( int i = 1; i < m_extensibleInputResults.Count; i++ )
			{
				result += " + " + m_extensibleInputResults[ i ];
			}
			result += " )";
			return result;
		}
	}
}
