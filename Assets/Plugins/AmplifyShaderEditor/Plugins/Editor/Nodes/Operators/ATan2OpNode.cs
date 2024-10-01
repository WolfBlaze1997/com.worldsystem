


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( "ATan2",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Trigonometry Operators"
#else
"三角运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Arctangent of y/x"
#else
"y/x的反正切"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"arctangent2"
#else
"圆弧切线2"
#endif
)]
	public sealed class ATan2OpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_dynamicOutputType = true;
			m_useInternalPortData = true;
			m_previewShaderGUID = "02e3ff61784e38840af6313936b6a730";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			string result = "atan2( " + m_inputA + " , " + m_inputB + " )";
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
