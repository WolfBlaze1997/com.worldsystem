


using UnityEngine;
using UnityEditor;
using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Gamma To Linear"
#else
"Gamma到线性"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Converts color from gamma space to linear space"
#else
"将颜色从伽玛空间转换为线性空间"
#endif
)]
	public sealed class GammaToLinearNode : HelperParentNode
	{
		public readonly static string[] ModeListStr = { "Fast sRGB to Linear", "Exact sRGB to Linear" };
		public readonly static int[] ModeListInt = { 0, 1 };

		public readonly static string[] ModeListStrLW = { "Fast sRGB to Linear", "Exact sRGB to Linear", "Gamma 2.0 to Linear", "Gamma 2.2 to Linear" };
		public readonly static int[] ModeListIntLW = { 0, 1, 2, 3 };

		[SerializeField]
		public int m_selectedMode = 0;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "GammaToLinearSpace";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_inputPorts[ 0 ].Name = "RGB";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_autoWrapProperties = true;
			m_previewShaderGUID = "e82a888a6ebdb1443823aafceaa051b9";
			m_textLabelWidth = 120;
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "gammaToLinear" + OutputId;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			if( ContainerGraph.IsSRP )
			{
				m_selectedMode = EditorGUILayoutIntPopup( "Mode", m_selectedMode, ModeListStrLW, ModeListIntLW );
				EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Fast sRGB: fast approximation from sRGB to Linear\n\nExact sRGB: a more expensive but exact calculation from sRGB to Linear.\n\nGamma 2.0: crude approximation from Gamma to Linear using a power of 2.0 gamma value\n\nGamma 2.2: an approximation from Gamma to Linear using a power of 2.2 gamma value"
#else
"快速sRGB：从sRGB到线性的快速近似\n\n精确sRGB：一种更昂贵但更精确的从sRGB计算到线性的方法。\n\nGamma 2.0：使用2.0次幂的Gamma值从Gamma到Linear的粗略近似值\n\nGamma 2.2：使用2.2次幂的Gamma值从Gamma到Liner的近似值"
#endif
, MessageType.None );
			}
			else
			{
				m_selectedMode = EditorGUILayoutIntPopup( "Mode", m_selectedMode, ModeListStr, ModeListInt );
				EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Fast sRGB: fast approximation from sRGB to Linear\n\nExact sRGB: a more expensive but exact calculation from sRGB to Linear."
#else
"快速sRGB：从sRGB到线性的快速近似\n\n精确sRGB：一种更昂贵但更精确的从sRGB计算到线性的方法。"
#endif
, MessageType.None );
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string result = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );

			if( !dataCollector.IsSRP )
			{
				m_selectedMode = Mathf.Min( m_selectedMode, 1 );

				if( m_selectedMode == 1 )
				{
					dataCollector.AddLocalVariable( UniqueId, "half3 " + m_localVarName + " = " + result + ";" );
					dataCollector.AddLocalVariable( UniqueId, m_localVarName + " = half3( GammaToLinearSpaceExact(" + m_localVarName + ".r), GammaToLinearSpaceExact(" + m_localVarName + ".g), GammaToLinearSpaceExact(" + m_localVarName + ".b) );" );
					return m_localVarName;
				}
				return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			}
			else
			{
				dataCollector.AddToIncludes( UniqueId, TemplateHelperFunctions.CoreCommonLib );
				dataCollector.AddToIncludes( UniqueId, TemplateHelperFunctions.CoreColorLib );
				switch( m_selectedMode )
				{
					default:
					case 0:
					m_funcLWFormatOverride = "FastSRGBToLinear( {0} )";
					m_funcHDFormatOverride = "FastSRGBToLinear( {0} )";
					break;
					case 1:
					m_funcLWFormatOverride = "SRGBToLinear( {0} )";
					m_funcHDFormatOverride = "SRGBToLinear( {0} )";
					break;
					case 2:
					m_funcLWFormatOverride = "Gamma20ToLinear( {0} )";
					m_funcHDFormatOverride = "Gamma20ToLinear( {0} )";
					break;
					case 3:
					m_funcLWFormatOverride = "Gamma22ToLinear( {0} )";
					m_funcHDFormatOverride = "Gamma22ToLinear( {0} )";
					break;
				}

				return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedMode );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 11003 && UIUtils.CurrentShaderVersion() <= 14503 )
			{
				bool fast = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
				if( fast )
					m_selectedMode = 1;
			}

			if( UIUtils.CurrentShaderVersion() > 14503 )
			{
				m_selectedMode = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			}
		}
	}
}
