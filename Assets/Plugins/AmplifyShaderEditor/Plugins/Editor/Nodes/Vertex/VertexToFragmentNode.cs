





using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Vertex To Fragment"
#else
"顶点到碎片"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Pass vertex data to the pixel shader"
#else
"将顶点数据传递给像素着色器"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"Jason Booth - http://u3d.as/DND"
#else
"杰森·布斯http://u3d.as/DND"
#endif
)]
	public sealed class VertexToFragmentNode : SingleInputOp
	{
		private const string DisabledInterpolatorMsg = 
#if !WB_LANGUAGE_CHINESE
"No Interpolation option cannot be used over Standard Surface type as we must be able to directly control interpolators registry, which does't happen over this shader type. Please disable it."
#else
"“无插值”选项不能在“标准曲面”类型上使用，因为我们必须能够直接控制插值器注册表，而这在该着色器类型上不会发生。请禁用它。"
#endif
;
		private const string NoInterpolationUsageMsg = 
#if !WB_LANGUAGE_CHINESE
"No interpolation is performed when passing value from vertex to fragment during rasterization. Please note this option will not work across all API's and can even throw compilation errors on some of them ( p.e. Metal and GLES 2.0 )"
#else
"在光栅化过程中将值从顶点传递到片段时，不会执行插值。请注意，此选项不适用于所有API，甚至可能在其中一些API上引发编译错误（p.e.Metal和GLES 2.0）"
#endif
;

		private const string SampleInfoMessage = 
#if !WB_LANGUAGE_CHINESE
"Interpolate at sample location rather than at the pixel center. This causes the pixel shader to execute per-sample rather than per-pixel. Only available in shader model 4.1 or higher"
#else
"在样本位置而不是像素中心进行插值。这会导致像素着色器按采样而不是按像素执行。仅在着色器模型4.1或更高版本中可用"
#endif
;

		[SerializeField]
		private bool m_noInterpolation;

		[SerializeField]
		private bool m_sample;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputPorts[ 0 ].AddPortForbiddenTypes(	WirePortDataType.FLOAT3x3,
														WirePortDataType.FLOAT4x4,
														WirePortDataType.SAMPLER1D,
														WirePortDataType.SAMPLER2D,
														WirePortDataType.SAMPLER3D,
														WirePortDataType.SAMPLERCUBE,
														WirePortDataType.SAMPLER2DARRAY,
														WirePortDataType.SAMPLERSTATE );
			m_inputPorts[ 0 ].Name = "(VS) In";
			m_outputPorts[ 0 ].Name = "Out";
			m_useInternalPortData = false;
			m_autoWrapProperties = true;
			m_errorMessageTypeIsError = NodeMessageType.Warning;
			m_previewShaderGUID = "74e4d859fbdb2c0468de3612145f4929";
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			bool isSurface = ContainerGraph.IsStandardSurface;
			EditorGUI.BeginDisabledGroup( isSurface && !m_noInterpolation );
			m_noInterpolation = EditorGUILayoutToggle( 
#if !WB_LANGUAGE_CHINESE
"No Interpolation"
#else
"无插值"
#endif
, m_noInterpolation );
			EditorGUI.EndDisabledGroup();
			if( m_noInterpolation  )
			{
				if( isSurface )
				{
					EditorGUILayout.HelpBox( DisabledInterpolatorMsg, MessageType.Warning );
				} else
				{
					EditorGUILayout.HelpBox( NoInterpolationUsageMsg, MessageType.Info );
				}
			}

			EditorGUI.BeginDisabledGroup( isSurface && !m_sample );
			m_sample = EditorGUILayoutToggle( 
#if !WB_LANGUAGE_CHINESE
"Sample"
#else
"样品"
#endif
, m_sample );
			EditorGUI.EndDisabledGroup();
			if( m_sample )
				EditorGUILayout.HelpBox( SampleInfoMessage , MessageType.Info ); 
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );
			m_showErrorMessage =	ContainerGraph.IsStandardSurface && m_noInterpolation ||
									ContainerGraph.IsStandardSurface && m_sample;
		}


		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			bool noInterpolationFlag = dataCollector.IsTemplate ? m_noInterpolation : false;
			bool sampleFlag = dataCollector.IsTemplate ? m_sample : false;
			string varName = GenerateInputInVertex( ref dataCollector, 0, "vertexToFrag" + OutputId,true, noInterpolationFlag, sampleFlag );
			m_outputPorts[ 0 ].SetLocalValue( varName, dataCollector.PortCategory );

			return varName;

			
			
			
			
			

			
			
			

			
			
			
			

			

			
			
			
			
			
			
			

			

			
			

			
			
			

			
			
			
			
			
			

			
			
			
			
			


			
			

			
			

			

			

			
			

			

			
			
			
			
			
			

			

			

			
			
			
		}
		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 18707 )
				m_noInterpolation = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );

			if( UIUtils.CurrentShaderVersion() > 18808 )
				m_sample = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_noInterpolation );
			IOUtils.AddFieldValueToString( ref nodeInfo , m_sample );
		}
	}
}
