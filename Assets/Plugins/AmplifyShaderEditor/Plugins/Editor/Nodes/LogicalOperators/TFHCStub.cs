



namespace AmplifyShaderEditor
{
	[System.Serializable]
	public class TFHCStub : DynamicTypeNode
	{
		protected WirePortDataType m_mainInputType = WirePortDataType.FLOAT;
		protected WirePortDataType m_mainOutputType = WirePortDataType.FLOAT;
		protected string m_inputDataPort0 = string.Empty;
		protected string m_inputDataPort1 = string.Empty;
		protected string m_inputDataPort2 = string.Empty;
		protected string m_inputDataPort3 = string.Empty;

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			UpdateConnections( portId );
		}

		public override void OnConnectedOutputNodeChanges( int outputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			UpdateConnections( outputPortId );
		}

		public override void OnInputPortDisconnected( int portId )
		{
			int otherPortId = 0;
			if ( portId < 2 )
			{
				otherPortId = ( portId == 0 ) ? 1 : 0;
				if ( m_inputPorts[ otherPortId ].IsConnected )
				{
					m_mainInputType = m_inputPorts[ otherPortId ].DataType;
					m_inputPorts[ portId ].ChangeType( m_mainInputType, false );
				}
			}
			else
			{
				otherPortId = ( portId == 2 ) ? 3 : 2;
				if ( m_inputPorts[ otherPortId ].IsConnected )
				{
					m_mainOutputType = m_inputPorts[ otherPortId ].DataType;
					m_inputPorts[ portId ].ChangeType( m_mainOutputType, false );
					m_outputPorts[ 0 ].ChangeType( m_mainOutputType, false );
				}
			}
		}
		
		public void GetInputData( ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			
			
			
			
			
			
			WirePortDataType compatibleInputType = m_mainInputType;
			if ( m_mainInputType != WirePortDataType.FLOAT && m_mainInputType != WirePortDataType.INT && m_mainInputType != m_mainOutputType )
			{
				compatibleInputType = m_mainOutputType;
			}

			m_inputDataPort0 = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, compatibleInputType, ignoreLocalvar, true );
			m_inputDataPort1 = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, compatibleInputType, ignoreLocalvar, true );


			m_inputDataPort2 = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, m_mainOutputType, ignoreLocalvar, true );
			m_inputDataPort3 = m_inputPorts[ 3 ].GenerateShaderForOutput( ref dataCollector, m_mainOutputType, ignoreLocalvar, true );
		}

		void UpdateConnections( int portId )
		{
			m_inputPorts[ portId ].MatchPortToConnection();
			int otherPortId = 0;
			WirePortDataType otherPortType = WirePortDataType.FLOAT;
			if ( portId < 2 )
			{
				otherPortId = ( portId == 0 ) ? 1 : 0;
				otherPortType = m_inputPorts[ otherPortId ].IsConnected ? m_inputPorts[ otherPortId ].DataType : WirePortDataType.FLOAT;
				m_mainInputType = UIUtils.GetPriority( m_inputPorts[ portId ].DataType ) > UIUtils.GetPriority( otherPortType ) ? m_inputPorts[ portId ].DataType : otherPortType;
				if ( !m_inputPorts[ otherPortId ].IsConnected )
				{
					m_inputPorts[ otherPortId ].ChangeType( m_mainInputType, false );
				}
			}
			else
			{
				otherPortId = ( portId == 2 ) ? 3 : 2;
				otherPortType = m_inputPorts[ otherPortId ].IsConnected ? m_inputPorts[ otherPortId ].DataType : WirePortDataType.FLOAT;
				m_mainOutputType = UIUtils.GetPriority( m_inputPorts[ portId ].DataType ) > UIUtils.GetPriority( otherPortType ) ? m_inputPorts[ portId ].DataType : otherPortType;

				m_outputPorts[ 0 ].ChangeType( m_mainOutputType, false );

				if ( !m_inputPorts[ otherPortId ].IsConnected )
				{
					m_inputPorts[ otherPortId ].ChangeType( m_mainOutputType, false );
				}
			}
		}
	}
}
