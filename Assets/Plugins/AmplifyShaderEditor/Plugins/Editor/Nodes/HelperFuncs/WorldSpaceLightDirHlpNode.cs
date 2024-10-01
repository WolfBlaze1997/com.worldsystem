


using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World Space Light Dir"
#else
"世界空间灯光总监"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Lighting"
#else
"照明"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Computes normalized world space light direction"
#else
"计算归一化世界空间光方向"
#endif
)]
	public sealed class WorldSpaceLightDirHlpNode : HelperParentNode
	{
		private const string NormalizeOptionStr = 
#if !WB_LANGUAGE_CHINESE
"Safe Normalize"
#else
"安全正常化"
#endif
;

		[SerializeField]
		private bool m_safeNormalize = false;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "UnityWorldSpaceLightDir";
			m_inputPorts[ 0 ].Visible = false;
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].Name = "XYZ";

			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );

			m_useInternalPortData = false;
			m_drawPreviewAsSphere = true;
			m_autoWrapProperties = true;
			m_textLabelWidth = 120;
			m_previewShaderGUID = "2e8dc46eb6fb2124d9f0007caf9567e3";
		}

		public override void PropagateNodeData( NodeData nodeData, ref MasterNodeDataCollector dataCollector )
		{
			base.PropagateNodeData( nodeData, ref dataCollector );
			if( m_safeNormalize )
				dataCollector.SafeNormalizeLightDir = true;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_safeNormalize = EditorGUILayoutToggle( NormalizeOptionStr, m_safeNormalize );
			EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Having safe normalize ON makes sure your light vector is not zero even if there's no lights in your scene."
#else
"启用安全归一化可确保即使场景中没有灯光，光矢量也不为零。"
#endif
, MessageType.None );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.IsTemplate )
				return GetOutputVectorItem( 0, outputId, dataCollector.TemplateDataCollectorInstance.GetWorldSpaceLightDir( CurrentPrecisionType ) ); ;

			dataCollector.AddToIncludes( UniqueId, Constants.UnityCgLibFuncs );
			dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_POS );

			return GetOutputVectorItem( 0, outputId, GeneratorUtils.GenerateWorldLightDirection( ref dataCollector, UniqueId, CurrentPrecisionType ) );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 15201 )
			{
				m_safeNormalize = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_safeNormalize );
		}
	}
}
