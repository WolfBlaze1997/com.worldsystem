


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( "DDX",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Approximate partial derivative with respect to window-space X"
#else
"关于窗空间X的近似偏导数"
#endif
)]
	public sealed class DdxOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "ddx";
			m_previewShaderGUID = "b54ea73d5568b3540977557813eb9c3c";
			m_inputPorts[ 0 ].CreatePortRestrictions( WirePortDataType.OBJECT,
														WirePortDataType.FLOAT,
														WirePortDataType.FLOAT2,
														WirePortDataType.FLOAT3,
														WirePortDataType.FLOAT4,
														WirePortDataType.COLOR,
														WirePortDataType.INT );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.IsFragmentCategory )
				return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			else
				return m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
		}
	}
}
