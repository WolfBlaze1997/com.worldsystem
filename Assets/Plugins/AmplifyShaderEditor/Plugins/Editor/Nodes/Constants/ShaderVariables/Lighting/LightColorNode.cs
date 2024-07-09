// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( "Light Color", "Lighting", "Light Color, RGB value already contains light intensity while A only contains light intensity" )]
	public sealed class LightColorNode : ShaderVariablesNode
	{
		private const string m_lightColorValue = "_LightColor0";

		private const string m_localIntensityVar = "ase_lightIntensity";
		private const string m_localColorVar = "ase_lightColor";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "RGBA", WirePortDataType.COLOR );
			AddOutputPort( WirePortDataType.FLOAT3, "Color" );
			AddOutputPort( WirePortDataType.FLOAT, "Intensity" );
			m_previewShaderGUID = "43f5d3c033eb5044e9aeb40241358349";
		}

		public override void RenderNodePreview()
		{
			//Runs at least one time
			if( !m_initialized )
			{
				// nodes with no preview don't update at all
				PreviewIsDirty = false;
				return;
			}

			if( !PreviewIsDirty )
				return;
			if( !Preferences.GlobalDisablePreviews )
			{
				int count = m_outputPorts.Count;
				for( int i = 0 ; i < count ; i++ )
				{
					RenderTexture temp = RenderTexture.active;
					RenderTexture.active = m_outputPorts[ i ].OutputPreviewTexture;
					Graphics.Blit( null , m_outputPorts[ i ].OutputPreviewTexture , PreviewMaterial , i );
					RenderTexture.active = temp;
				}
			}

			PreviewIsDirty = m_continuousPreviewRefresh;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.IsTemplate && !dataCollector.IsSRP )
				dataCollector.AddToIncludes( -1, Constants.UnityLightingLib );

			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			
			if ( dataCollector.IsTemplate && dataCollector.IsSRP )
			{
				string constantVar;
				if ( dataCollector.TemplateDataCollectorInstance.CurrentSRPType == TemplateSRPType.HDRP )
				{
					dataCollector.TemplateDataCollectorInstance.AddHDLightInfo();
					constantVar = string.Format( TemplateHelperFunctions.HDLightInfoFormat, "0", "color" ); ;
				}
				else
				{
					constantVar = "_MainLightColor";
				}

				dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, m_localIntensityVar, 
					string.Format( "max( max( {0}.r, {0}.g ), {0}.b )", constantVar ) );

				dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT4, m_localColorVar,
					string.Format( "float4( {0}.rgb / {1}, {1} )", constantVar, m_localIntensityVar ) );
			}
			else
			{
				dataCollector.AddLocalVariable( UniqueId, "#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc" );
				dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT4, m_localColorVar, "0" );
				dataCollector.AddLocalVariable( UniqueId, "#else //aselc" );
				dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT4, m_localColorVar, m_lightColorValue );
				dataCollector.AddLocalVariable( UniqueId, "#endif //aselc" );
			}
			//else if( ContainerGraph.CurrentStandardSurface.CurrentLightingModel == StandardShaderLightModel.CustomLighting )
			//	finalVar = "gi.light.color";

			switch ( outputId )
			{
				default:
				case 0: return m_localColorVar;
				case 1: return m_localColorVar + ".rgb";
				case 2: return m_localColorVar + ".a";
			}
		}
	}
}
