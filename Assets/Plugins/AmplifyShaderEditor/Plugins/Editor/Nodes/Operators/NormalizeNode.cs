


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Normalize"
#else
"正常化"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vector Operators"
#else
"矢量运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Normalizes a vector"
#else
"规范化向量"
#endif
, null, KeyCode.N )]
	public sealed class NormalizeNode : SingleInputOp
	{
		[SerializeField]
		private bool m_safeNormalize = false;

		private const string SubtitleFormat = "({0})";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_selectedLocation = PreviewLocation.TopCenter;
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_inputPorts[ 0 ].CreatePortRestrictions( WirePortDataType.FLOAT, WirePortDataType.FLOAT2, WirePortDataType.FLOAT3, WirePortDataType.FLOAT4, WirePortDataType.COLOR, WirePortDataType.OBJECT, WirePortDataType.INT );

			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );

			m_previewShaderGUID = "a51b11dfb6b32884e930595e5f9defa8";
			m_autoWrapProperties = true;

			m_textLabelWidth = 100;

			UpdateSubtitle();
		}

		private void UpdateSubtitle()
		{
			SetAdditonalTitleText( m_safeNormalize ? string.Format( SubtitleFormat, "Safe" ) : "" );
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			m_safeNormalize = EditorGUILayoutToggle( 
#if !WB_LANGUAGE_CHINESE
"Safe Normalize"
#else
"安全正常化"
#endif
, m_safeNormalize );
			if ( EditorGUI.EndChangeCheck() )
			{
				UpdateSubtitle();
			}
			EditorGUILayout.HelpBox( Constants.SafeNormalizeInfoStr, MessageType.Info );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string result = string.Empty;
			switch ( m_inputPorts[ 0 ].DataType )
			{
				case WirePortDataType.FLOAT:
				case WirePortDataType.FLOAT2:
				case WirePortDataType.FLOAT3:
				case WirePortDataType.FLOAT4:
				case WirePortDataType.OBJECT:
				case WirePortDataType.COLOR:
				{
					string value = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
					result = GeneratorUtils.NormalizeValue( ref dataCollector, m_safeNormalize, m_inputPorts[ 0 ].DataType, value );
				}
				break;
				case WirePortDataType.INT:
				{
					return m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
				}
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					result = UIUtils.InvalidParameter( this );
				}
				break;
			}
			RegisterLocalVariable( 0, result, ref dataCollector, "normalizeResult" + OutputId );

			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if ( UIUtils.CurrentShaderVersion() > 18814 )
			{
				m_safeNormalize = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
				UpdateSubtitle();
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_safeNormalize );
		}
	}
}
