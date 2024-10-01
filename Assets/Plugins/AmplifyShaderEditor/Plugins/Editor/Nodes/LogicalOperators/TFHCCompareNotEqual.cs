





using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
    [Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Compare (A \u2260 B)"
#else
"比较（A\u2260 B）"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Logical Operators"
#else
"逻辑运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Check if A is not equal to B. If true return value of True else return value of False"
#else
"检查A是否不等于B。如果为真，则返回true；否则返回False"
#endif
, null, KeyCode.None, true, true, 
#if !WB_LANGUAGE_CHINESE
"Compare"
#else
"比较"
#endif
, typeof( Compare ), 
#if !WB_LANGUAGE_CHINESE
"The Four Headed Cat - @fourheadedcat"
#else
"四头猫-@fourheaddcat"
#endif
)]
    public sealed class TFHCCompareNotEqual : TFHCStub
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputPorts[ 0 ].Name = "A";
			m_inputPorts[ 1 ].Name = "B";
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"True"
#else
"没错"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"False"
#else
"错误的"
#endif
);
			m_textLabelWidth = 100;
			m_useInternalPortData = true;
			m_previewShaderGUID = "75f433376eef1ad4a881d99124e08008";
		}
		
		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			GetInputData( ref dataCollector, ignoreLocalvar );
			string strout = "(( " + m_inputDataPort0 + " != " + m_inputDataPort1 + " ) ? " + m_inputDataPort2 + " :  " + m_inputDataPort3  + " )";
			return CreateOutputLocalVariable( 0, strout, ref dataCollector );
		}
	}
}
