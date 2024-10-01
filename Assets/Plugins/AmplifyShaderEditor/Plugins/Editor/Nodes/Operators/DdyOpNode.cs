


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( "DDY",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Approximate partial derivative with respect to window-space Y"
#else
"关于窗空间Y的近似偏导数"
#endif
)]
	public sealed class DdyOpNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_opName = "ddy";
			m_previewShaderGUID = "197dcc7f05339da47b6b0e681c475c5e";
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
