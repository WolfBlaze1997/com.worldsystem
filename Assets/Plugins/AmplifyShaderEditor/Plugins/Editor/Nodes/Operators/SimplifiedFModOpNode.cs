


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Simplified Fmod"
#else
"简化格式"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Floating point remainder of x/y"
#else
"x/y的浮点余数"
#endif
)]
	public sealed class SimplifiedFModOpNode : DynamicTypeNode
	{
		private const string FmodCustomOp = "frac({0}/{1})*{1}";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_previewShaderGUID = "2688236fb4f37ce47b81cc818c53321d";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
			{
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
			}

			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			RegisterLocalVariable( 0, string.Format( FmodCustomOp, m_inputA, m_inputB ), ref dataCollector, ( "fmodResult" + OutputId ) );
			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
