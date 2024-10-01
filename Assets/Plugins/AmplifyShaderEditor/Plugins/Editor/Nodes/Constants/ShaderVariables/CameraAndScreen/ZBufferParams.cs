


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Z-Buffer Params"
#else
"Z缓冲区参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Linearized Z buffer values"
#else
"线性化Z缓冲值"
#endif
)]
	public sealed class ZBufferParams : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"1-far/near"
#else
"1-远/近"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"far/near"
#else
"远/近"
#endif
);
			ChangeOutputName( 3, "[0]/far" );
			ChangeOutputName( 4, "[1]/far" );
			m_value = "_ZBufferParams";
			m_previewShaderGUID = "56c42c106bcb497439187f5bb6b6f94d";
		}
		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			if( !m_outputPorts[ 0 ].IsConnected )
			{
				m_outputPorts[ 0 ].Visible = false;
				m_sizeIsDirty = true;
			}
		}
	}
}
