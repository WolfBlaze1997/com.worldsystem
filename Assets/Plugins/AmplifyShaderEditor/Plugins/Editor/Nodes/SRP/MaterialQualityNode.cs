


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Material Quality"
#else
"材料质量"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Logical Operators"
#else
"逻辑运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Choose between separate branches according to currently selected Quality (SRP only) "
#else
"根据当前选定的质量在单独的分支之间进行选择（仅SRP）"
#endif
, Available = true )]
	public class MaterialQualityNode : ParentNode
	{
		private const string SRPError = "Node intended to be used only on SRP templates as it makes use of keywords defined over that environment.";

		private const string MaxKeyword = "MATERIAL_QUALITY_HIGH";
		private const string MedKeyword = "MATERIAL_QUALITY_MEDIUM";
		private const string MinKeyword = "MATERIAL_QUALITY_LOW";
		private const string MaterialPragmas = "#pragma shader_feature " + MaxKeyword + " " + MedKeyword + " " + MinKeyword;
		private readonly string[] MaterialCode =
		{
			"#if defined("+MaxKeyword+")",
			"#elif defined("+MedKeyword+")",
			"#else",
			"#endif"
		};
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"High"
#else
"高"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Medium"
#else
"中等"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Low"
#else
"低"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_errorMessageTypeIsError = NodeMessageType.Error;
			m_errorMessageTooltip = SRPError;
		}

		public override void OnNodeLogicUpdate( DrawInfo drawInfo )
		{
			base.OnNodeLogicUpdate( drawInfo );
			if( !ContainerGraph.IsSRP )
			{
				if( !m_showErrorMessage )
				{
					m_showErrorMessage = true;
				}
			}
			else
			{
				if( m_showErrorMessage )
				{
					m_showErrorMessage = false;
				}
			}
		}
		public override void OnInputPortConnected( int portId , int otherNodeId , int otherPortId , bool activateNode = true )
		{
			base.OnInputPortConnected( portId , otherNodeId , otherPortId , activateNode );
			UpdateConnections();
		}

		public override void OnConnectedOutputNodeChanges( int inputPortId , int otherNodeId , int otherPortId , string name , WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( inputPortId , otherNodeId , otherPortId , name , type );
			UpdateConnections();
		}

		public override void OnInputPortDisconnected( int portId )
		{
			base.OnInputPortDisconnected( portId );
			UpdateConnections();
		}

		private void UpdateConnections()
		{
			WirePortDataType mainType = WirePortDataType.FLOAT;

			int highest = UIUtils.GetPriority( mainType );
			for( int i = 0 ; i < m_inputPorts.Count ; i++ )
			{
				if( m_inputPorts[ i ].IsConnected )
				{
					WirePortDataType portType = m_inputPorts[ i ].GetOutputConnection().DataType;
					if( UIUtils.GetPriority( portType ) > highest )
					{
						mainType = portType;
						highest = UIUtils.GetPriority( portType );
					}
				}
			}

			for( int i = 0 ; i < m_inputPorts.Count ; i++ )
			{
				m_inputPorts[ i ].ChangeType( mainType , false );
			}

			m_outputPorts[ 0 ].ChangeType( mainType , false );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			dataCollector.AddToDirectives( MaterialPragmas );
			string maxQualityValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string medQualityValue = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
			string minQualityValue = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );
			string localVarName = "currQuality" + OutputId;
			dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, m_outputPorts[ 0 ].DataType, localVarName, "0" );

			
			dataCollector.AddLocalVariable( UniqueId, MaterialCode[ 0 ], true );
			dataCollector.AddLocalVariable( UniqueId, localVarName, maxQualityValue, false, true );
			
			
			dataCollector.AddLocalVariable( UniqueId, MaterialCode[ 1 ], true );
			dataCollector.AddLocalVariable( UniqueId, localVarName, medQualityValue, false, true );

			
			dataCollector.AddLocalVariable( UniqueId, MaterialCode[ 2 ], true );
			dataCollector.AddLocalVariable( UniqueId, localVarName, minQualityValue,false,true );
			m_outputPorts[ 0 ].SetLocalValue( localVarName, dataCollector.PortCategory );

			dataCollector.AddLocalVariable( UniqueId, MaterialCode[ 3 ], true );
			return localVarName;
		}
	}
}
