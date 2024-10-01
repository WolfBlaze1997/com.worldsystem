


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Fog Params"
#else
"雾参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Lighting"
#else
"照明"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Parameters for fog calculation"
#else
"雾计算参数"
#endif
)]
	public sealed class FogParamsNode : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"Density/Sqrt(Ln(2))"
#else
"密度/平方英寸（Ln（2））"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"Density/Ln(2)"
#else
"密度/Ln（2）"
#endif
);
			ChangeOutputName( 3, 
#if !WB_LANGUAGE_CHINESE
"-1/(End-Start)"
#else
"-1/（结束-开始）"
#endif
);
			ChangeOutputName( 4, "End/(End-Start))" );
			m_value = "unity_FogParams";
			m_previewShaderGUID = "42abde3281b1848438c3b53443c91a1e";
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
