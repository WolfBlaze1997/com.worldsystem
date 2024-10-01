


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Time Parameters"
#else
"时间参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Time"
#else
"时间"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Time since level load"
#else
"自水平加载以来的时间"
#endif
)]
	public sealed class TimeNode : ConstVecShaderVariable
	{
		private readonly string[] SRPTime =
		{
			"( _TimeParameters.x * 0.05 )",
			"( _TimeParameters.x )",
			"( _TimeParameters.x * 2 )",
			"( _TimeParameters.x * 3 )",
		};

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, "t/20" );
			ChangeOutputName( 2, "t" );
			ChangeOutputName( 3, "t*2" );
			ChangeOutputName( 4, "t*3" );
			m_value = "_Time";
			m_previewShaderGUID = "73abc10c8d1399444827a7eeb9c24c2a";
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

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( outputId > 0 && dataCollector.IsTemplate )
			{
				if(	dataCollector.TemplateDataCollectorInstance.IsHDRP || dataCollector.TemplateDataCollectorInstance.IsLWRP )
					return SRPTime[ outputId - 1 ];
			}

			return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
		}
	}
}
