


using UnityEngine;
using UnityEditor;
using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Linear To Gamma"
#else
"线性到Gamma"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Converts color from linear space to gamma space"
#else
"将颜色从线性空间转换到gamma空间"
#endif
)]
	public sealed class LinearToGammaNode : HelperParentNode
	{
		
		

		

		public readonly static string[] ModeListStr = { "Fast Linear to sRGB", "Exact Linear to sRGB" };
		public readonly static int[] ModeListInt = { 0, 1 };

		public readonly static string[] ModeListStrLW = { "Fast Linear to sRGB", "Exact Linear to sRGB", "Linear to Gamma 2.0", "Linear to Gamma 2.2" };
		public readonly static int[] ModeListIntLW = { 0, 1, 2, 3 };

		[SerializeField]
		public int m_selectedMode = 0;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "LinearToGammaSpace";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_inputPorts[ 0 ].Name = "RGB";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_autoWrapProperties = true;
			m_previewShaderGUID = "9027c408b928c5c4d8b450712049d541";
			m_textLabelWidth = 120;
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "linearToGamma" + OutputId;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			if( ContainerGraph.IsSRP )
			{
				m_selectedMode = EditorGUILayoutIntPopup( "Mode", m_selectedMode, ModeListStrLW, ModeListIntLW );
				EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Fast Linear: fast approximation from Linear to sRGB\n\nExact Linear: a more expensive but exact calculation from Linear to sRGB.\n\nLinear 2.0: crude approximation from Linear to Gamma using a power of 1/2.0 gamma value\n\nLinear 2.2: an approximation from Linear to Gamma using a power of 1/2.2 gamma value"
#else
"快速线性：从线性到sRGB的快速近似\n\n精确线性：从直线到sRGB更昂贵但更精确的计算。\n\n线性2.0：使用1/2.0的伽马值幂从线性到伽马的粗略近似\n线性2.2：使用1/2.2的伽马值的幂从线性近似到伽马"
#endif
, MessageType.None );
			}
			else
			{
				m_selectedMode = EditorGUILayoutIntPopup( "Mode", m_selectedMode, ModeListStr, ModeListInt );
				EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Fast Linear: fast approximation from Linear to sRGB\n\nExact Linear: a more expensive but exact calculation from Linear to sRGB."
#else
"快速线性：从线性到sRGB的快速近似\n\n精确线性：从直线到sRGB更昂贵但更精确的计算。"
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
					dataCollector.AddLocalVariable( UniqueId, m_localVarName + " = half3( LinearToGammaSpaceExact(" + m_localVarName + ".r), LinearToGammaSpaceExact(" + m_localVarName + ".g), LinearToGammaSpaceExact(" + m_localVarName + ".b) );" );
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
					m_funcLWFormatOverride = "FastLinearToSRGB( {0} )";
					m_funcHDFormatOverride = "FastLinearToSRGB( {0} )";
					break;
					case 1:
					m_funcLWFormatOverride = "LinearToSRGB( {0} )";
					m_funcHDFormatOverride = "LinearToSRGB( {0} )";
					break;
					case 2:
					m_funcLWFormatOverride = "LinearToGamma20( {0} )";
					m_funcHDFormatOverride = "LinearToGamma20( {0} )";
					break;
					case 3:
					m_funcLWFormatOverride = "LinearToGamma22( {0} )";
					m_funcHDFormatOverride = "LinearToGamma22( {0} )";
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
