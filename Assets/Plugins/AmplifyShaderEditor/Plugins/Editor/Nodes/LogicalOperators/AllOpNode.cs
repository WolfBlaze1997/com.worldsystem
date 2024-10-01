


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"All"
#else
"全部"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Logical Operators"
#else
"逻辑运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Determines if all components of the specified value are non-zero."
#else
"确定指定值的所有分量是否为非零。"
#endif
)]
	public sealed class AllOpNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, Constants.EmptyPortValue );
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_useInternalPortData = true;
			m_previewShaderGUID = "c33c751fb9fd7ad4e9fdc18d1f3a28d3";
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			m_inputPorts[ 0 ].MatchPortToConnection();
			m_outputPorts[ 0 ].ChangeType( InputPorts[ 0 ].DataType, false );
		}

		public override void OnConnectedOutputNodeChanges( int outputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( outputPortId, otherNodeId, otherPortId, name, type );
			m_inputPorts[ 0 ].MatchPortToConnection();
			m_outputPorts[ 0 ].ChangeType( InputPorts[ 0 ].DataType, false );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			var inputValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			var outputType = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, m_outputPorts[ 0 ].DataType );
			
			string result = string.Format( "( ( {0} )( all( {1} ) ? 1 : 0 ) )", outputType, inputValue );
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
