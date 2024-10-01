


using UnityEngine;
using System;



namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "If",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Logical Operators"
#else
"逻辑运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Conditional comparison between A with B."
#else
"A与B之间的条件比较。"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"branch"
#else
"分支"
#endif
)]
	public sealed class ConditionalIfNode : ParentNode
	{
		private const string UseUnityBranchesStr = 
#if !WB_LANGUAGE_CHINESE
"Dynamic Branching"
#else
"动态分支"
#endif
;
		private const string UnityBranchStr = "UNITY_BRANCH ";

		private readonly string[] IfOps = { "if( {0} > {1} )",
											"if( {0} == {1} )",
											"if( {0} < {1} )",
											"if( {0} >= {1} )",
											"if( {0} <= {1} )",
											"if( {0} != {1} )" };

		
		private WirePortDataType m_outputMainDataType = WirePortDataType.FLOAT;
		private string[] m_results = { string.Empty, string.Empty, string.Empty };

		[SerializeField]
		private bool m_useUnityBranch = false;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, "A" );
			AddInputPort( WirePortDataType.FLOAT, false, "B" );
			m_inputPorts[ 0 ].AddPortRestrictions( WirePortDataType.FLOAT, WirePortDataType.INT );
			m_inputPorts[ 1 ].AddPortRestrictions( WirePortDataType.FLOAT, WirePortDataType.INT );

			AddInputPort( WirePortDataType.FLOAT, false, "A > B" );
			AddInputPort( WirePortDataType.FLOAT, false, "A == B" );
			AddInputPort( WirePortDataType.FLOAT, false, "A < B" );
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_inputPorts[ 0 ].AutoDrawInternalData = true;
			m_inputPorts[ 1 ].AutoDrawInternalData = true;
			m_textLabelWidth = 131;
			
			m_autoWrapProperties = true;
			m_previewShaderGUID = "f6fb4d46bddf29e45a8a3ddfed75d0c0";
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

		public override void DrawProperties()
		{
			base.DrawProperties();
			if( !m_inputPorts[ 0 ].IsConnected )
				m_inputPorts[ 0 ].FloatInternalData = EditorGUILayoutFloatField( m_inputPorts[ 0 ].Name, m_inputPorts[ 0 ].FloatInternalData );
			if( !m_inputPorts[ 1 ].IsConnected )
				m_inputPorts[ 1 ].FloatInternalData = EditorGUILayoutFloatField( m_inputPorts[ 1 ].Name, m_inputPorts[ 1 ].FloatInternalData );
			m_useUnityBranch = EditorGUILayoutToggle( UseUnityBranchesStr, m_useUnityBranch );
		}

		public override void OnInputPortDisconnected( int portId )
		{
			base.OnInputPortDisconnected( portId );
			UpdateConnection( portId );
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
			}
			m_outputPorts[ 0 ].ChangeType( m_outputMainDataType, false );
		}

		public void UpdateConnection( int portId )
		{
			m_inputPorts[ portId ].MatchPortToConnection();
			switch( portId )
			{
				
				
				
				
				
				
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
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string AValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector);
			string BValue = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			m_results[ 0 ] = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, m_outputMainDataType, ignoreLocalvar, true );
			m_results[ 1 ] = m_inputPorts[ 3 ].GenerateShaderForOutput( ref dataCollector, m_outputMainDataType, ignoreLocalvar, true );
			m_results[ 2 ] = m_inputPorts[ 4 ].GenerateShaderForOutput( ref dataCollector, m_outputMainDataType, ignoreLocalvar, true );

			string localVarName = "ifLocalVar" + OutputId;
			string localVarDec = string.Format( "{0} {1} = 0;", UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, m_outputPorts[ 0 ].DataType ), localVarName );

			bool lequal = false;
			bool greater = false;
			bool lesser = false;
			bool gequal = false;
			bool equal = false;
			bool nequal = false;
			bool welse = false;
			bool midCon = false;

			if( m_inputPorts[ 2 ].IsConnected )
			{
				greater = true;
			}

			if( m_inputPorts[ 4 ].IsConnected )
			{
				lesser = true;
			}

			if( greater && m_inputPorts[ 2 ].GetOutputConnection() == m_inputPorts[ 3 ].GetOutputConnection() )
			{
				gequal = true;
			}

			if( lesser && m_inputPorts[ 4 ].GetOutputConnection() == m_inputPorts[ 3 ].GetOutputConnection() )
			{
				lequal = true;
			}

			if( m_inputPorts[ 2 ].GetOutputConnection() == m_inputPorts[ 4 ].GetOutputConnection() )
			{
				if( m_inputPorts[ 3 ].IsConnected )
					equal = true;
				else if( m_inputPorts[ 2 ].IsConnected )
					nequal = true;
			}

			if( m_inputPorts[ 3 ].IsConnected )
			{
				midCon = true;

				if( greater && lesser )
					welse = true;
			}

			dataCollector.AddLocalVariable( UniqueId, localVarDec, true );
			if ( m_useUnityBranch && !( lequal && gequal ) && !( !greater && !midCon && !lesser ) )
				dataCollector.AddLocalVariable( UniqueId, UnityBranchStr, true );

			if( lequal && gequal ) 
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( "{0} = {1};", localVarName, m_results[ 1 ] ), true );
			}
			else if( !lequal && gequal ) 
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 3 ], AValue, BValue ), true );
				dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 0 ] ), true );

				if( welse )
				{
					dataCollector.AddLocalVariable( UniqueId, "else", true );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 2 ] ), true );
				}
			}
			else if( lequal && !gequal )
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 4 ], AValue, BValue ), true );
				dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 2 ] ), true );

				if( welse )
				{
					dataCollector.AddLocalVariable( UniqueId, "else", true );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 0 ] ), true );
				}
			}
			else if( nequal )
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 5 ], AValue, BValue ), true );
				dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 0 ] ), true );
			}
			else if( equal )
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 1 ], AValue, BValue ), true );
				dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 1 ] ), true );

				if( welse )
				{
					dataCollector.AddLocalVariable( UniqueId, "else", true );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 0 ] ), true );
				}
			}
			else if( lesser && !midCon && !greater ) 
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 2 ], AValue, BValue ), true );
				dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 2 ] ), true );
			}
			else if( greater && !midCon && !lesser ) 
			{
				dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 0 ], AValue, BValue ), true );
				dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 0 ] ), true );
			}
			else if( !greater && !midCon && !lesser ) 
			{
				
			}
			else 
			{
				bool ifStarted = false;
				if( greater )
				{
					dataCollector.AddLocalVariable( UniqueId, string.Format( IfOps[ 0 ], AValue, BValue ), true );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 0 ] ), true );
					ifStarted = true;
				}

				if( midCon )
				{
					dataCollector.AddLocalVariable( UniqueId, ( ifStarted ? "else " : string.Empty ) +string.Format( IfOps[ 1 ], AValue, BValue ), true );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 1 ] ), true );
					ifStarted = true;
				}

				if( lesser )
				{
					dataCollector.AddLocalVariable( UniqueId, "else " + string.Format( IfOps[ 2 ], AValue, BValue ), true );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "\t{0} = {1};", localVarName, m_results[ 2 ] ), true );
				}
			}

			m_outputPorts[ 0 ].SetLocalValue( localVarName, dataCollector.PortCategory );
			return localVarName;
		}
		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 4103 )
			{
				m_useUnityBranch = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_useUnityBranch );
		}
	}
}
