


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Ortho Params"
#else
"正交参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Orthographic Parameters"
#else
"正交参数"
#endif
)]
	public sealed class OrthoParams : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"Ortho Cam Width"
#else
"正交凸轮宽度"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"Ortho Cam Height"
#else
"正交凸轮高度"
#endif
);
			ChangeOutputName( 3, 
#if !WB_LANGUAGE_CHINESE
"Unused"
#else
"未使用"
#endif
);
			ChangeOutputName( 4, 
#if !WB_LANGUAGE_CHINESE
"Projection Mode"
#else
"投影模式"
#endif
);
			m_value = "unity_OrthoParams";
			m_previewShaderGUID = "88a910ece3dce224793e669bb1bc158d";
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			if( !m_outputPorts[ 0 ].IsConnected )
			{
				m_outputPorts[ 0 ].Visible = false;
				m_sizeIsDirty = true;
			}

			if( !m_outputPorts[ 3 ].IsConnected )
			{
				m_outputPorts[ 3 ].Visible = false;
				m_sizeIsDirty = true;
			}
		}
	}
}
