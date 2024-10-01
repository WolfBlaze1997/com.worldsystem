





using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"If [Community]"
#else
"如果[社区]"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Logical Operators"
#else
"逻辑运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Compare A with B. If A is greater than B output the value of A > B port. If A is equal to B output the value of A == B port. If A is lower than B output the value of A < B port. Equal Threshold parameter will be used to check A == B adding and subtracting this value to A."
#else
"将A与B进行比较。如果A大于B，则输出A>B端口的值。如果A等于B，则输入A==B端口的数值。如果A小于B，则显示A<B端口的价值。equal Threshold参数将用于检查A==B与A的加减关系。"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"The Four Headed Cat - @fourheadedcat"
#else
"四头猫-@fourheaddcat"
#endif
)]
	public sealed class TFHCIf : ParentNode
	{
		private WirePortDataType m_inputMainDataType = WirePortDataType.FLOAT;
		private WirePortDataType m_outputMainDataType = WirePortDataType.FLOAT;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, "A" );
			AddInputPort( WirePortDataType.FLOAT, false, "B" );
			AddInputPort( WirePortDataType.FLOAT, false, "A > B" );
			AddInputPort( WirePortDataType.FLOAT, false, "A == B" );
			AddInputPort( WirePortDataType.FLOAT, false, "A < B" );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Equal Threshold"
#else
"相等阈值"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_textLabelWidth = 110;
			m_useInternalPortData = true;
			m_previewShaderGUID = "5c7bc7e3cab81da499e4864ace0d86c5";
		}

		public override void OnConnectedOutputNodeChanges( int inputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( inputPortId, otherNodeId, otherPortId, name, type );
			UpdateConnection( inputPortId );
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			UpdateConnection( portId );
		}

		public override void OnInputPortDisconnected( int portId )
		{
			UpdateConnection( portId );
		}

		void TestMainInputDataType()
		{
			WirePortDataType newType = WirePortDataType.FLOAT;
			if( m_inputPorts[ 0 ].IsConnected && UIUtils.GetPriority( m_inputPorts[ 0 ].DataType ) > UIUtils.GetPriority( newType ) )
			{
				newType = m_inputPorts[ 0 ].DataType;
			}

			if( m_inputPorts[ 1 ].IsConnected && ( UIUtils.GetPriority( m_inputPorts[ 1 ].DataType ) > UIUtils.GetPriority( newType ) ) )
			{
				newType = m_inputPorts[ 1 ].DataType;
			}

			if( m_inputPorts[ 5 ].IsConnected && ( UIUtils.GetPriority( m_inputPorts[ 5 ].DataType ) > UIUtils.GetPriority( newType ) ) )
			{
				newType = m_inputPorts[ 5 ].DataType;
			}

			m_inputMainDataType = newType;
		}

		void TestMainOutputDataType()
		{
			WirePortDataType newType = WirePortDataType.FLOAT;
			for( int i = 2; i < 5; i++ )
			{
				if( m_inputPorts[ i ].IsConnected && ( UIUtils.GetPriority( m_inputPorts[ i ].DataType ) > UIUtils.GetPriority( newType ) ) )
				{
					newType = m_inputPorts[ i ].DataType;
				}
			}

			if( newType != m_outputMainDataType )
			{
				m_outputMainDataType = newType;
				m_outputPorts[ 0 ].ChangeType( m_outputMainDataType, false );
			}
		}

		public void UpdateConnection( int portId )
		{
			m_inputPorts[ portId ].MatchPortToConnection();
			switch( portId )
			{
				case 0:
				case 1:
				case 5:
				{
					TestMainInputDataType();
				}
				break;
				case 2:
				case 3:
				case 4:
				{
					TestMainOutputDataType();
				}
				break;
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string a = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, m_inputMainDataType, ignoreLocalvar, true );
			string b = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, m_inputMainDataType, ignoreLocalvar, true );
			string r1 = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, m_outputMainDataType, ignoreLocalvar, true );
			string r2 = m_inputPorts[ 3 ].GenerateShaderForOutput( ref dataCollector, m_outputMainDataType, ignoreLocalvar, true );
			string r3 = m_inputPorts[ 4 ].GenerateShaderForOutput( ref dataCollector, m_outputMainDataType, ignoreLocalvar, true );
			string tr = m_inputPorts[ 5 ].GenerateShaderForOutput( ref dataCollector, m_inputMainDataType, ignoreLocalvar, true );

			
			
			

			
			
			string strout = " ( " + a + " - " + tr + " > " + b + " ? " + r1 + " : " + a + " - " + tr + " <= " + b + " && " + a + " + " + tr + " >= " + b + " ? " + r2 + " : " + r3 + " ) ";

			
			return CreateOutputLocalVariable( 0, strout, ref dataCollector );
		}
	}
}
