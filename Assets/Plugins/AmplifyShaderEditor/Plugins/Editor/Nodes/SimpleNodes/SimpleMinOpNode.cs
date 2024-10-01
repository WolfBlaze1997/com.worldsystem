


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Min"
#else
"分钟"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Minimum of two scalars or each respective component of two vectors"
#else
"至少两个标量或两个向量的每个相应分量"
#endif
)]
	public sealed class SimpleMinOpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_previewShaderGUID = "d6033298044f0f14aa9932ca46e58ce6";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			return "min( " + m_inputA + " , " + m_inputB + " )";
		}
	}
}
