


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Distance"
#else
"距离"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vector Operators"
#else
"矢量运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Euclidean distance between two points"
#else
"两点之间的欧几里德距离"
#endif
)]
	public sealed class DistanceOpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_inputPorts[ 1 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
			m_dynamicOutputType = false;
			m_useInternalPortData = true;
			m_previewShaderGUID = "3be9a95031c0cb740ae982e465dfc242";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			string result = "distance( " + m_inputA + " , " + m_inputB + " )";
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
