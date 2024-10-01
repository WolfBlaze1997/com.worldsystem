


using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Log"
#else
"日志"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Master"
#else
"大师"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Debug node to dump output to log"
#else
"调试节点将输出转储到日志"
#endif
, null, KeyCode.None, false )]
	public sealed class LogNode : MasterNode
	{
		private const string InputAmountStr = 
#if !WB_LANGUAGE_CHINESE
"Input amount"
#else
"输入金额"
#endif
;

		[SerializeField]
		private int m_inputCount = 1;

		[SerializeField]
		private int m_lastInputCount = 1;

		public LogNode() : base() { }
		public LogNode( int uniqueId, float x, float y, float width, float height ) : base( uniqueId, x, y, width, height ) { }
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddMasterPorts();
		}

		public override void AddMasterPorts()
		{
			DeleteAllInputConnections( true );
			base.AddMasterPorts();

			for ( int i = 0; i < m_inputCount; i++ )
			{
				AddInputPort( WirePortDataType.OBJECT, false, i.ToString() );
			}
			m_sizeIsDirty = true;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.LabelField( InputAmountStr );
				m_inputCount = EditorGUILayoutIntField( m_inputCount );
			}
			EditorGUILayout.EndVertical();
			if ( m_inputCount != m_lastInputCount )
			{
				m_lastInputCount = Mathf.Max( m_inputCount, 1 );
				AddMasterPorts();
			}
		}

		public override void Execute( Shader currentSelected )
		{
			string valueDump = "";
			string valueInstructions = "";

			MasterNodeDataCollector dataCollector = new MasterNodeDataCollector( this );
			foreach ( InputPort port in InputPorts )
			{
				if ( port.IsConnected )
				{
					valueInstructions += "Port: " + port.PortId + " Value: " + port.GenerateShaderForOutput( ref dataCollector, port.DataType, false );
				}
			}
			Debug.Log( "Value: " + valueDump );
			Debug.Log( "Instructions: " + valueInstructions );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
		}

	}
}
