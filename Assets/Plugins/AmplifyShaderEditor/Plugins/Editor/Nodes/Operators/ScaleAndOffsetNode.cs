

using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Scale And Offset"
#else
"比例和偏移"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Scales and offsets an input value\n( ( <b>Value</b> * <b>Scale</b> ) + <b>Offset</b> )"
#else
"缩放和偏移输入值\n（（<b>值</b>*<b>缩放</b>）+<b>偏移</b>"
#endif
)]
	public sealed class ScaleAndOffsetNode : ParentNode
	{
		private const string ScaleOffsetOpStr = "({0}*{1} + {2})";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, Constants.EmptyPortValue );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Scale"
#else
"规模"
#endif
);
			m_inputPorts[ 1 ].FloatInternalData = 1;
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Offset"
#else
"抵消"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT, " " );
			m_useInternalPortData = true;
			m_previewShaderGUID = "a1f1053d4d9c3be439e0382038b74771";
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			UpdateConnection( portId );
		}

		public override void OnConnectedOutputNodeChanges( int inputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( inputPortId, otherNodeId, otherPortId, name, type );
			UpdateConnection( inputPortId );
		}

		public override void OnInputPortDisconnected( int portId )
		{
			base.OnInputPortDisconnected( portId );
			m_inputPorts[ portId ].ChangeType( WirePortDataType.FLOAT, false );
			if( portId == 0 )
			{
				m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
			}
		}

		void UpdateConnection( int portId )
		{
			if( portId == 0 )
			{
				m_inputPorts[ 0 ].MatchPortToConnection();
				m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
			}
			else
			{
				WirePortDataType newDataType = m_inputPorts[ portId ].ConnectionType() == WirePortDataType.FLOAT ? WirePortDataType.FLOAT : m_outputPorts[ 0 ].DataType;
				m_inputPorts[ portId ].ChangeType( newDataType, false );
			}
		}
		
		public override string GenerateShaderForOutput( int outputId,  ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

            string value = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );

			
			WirePortDataType scaleType = ( m_inputPorts[ 1 ].ConnectionType() == WirePortDataType.FLOAT ) ? WirePortDataType.FLOAT : m_outputPorts[ 0 ].DataType;
			string scale =  m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, scaleType, ignoreLocalvar , true );

			WirePortDataType offsetType = ( m_inputPorts[ 2 ].ConnectionType() == WirePortDataType.FLOAT ) ? WirePortDataType.FLOAT : m_outputPorts[ 0 ].DataType;
			string offset = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, offsetType, ignoreLocalvar, true );
			
			string result = string.Format( ScaleOffsetOpStr, value, scale, offset );
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
