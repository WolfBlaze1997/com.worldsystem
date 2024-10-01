


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Fmod"
#else
"Fmod"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Floating point remainder of x/y with the same sign as x"
#else
"x/y的浮点余数，符号与x相同"
#endif
)]
	public sealed class FmodOpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_previewShaderGUID = "65083930f9d7812479fd6ff203ad2992";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			base.BuildResults( outputId,  ref dataCollector, ignoreLocalvar );
			if( m_inputPorts[ 0 ].DataType == WirePortDataType.INT )
				m_inputA = "(float)" + m_inputA;


			if( m_inputPorts[ 1 ].DataType == WirePortDataType.INT )
				m_inputB = "(float)" + m_inputB;


			string result = "fmod( " + m_inputA + " , " + m_inputB + " )";
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
