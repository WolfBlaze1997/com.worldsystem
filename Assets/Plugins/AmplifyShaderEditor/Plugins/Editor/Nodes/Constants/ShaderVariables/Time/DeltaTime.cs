


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Delta Time"
#else
"三角洲时间"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Time"
#else
"时间"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Delta time"
#else
"三角洲时间"
#endif
)]
	public sealed class DeltaTime : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"dt"
#else
"dt"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"1/dt"
#else
"1/dt"
#endif
);
			ChangeOutputName( 3, 
#if !WB_LANGUAGE_CHINESE
"smoothDt"
#else
"平滑Dt"
#endif
);
			ChangeOutputName( 4, 
#if !WB_LANGUAGE_CHINESE
"1/smoothDt"
#else
"1/平滑Dt"
#endif
);
			m_value = "unity_DeltaTime";
			m_previewShaderGUID = "9d69a693042c443498f96d6da60535eb";
			m_continuousPreviewRefresh = true;
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
