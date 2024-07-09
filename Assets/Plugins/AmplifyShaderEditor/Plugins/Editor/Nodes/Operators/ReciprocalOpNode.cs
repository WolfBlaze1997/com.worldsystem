// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Reciprocal", "Math Operators", "Reciprocal of scalars and vectors", tags: "rcp recip reciprocal" )]
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
