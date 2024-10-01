





using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"HeightMap Texture Blend"
#else
"高度贴图纹理混合"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Textures"
#else
"纹理"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Advanced Texture Blending by using heightMap and splatMask, usefull for texture layering "
#else
"使用heightMap和splatMask进行高级纹理混合，用于纹理分层"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"Rea"
#else
"雷亚"
#endif
)]
	public sealed class HeightMapBlendNode : ParentNode
	{
		private const string PreventNaNLabel = 
#if !WB_LANGUAGE_CHINESE
"Prevent NaN"
#else
"防止NaN"
#endif
;
		private const string PreventNaNInfo = 
#if !WB_LANGUAGE_CHINESE
"Prevent NaN clamps negative base numbers over the internal pow instruction to 0 since these originate NaN."
#else
"防止NaN将内部pow指令上的负基数箝位为0，因为这些数字是NaN的起源。"
#endif
;
		[SerializeField]
		private bool m_preventNaN = false;
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"HeightMap"
#else
"高度图"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"SplatMask"
#else
"SplatMask"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"BlendStrength"
#else
"混合强度"
#endif
);
			AddOutputVectorPorts( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_textLabelWidth = 120;
			m_useInternalPortData = true;
			m_inputPorts[ 2 ].FloatInternalData = 1;
			m_autoWrapProperties = true;
			m_previewShaderGUID = "b2ac23d6d5dcb334982b6f31c2e7a734";
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_preventNaN = EditorGUILayoutToggle( PreventNaNLabel , m_preventNaN );
			EditorGUILayout.HelpBox( PreventNaNInfo , MessageType.Info );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string HeightMap = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string SplatMask = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector);
			string Blend = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );
			string baseOp = "((" + HeightMap + "*" + SplatMask + ")*4)+(" + SplatMask + "*2)";
			if( m_preventNaN )
				baseOp = "max( (" + baseOp + "), 0 )";
			string HeightMask =  "saturate(pow("+baseOp+"," + Blend + "))";
			string varName = "HeightMask" + OutputId;

			RegisterLocalVariable( 0, HeightMask, ref dataCollector , varName );
			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
		

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 18910 )
				m_preventNaN = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteToString( ref string nodeInfo , ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo , ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo , m_preventNaN );
		}
	}
}
