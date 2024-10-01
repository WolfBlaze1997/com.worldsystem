


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"FWidth"
#else
"F宽度"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Sum of approximate window-space partial derivatives magnitudes (Only valid on Fragment type ports)"
#else
"近似窗口空间偏导数幅度之和（仅对片段类型端口有效）"
#endif
)]
	public sealed class FWidthOpNode : SingleInputOp
	{
		private const string FWidthErrorMsg = "Attempting to connect an FWidth to a {0} type port. It is only valid on Fragment type ports";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "fwidth";
			m_previewShaderGUID = "81ea481faaef9c8459a555479ba64df7";
			m_inputPorts[ 0 ].CreatePortRestrictions(	WirePortDataType.OBJECT,
														WirePortDataType.FLOAT ,
														WirePortDataType.FLOAT2,
														WirePortDataType.FLOAT3,
														WirePortDataType.FLOAT4,
														WirePortDataType.COLOR ,
														WirePortDataType.INT);
			
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.PortCategory == MasterNodePortCategory.Vertex ||
				dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				UIUtils.ShowMessage( UniqueId, string.Format( FWidthErrorMsg, dataCollector.PortCategory ), MessageSeverity.Error );
				return GenerateErrorValue();
			}

			return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
		}
	}
}
