


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Reciprocal"
#else
"互惠互利"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Reciprocal of scalars and vectors"
#else
"标量和向量的倒数"
#endif
, tags: 
#if !WB_LANGUAGE_CHINESE
"rcp recip reciprocal"
#else
"rcp互惠"
#endif
)]
	public sealed class ReciprocalOpNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, Constants.EmptyPortValue );
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_useInternalPortData = true;
			m_previewShaderGUID = "51c79938d491c8244a633fe407c49327";
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

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
			
			var inputValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			var localVarType = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, m_outputPorts[ 0 ].DataType );
			var localVarName = "recip" + OutputId;

			dataCollector.AddLocalVariable( UniqueId, "#if ( SHADER_TARGET >= 50 )" );
			dataCollector.AddLocalVariable( UniqueId, string.Format( "{0} {1} = rcp( {2} );", localVarType, localVarName, inputValue ) );
			dataCollector.AddLocalVariable( UniqueId, "#else" );
			dataCollector.AddLocalVariable( UniqueId, string.Format( "{0} {1} = 1.0 / {2};", localVarType, localVarName, inputValue ) );
			dataCollector.AddLocalVariable( UniqueId, "#endif" );

			m_outputPorts[ 0 ].SetLocalValue( localVarName, dataCollector.PortCategory );
			return localVarName;
		}
	}
}
