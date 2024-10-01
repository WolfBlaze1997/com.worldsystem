


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	public enum BuiltInFogAndAmbientColors
	{
		UNITY_LIGHTMODEL_AMBIENT = 0,
		unity_AmbientSky,
		unity_AmbientEquator,
		unity_AmbientGround,
		unity_FogColor
	}

	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Fog And Ambient Colors"
#else
"雾和环境色"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Lighting"
#else
"照明"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Fog and Ambient colors"
#else
"雾和环境色"
#endif
)]
	public sealed class FogAndAmbientColorsNode : ShaderVariablesNode
	{
		private const string ColorLabelStr = 
#if !WB_LANGUAGE_CHINESE
"Color"
#else
"颜色"
#endif
;
		private readonly string[] ColorValuesStr = {
														"Ambient light ( Legacy )",
														"Sky ambient light",
														"Equator ambient light",
														"Ground ambient light",
														"Fog"
													};

		[SerializeField]
		private BuiltInFogAndAmbientColors m_selectedType = BuiltInFogAndAmbientColors.UNITY_LIGHTMODEL_AMBIENT;
		
		private UpperLeftWidgetHelper m_upperLeftWidget = new UpperLeftWidgetHelper();

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, ColorValuesStr[ ( int ) m_selectedType ], WirePortDataType.COLOR );
			m_textLabelWidth = 50;
			m_autoWrapProperties = true;
			m_hasLeftDropdown = true;
			m_previewShaderGUID = "937c7bde062f0f942b600d9950d2ebb2";
		}

		public override void AfterCommonInit()
		{
			base.AfterCommonInit();
			if( PaddingTitleLeft == 0 )
			{
				PaddingTitleLeft = Constants.PropertyPickerWidth + Constants.IconsLeftRightMargin;
				if( PaddingTitleRight == 0 )
					PaddingTitleRight = Constants.PropertyPickerWidth + Constants.IconsLeftRightMargin;
			}
		}

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();
			m_previewMaterialPassId = (int)m_selectedType;
		}

		public override void Destroy()
		{
			base.Destroy();
			m_upperLeftWidget = null;
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );
			EditorGUI.BeginChangeCheck();
			m_selectedType = (BuiltInFogAndAmbientColors)m_upperLeftWidget.DrawWidget( this, (int)m_selectedType, ColorValuesStr );
			if( EditorGUI.EndChangeCheck() )
			{
				ChangeOutputName( 0, ColorValuesStr[ (int)m_selectedType ] );
			}
		}
		
		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			m_selectedType = ( BuiltInFogAndAmbientColors ) EditorGUILayoutPopup( ColorLabelStr, ( int ) m_selectedType, ColorValuesStr );

			if ( EditorGUI.EndChangeCheck() )
			{
				ChangeOutputName( 0, ColorValuesStr[ ( int ) m_selectedType ] );
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			if( dataCollector.IsTemplate && dataCollector.CurrentSRPType == TemplateSRPType.HDRP )
			{
				switch( m_selectedType )
				{
					case BuiltInFogAndAmbientColors.unity_AmbientSky:
					return "_Ambient_ColorSky";
					case BuiltInFogAndAmbientColors.unity_AmbientEquator:
					return "_Ambient_Equator";
					case BuiltInFogAndAmbientColors.unity_AmbientGround:
					return "_Ambient_Ground";
					case BuiltInFogAndAmbientColors.unity_FogColor:
					return "_FogColor";
				}
			}
			return m_selectedType.ToString();
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_selectedType = ( BuiltInFogAndAmbientColors ) Enum.Parse( typeof( BuiltInFogAndAmbientColors ), GetCurrentParam( ref nodeParams ) );
			ChangeOutputName( 0, ColorValuesStr[ ( int ) m_selectedType ] );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedType );
		}
	}
}
