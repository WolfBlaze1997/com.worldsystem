



using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Posterize"
#else
"背面"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Converts a continuous gradation of tones to multiple regions of fewer tones"
#else
"将连续的色调渐变转换为色调较少的多个区域"
#endif
)]
	public sealed class PosterizeNode : ParentNode
	{
		private const string PosterizationPowerStr = "Power";
		[SerializeField]
		private int m_posterizationPower = 1;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.COLOR, false, "RGBA", -1, MasterNodePortCategory.Fragment, 1 );
			AddInputPort( WirePortDataType.INT, false, 
#if !WB_LANGUAGE_CHINESE
"Power"
#else
"电源"
#endif
, -1, MasterNodePortCategory.Fragment, 0 );
			m_inputPorts[ 1 ].AutoDrawInternalData = true;
			AddOutputPort( WirePortDataType.COLOR, Constants.EmptyPortValue );
			m_textLabelWidth = 60;
			m_autoWrapProperties = true;
			m_previewShaderGUID = "ecb3048ef0eec1645bad1d72a98d8279";
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			if( !m_inputPorts[ 1 ].IsConnected )
			{
				EditorGUILayout.BeginVertical();
				{
					EditorGUI.BeginChangeCheck();
					m_posterizationPower = EditorGUILayoutIntSlider( PosterizationPowerStr, m_posterizationPower, 1, 256 );
					if( EditorGUI.EndChangeCheck() )
					{
						GetInputPortByUniqueId( 0 ).IntInternalData = m_posterizationPower;
					}
				}
				EditorGUILayout.EndVertical();
			}
			else
			{
				EditorGUILayout.Space();
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string posterizationPower = "1";
			if( m_inputPorts[ 1 ].IsConnected )
			{
				posterizationPower = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
			}
			else
			{
				posterizationPower = m_posterizationPower.ToString();
			}

			string colorTarget = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );

			string divVar = "div" + OutputId;
			dataCollector.AddLocalVariable( UniqueId, "float " + divVar + "=256.0/float(" + posterizationPower + ");" );
			string result = "( floor( " + colorTarget + " * " + divVar + " ) / " + divVar + " )";

			RegisterLocalVariable( 0, result, ref dataCollector, "posterize" + OutputId );

			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.COLOR, false );
			m_inputPorts[ 1 ].ChangeType( WirePortDataType.INT, false );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_posterizationPower );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_posterizationPower = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
		}
	}
}
