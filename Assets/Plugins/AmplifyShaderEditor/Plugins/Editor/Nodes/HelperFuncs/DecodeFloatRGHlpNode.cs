


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Decode Float RG"
#else
"解码浮点RG"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Decodes a previously-encoded RG float"
#else
"对先前编码的RG浮点进行解码"
#endif
)]
	public sealed class DecodeFloatRGHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "DecodeFloatRG";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
			m_inputPorts[ 0 ].Name = "RG";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
			m_previewShaderGUID = "1fb3121b1c8febb4dbcc2a507a2df2db";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "decodeFloatRG" + OutputId;
		}
	}
}
