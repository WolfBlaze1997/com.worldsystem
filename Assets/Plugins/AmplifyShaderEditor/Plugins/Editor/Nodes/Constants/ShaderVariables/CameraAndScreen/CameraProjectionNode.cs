


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	public enum BuiltInShaderCameraTypes
	{
		unity_CameraProjection = 0,
		unity_CameraInvProjection
	}

	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Projection Matrices"
#else
"投影矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Camera's Projection/Inverse Projection matrix"
#else
"相机投影/逆投影矩阵"
#endif
)]
	public sealed class CameraProjectionNode : ShaderVariablesNode
	{
		private const string _projMatrixLabelStr = 
#if !WB_LANGUAGE_CHINESE
"Projection Matrix"
#else
"投影矩阵"
#endif
;
		private readonly string[] _projMatrixValuesStr = {  "Camera Projection",
															"Inverse Camera Projection"};


		[SerializeField]
		private BuiltInShaderCameraTypes m_selectedType = BuiltInShaderCameraTypes.unity_CameraProjection;

		private UpperLeftWidgetHelper m_upperLeftWidget = new UpperLeftWidgetHelper();

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, _projMatrixValuesStr[ (int)m_selectedType ], WirePortDataType.FLOAT4x4 );
			m_textLabelWidth = 115;
			m_autoWrapProperties = true;
			m_hasLeftDropdown = true;
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
			m_selectedType = (BuiltInShaderCameraTypes)m_upperLeftWidget.DrawWidget( this, (int)m_selectedType, _projMatrixValuesStr );
			if( EditorGUI.EndChangeCheck() )
			{
				ChangeOutputName( 0, _projMatrixValuesStr[ (int)m_selectedType ] );
				SetSaveIsDirty();
			}
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			m_selectedType = (BuiltInShaderCameraTypes)EditorGUILayoutPopup( _projMatrixLabelStr, (int)m_selectedType, _projMatrixValuesStr );
			if( EditorGUI.EndChangeCheck() )
			{
				ChangeOutputName( 0, _projMatrixValuesStr[ (int)m_selectedType ] );
				SetSaveIsDirty();
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			GeneratorUtils.RegisterUnity2019MatrixDefines( ref dataCollector );
			return m_selectedType.ToString();
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_selectedType = (BuiltInShaderCameraTypes)Enum.Parse( typeof( BuiltInShaderCameraTypes ), GetCurrentParam( ref nodeParams ) );
			ChangeOutputName( 0, _projMatrixValuesStr[ (int)m_selectedType ] );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedType );
		}
	}
}
