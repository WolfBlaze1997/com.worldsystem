


using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Reflection Probe"
#else
"反射探头"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Provides access to the nearest Reflection Probe to the object. Only available on URP."
#else
"提供对距离对象最近的反射探测器的访问。仅在URP上可用。"
#endif
)]
	public class ReflectionProbeNode : ParentNode
	{
		private const string ReflectionProbeStr = "SHADERGRAPH_REFLECTION_PROBE({0},{1},{2})";
		private const string InfoTransformSpace = 
#if !WB_LANGUAGE_CHINESE
"Both View Dir and Normal vectors are set in Object Space"
#else
"视图方向和法线矢量都在对象空间中设置"
#endif
;
		public const string NodeErrorMsg = 
#if !WB_LANGUAGE_CHINESE
"Only valid on URP"
#else
"仅在URP上有效"
#endif
;
		public const string ErrorOnCompilationMsg = "Attempting to use URP specific node on incorrect SRP or Builtin RP.";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3 , false , 
#if !WB_LANGUAGE_CHINESE
"View Dir"
#else
"查看目录"
#endif
);
			AddInputPort( WirePortDataType.FLOAT3 , false , 
#if !WB_LANGUAGE_CHINESE
"Normal"
#else
"正常"
#endif
);
			AddInputPort( WirePortDataType.FLOAT , false , "LOD" );
			AddOutputPort( WirePortDataType.FLOAT3 , "Out" );
			m_autoWrapProperties = true;
			m_errorMessageTooltip = NodeErrorMsg;
			m_errorMessageTypeIsError = NodeMessageType.Error;
		}

		public override void OnNodeLogicUpdate( DrawInfo drawInfo )
		{
			base.OnNodeLogicUpdate( drawInfo );
			m_showErrorMessage = ( ContainerGraph.CurrentCanvasMode == NodeAvailability.SurfaceShader ) ||
									( ContainerGraph.CurrentCanvasMode == NodeAvailability.TemplateShader && ContainerGraph.CurrentSRPType != TemplateSRPType.URP );
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUILayout.HelpBox( InfoTransformSpace , MessageType.Info );
			if( m_showErrorMessage )
			{
				EditorGUILayout.HelpBox( NodeErrorMsg , MessageType.Error );
			}
		}

		public override string GenerateShaderForOutput( int outputId , ref MasterNodeDataCollector dataCollector , bool ignoreLocalvar )
		{
			if( !dataCollector.IsSRP || !dataCollector.TemplateDataCollectorInstance.IsLWRP )
			{
				UIUtils.ShowMessage( ErrorOnCompilationMsg , MessageSeverity.Error );
				return GenerateErrorValue();
			}

			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string viewDir = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string normal = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
			string lod = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );

			RegisterLocalVariable( outputId , string.Format( ReflectionProbeStr , viewDir , normal , lod ), ref dataCollector );
			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
