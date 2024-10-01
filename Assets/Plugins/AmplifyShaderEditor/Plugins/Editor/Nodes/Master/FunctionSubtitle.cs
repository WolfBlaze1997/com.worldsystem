


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Function Subtitle"
#else
"功能字幕"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Functions"
#else
"功能"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Adds a subtitle to its shader function"
#else
"将字幕添加到其着色器功能中"
#endif
, NodeAvailabilityFlags = (int)NodeAvailability.ShaderFunction )]
	public sealed class FunctionSubtitle : ParentNode
	{

		
		
		
		
		
		
		
		
		
		
		
		[SerializeField]
		private string m_subtitle = 
#if !WB_LANGUAGE_CHINESE
"Subtitle"
#else
"字幕"
#endif
;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, Constants.EmptyPortValue );
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_autoWrapProperties = true;
			m_textLabelWidth = 100;
			SetTitleText( m_subtitle );
			m_previewShaderGUID = "74e4d859fbdb2c0468de3612145f4929";
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			m_inputPorts[ 0 ].MatchPortToConnection();
			m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
		}

		public override void OnConnectedOutputNodeChanges( int outputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( outputPortId, otherNodeId, otherPortId, name, type );
			m_inputPorts[ 0 ].MatchPortToConnection();
			m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			return m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, m_inputPorts[ 0 ].DataType, ignoreLocalvar );
		}

		
		
		

		
		
		

		public override void OnNodeLogicUpdate( DrawInfo drawInfo )
		{
			base.OnNodeLogicUpdate( drawInfo );
			
			
			
			
			if( m_containerGraph.CurrentFunctionOutput != null && IsConnected )
				m_containerGraph.CurrentFunctionOutput.SubTitle = m_subtitle;
			
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUILayout.BeginVertical();
			EditorGUI.BeginChangeCheck();
			m_subtitle = EditorGUILayoutTextField( 
#if !WB_LANGUAGE_CHINESE
"Name"
#else
"姓名"
#endif
, m_subtitle );
			if( EditorGUI.EndChangeCheck() )
			{
				SetTitleText( m_subtitle );
				
			}
			EditorGUI.BeginChangeCheck();
			
			
			
			
			
			

			

			
			
			
			
			


			EditorGUILayout.EndVertical();
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_subtitle );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_subtitle = GetCurrentParam( ref nodeParams );
			SetTitleText( m_subtitle );
		}
	}
}
