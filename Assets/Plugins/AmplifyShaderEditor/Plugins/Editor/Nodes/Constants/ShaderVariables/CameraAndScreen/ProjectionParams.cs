


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Projection Params"
#else
"投影参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Projection Near/Far parameters"
#else
"投影近/远参数"
#endif
)]
	public sealed class ProjectionParams : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"Flipped"
#else
"轻弹"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"Near Plane"
#else
"近平面"
#endif
);
			ChangeOutputName( 3, 
#if !WB_LANGUAGE_CHINESE
"Far Plane"
#else
"远飞机"
#endif
);
			ChangeOutputName( 4, 
#if !WB_LANGUAGE_CHINESE
"1/Far Plane"
#else
"1/远平面"
#endif
);
			m_value = "_ProjectionParams";
			m_previewShaderGUID = "97ae846cb0a6b044388fad3bc03bb4c2";
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
