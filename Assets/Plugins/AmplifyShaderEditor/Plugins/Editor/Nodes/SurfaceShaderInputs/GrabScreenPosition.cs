


using System;
using UnityEditor;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Grab Screen Position"
#else
"抓取屏幕位置"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Screen position correctly transformed to be used with Grab Screen Color"
#else
"屏幕位置已正确转换，可与抓取屏幕颜色一起使用"
#endif
)]
	public sealed class GrabScreenPosition : ParentNode
	{
		private readonly string[] m_outputTypeStr = { "Normalized", "Screen" };

		[SerializeField]
		private int m_outputTypeInt = 0;

		private UpperLeftWidgetHelper m_upperLeftWidget = new UpperLeftWidgetHelper();

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputVectorPorts( WirePortDataType.FLOAT4, "XYZW" );
			m_autoWrapProperties = true;
			m_hasLeftDropdown = true;
			m_textLabelWidth = 65;
			ConfigureHeader();
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

		public override void Destroy()
		{
			base.Destroy();
			m_upperLeftWidget = null;
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );
			EditorGUI.BeginChangeCheck();
			m_outputTypeInt = m_upperLeftWidget.DrawWidget( this, m_outputTypeInt, m_outputTypeStr );
			if( EditorGUI.EndChangeCheck() )
			{
				ConfigureHeader();
			}
		}

		public override void DrawProperties()
		{
			base.DrawProperties();

			EditorGUI.BeginChangeCheck();
			m_outputTypeInt = EditorGUILayoutPopup( 
#if !WB_LANGUAGE_CHINESE
"Type"
#else
"类型"
#endif
, m_outputTypeInt, m_outputTypeStr );
			if( EditorGUI.EndChangeCheck() )
			{
				ConfigureHeader();
			}
		}

		void ConfigureHeader()
		{
			SetAdditonalTitleText( string.Format( Constants.SubTitleTypeFormatStr, m_outputTypeStr[ m_outputTypeInt ] ) );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return GetOutputColorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );

			string localVarName = string.Empty;

			if( m_outputTypeInt == 0 )
				localVarName = GeneratorUtils.GenerateGrabScreenPositionNormalized( ref dataCollector, UniqueId, CurrentPrecisionType, !dataCollector.UsingCustomScreenPos );
			else
				localVarName = GeneratorUtils.GenerateGrabScreenPosition( ref dataCollector, UniqueId, CurrentPrecisionType, !dataCollector.UsingCustomScreenPos );

			m_outputPorts[ 0 ].SetLocalValue( localVarName, dataCollector.PortCategory );
			return GetOutputColorItem( 0, outputId, localVarName );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 3108 )
			{
				if( UIUtils.CurrentShaderVersion() < 6102 )
				{
					bool project = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
					m_outputTypeInt = project ? 0 : 1;
				}
				else
				{
					m_outputTypeInt = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
				}
			}

			ConfigureHeader();
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_outputTypeInt );
		}
	}
}
