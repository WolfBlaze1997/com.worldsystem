


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes(            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Time"
#else
"时间"
#endif
/*<C!>*/,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Time"
#else
"时间"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Time in seconds with a scale multiplier"
#else
"时间（秒），带刻度倍数"
#endif
)]
	public sealed class SimpleTimeNode : ShaderVariablesNode
	{
		private const string TimeStandard = "_Time.y";
		private const string TimeSRP = "_TimeParameters.x";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Scale"
#else
"规模"
#endif
);
			m_inputPorts[ 0 ].FloatInternalData = 1;
			m_useInternalPortData = true;
			m_previewShaderGUID = "45b7107d5d11f124fad92bcb1fa53661";
			m_continuousPreviewRefresh = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			string multiplier = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string timeGlobalVar = TimeStandard;

			if( dataCollector.IsTemplate )
			{
				if( dataCollector.TemplateDataCollectorInstance.IsHDRP || dataCollector.TemplateDataCollectorInstance.IsLWRP )
					timeGlobalVar = TimeSRP;
			}

			if( multiplier == "1.0" )
				return timeGlobalVar;

			string scaledVarName = "mulTime" + OutputId;
			string scaledVarValue = timeGlobalVar + " * " + multiplier;
			dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, scaledVarName, scaledVarValue );
			return scaledVarName;
		}
	}
}
