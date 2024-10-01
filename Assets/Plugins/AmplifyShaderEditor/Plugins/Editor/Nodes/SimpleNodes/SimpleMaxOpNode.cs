


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Max"
#else
"马克斯"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Maximum of two scalars or each respective component of two vectors"
#else
"最多两个标量或两个向量的每个分量"
#endif
)]
	public sealed class SimpleMaxOpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_previewShaderGUID = "79d7f2a11092ac84a95ef6823b34adf2";
		}

		public override string BuildResults( int outputId,  ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.BuildResults( outputId,  ref dataCollector, ignoreLocalvar );
			return "max( " + m_inputA + " , " + m_inputB + " )";
		}
	}
}
