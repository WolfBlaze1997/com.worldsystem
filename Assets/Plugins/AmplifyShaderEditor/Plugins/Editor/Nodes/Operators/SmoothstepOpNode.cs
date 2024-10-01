


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Smoothstep"
#else
"平稳的步伐"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Returns a smooth Hermite interpolation between 0 and 1, if input is in the range [min, max]."
#else
"如果输入在[min，max]范围内，则返回0到1之间的平滑Hermite插值。"
#endif
)]
	public sealed class SmoothstepOpNode : ParentNode
	{
		
		
		
		private int m_alphaPortId = 0;
		private int m_minPortId = 0;
		private int m_maxPortId = 0;
		private const string SmoothstepOpFormat = "smoothstep( {0} , {1} , {2})";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, Constants.EmptyPortValue, -1, MasterNodePortCategory.Fragment, 0 );
			m_alphaPortId = m_inputPorts.Count - 1;
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Min"
#else
"分钟"
#endif
, -1, MasterNodePortCategory.Fragment, 1 );
			m_minPortId = m_inputPorts.Count - 1;
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Max"
#else
"马克斯"
#endif
, -1, MasterNodePortCategory.Fragment, 2 );
			m_maxPortId = m_inputPorts.Count - 1;

			GetInputPortByUniqueId( m_maxPortId ).FloatInternalData = 1;

			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_useInternalPortData = true;
			m_textLabelWidth = 55;
			m_previewShaderGUID = "954cdd40a7a528344a0a4d3ff1db5176";
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			if( portId == 0 )
			{
				m_inputPorts[ 0 ].MatchPortToConnection();
				m_inputPorts[ 1 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
				m_inputPorts[ 2 ].ChangeType( m_inputPorts[ 0 ].DataType, false );

				m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
			}
		}

		public override void OnConnectedOutputNodeChanges( int outputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( outputPortId, otherNodeId, otherPortId, name, type );
			if( outputPortId == 0 )
			{
				m_inputPorts[ 0 ].MatchPortToConnection();
				m_inputPorts[ 1 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
				m_inputPorts[ 2 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
				m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
			}
		}

		
		
		
		
		

		
		
		
		
		

		
		
		
		
		

		
		
		
		
		

		
		
		

		

		
		

		

		
		
		
		
		

		
		

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string aValue = m_inputPorts[ m_minPortId ].GeneratePortInstructions( ref dataCollector );
			string bValue = m_inputPorts[ m_maxPortId ].GeneratePortInstructions( ref dataCollector );
			string interp = m_inputPorts[ m_alphaPortId ].GeneratePortInstructions( ref dataCollector );
			
			string result = string.Format( SmoothstepOpFormat, aValue, bValue, interp );

			RegisterLocalVariable( 0, result, ref dataCollector, "smoothstepResult" + OutputId );

			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
