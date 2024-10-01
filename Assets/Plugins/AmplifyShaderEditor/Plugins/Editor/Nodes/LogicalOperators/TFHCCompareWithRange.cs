





using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Compare With Range"
#else
"与范围比较"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Logical Operators"
#else
"逻辑运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Check if A is in the range between Range Min and Range Max. If true return value of True else return value of False"
#else
"检查A是否在范围最小值和范围最大值之间的范围内。如果返回值为真，则返回值为假"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"The Four Headed Cat - @fourheadedcat"
#else
"四头猫-@fourheaddcat"
#endif
)]
	public sealed class TFHCCompareWithRange : DynamicTypeNode
	{
		private WirePortDataType m_mainInputType = WirePortDataType.FLOAT;
		private WirePortDataType m_mainOutputType = WirePortDataType.FLOAT;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputPorts[ 0 ].Name = "Value";
			m_inputPorts[ 1 ].Name = "Range Min";
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Range Max"
#else
"最大范围"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"True"
#else
"没错"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"False"
#else
"错误的"
#endif
);
			m_textLabelWidth = 100;
			m_useInternalPortData = true;
			m_previewShaderGUID = "127d114eed178d7409f900134a6c00d1";
		}

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
			if ( portId < 3 )
			{
				if ( portId > 0 )
				{
					m_inputPorts[ portId ].ChangeType( m_mainInputType, false );
				}
			}
			else
			{
				int otherPortId = ( portId == 3 ) ? 4 : 3;
				if ( m_inputPorts[ otherPortId ].IsConnected )
				{
					m_mainOutputType = m_inputPorts[ otherPortId ].DataType;
					m_inputPorts[ portId ].ChangeType( m_mainOutputType, false );
					m_outputPorts[ 0 ].ChangeType( m_mainOutputType, false );
				}
			}
		}

		void UpdateConnections( int portId )
		{
			m_inputPorts[ portId ].MatchPortToConnection();
			int otherPortId = 0;
			WirePortDataType otherPortType = WirePortDataType.FLOAT;
			if ( portId < 3 )
			{
				if ( portId == 0 )
				{
					m_mainInputType = m_inputPorts[ 0 ].DataType;
					for ( int i = 1; i < 3; i++ )
					{
						if ( !m_inputPorts[ i ].IsConnected )
						{
							m_inputPorts[ i ].ChangeType( m_mainInputType, false );
						}
					}
				}
			}
			else
			{
				otherPortId = ( portId == 3 ) ? 4 : 3;
				otherPortType = m_inputPorts[ otherPortId ].IsConnected ? m_inputPorts[ otherPortId ].DataType : WirePortDataType.FLOAT;
				m_mainOutputType = UIUtils.GetPriority( m_inputPorts[ portId ].DataType ) > UIUtils.GetPriority( otherPortType ) ? m_inputPorts[ portId ].DataType : otherPortType;

				m_outputPorts[ 0 ].ChangeType( m_mainOutputType, false );

				if ( !m_inputPorts[ otherPortId ].IsConnected )
				{
					m_inputPorts[ otherPortId ].ChangeType( m_mainOutputType, false );
				}
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			
			
			
			
			
			
			WirePortDataType compatibleInputType = m_mainInputType;
			if ( m_mainInputType != WirePortDataType.FLOAT && m_mainInputType != WirePortDataType.INT && m_mainInputType != m_mainOutputType )
			{
				compatibleInputType = m_mainOutputType;
			}

			
			string a = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, compatibleInputType, ignoreLocalvar, true );
			string b = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, compatibleInputType, ignoreLocalvar, true );
			string c = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, compatibleInputType, ignoreLocalvar, true );
			string d = m_inputPorts[ 3 ].GenerateShaderForOutput( ref dataCollector, m_mainOutputType, ignoreLocalvar, true );
			string e = m_inputPorts[ 4 ].GenerateShaderForOutput( ref dataCollector, m_mainOutputType, ignoreLocalvar, true );
			string strout = "(( " + a + " >= " + b + " && " + a + " <= " + c + " ) ? " + d + " :  " + e + " )";
			
			return CreateOutputLocalVariable( 0, strout, ref dataCollector );
		}
	}
}
