


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Screen Params"
#else
"屏幕参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Camera's Render Target size parameters"
#else
"摄影机的渲染目标大小参数"
#endif
)]
	public sealed class ScreenParams : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"RT Width"
#else
"RT宽度"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"RT Height"
#else
"RT高度"
#endif
);
			ChangeOutputName( 3, 
#if !WB_LANGUAGE_CHINESE
"1+1/Width"
#else
"1+1/宽度"
#endif
);
			ChangeOutputName( 4, 
#if !WB_LANGUAGE_CHINESE
"1+1/Height"
#else
"1+1/高度"
#endif
);
			m_value = "_ScreenParams";
			m_previewShaderGUID = "78173633b803de4419206191fed3d61e";
		}

		
		
		
		
		
		
		
		
		
	}
}
