


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Step"
#else
"步骤"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Step function returning either zero or one"
#else
"阶跃函数返回零或一"
#endif
)]
	public sealed class StepOpNode : DynamicTypeNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_previewShaderGUID = "2c757add7f97ecd4abd9ce6ec4659697";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			return "step( " + m_inputA + " , " + m_inputB + " )";
		}
	}
}
