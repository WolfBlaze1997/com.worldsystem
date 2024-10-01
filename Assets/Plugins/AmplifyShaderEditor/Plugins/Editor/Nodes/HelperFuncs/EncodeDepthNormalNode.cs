


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Encode Depth Normal"
#else
"编码深度正常"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Encodes both Depth and Normal values into a Float4 value"
#else
"将“深度”和“法线”值编码为Float4值"
#endif
)]
	public sealed class EncodeDepthNormalNode : ParentNode
	{
		private const string EncodeDepthNormalFunc = "EncodeDepthNormal( {0}, {1} )";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Depth"
#else
"深度"
#endif
);
			AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"Normal"
#else
"正常"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT4, Constants.EmptyPortValue );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			dataCollector.AddToIncludes( UniqueId, Constants.UnityCgLibFuncs );
			string depthValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string normalValue = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			RegisterLocalVariable( 0, string.Format( EncodeDepthNormalFunc, depthValue, normalValue ), ref dataCollector, "encodedDepthNormal" + OutputId );
			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
